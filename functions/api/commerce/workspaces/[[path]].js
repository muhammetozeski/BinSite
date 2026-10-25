import { accountFromRequest, accountStatus, hash, limit } from '../[[path]].js';

const encoder = new TextEncoder();
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
const kinds = new Set(['catalog', 'review', 'field']);
const accessSql = `SELECT w.*, CASE WHEN w.owner_id = ? THEN 'owner' ELSE m.role END AS role
 FROM commerce_workspaces w LEFT JOIN commerce_workspace_members m ON m.workspace_id=w.id AND m.member_id=?
 WHERE w.id=? AND (w.owner_id=? OR m.member_id IS NOT NULL) AND (w.archived_at IS NULL OR w.owner_id=?)`;

class WorkspaceError extends Error {
  constructor(status, message, code = 'workspace_error') { super(message); this.status = status; this.code = code; }
}
function result(value, status = 200) {
  return Response.json(value, { status, headers: { 'Cache-Control': 'no-store, private', 'X-Content-Type-Options': 'nosniff', 'Referrer-Policy': 'no-referrer' } });
}
function validId(value) {
  if (!UUID.test(value || '')) throw new WorkspaceError(400, 'Invalid workspace identifier.', 'invalid_id');
  return value;
}
function titleOf(value) {
  if (typeof value !== 'string' || !value.trim() || value.trim().length > 100 || /[\u0000-\u001f\u007f]/.test(value)) throw new WorkspaceError(400, 'A title between 1 and 100 characters is required.', 'invalid_title');
  return value.trim();
}
function documentOf(value, maximum) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) throw new WorkspaceError(400, 'The workspace document must be a JSON object.', 'invalid_document');
  const serialized = JSON.stringify(value);
  if (encoder.encode(serialized).length > maximum) throw new WorkspaceError(413, `This account allows ${maximum / 1024} KB per workspace document. Export the full project locally instead.`, 'document_limit');
  return serialized;
}
async function readBody(request) {
  if (!(request.headers.get('content-type') || '').toLowerCase().startsWith('application/json')) throw new WorkspaceError(415, 'Send a JSON request.', 'content_type');
  const reader = request.body?.getReader();
  if (!reader) throw new WorkspaceError(400, 'A request body is required.', 'missing_body');
  const chunks = []; let size = 0;
  while (true) {
    const { value, done } = await reader.read(); if (done) break;
    size += value.byteLength;
    if (size > 550000) { await reader.cancel(); throw new WorkspaceError(413, 'The request is too large.', 'body_limit'); }
    chunks.push(value);
  }
  const data = new Uint8Array(size); let offset = 0;
  for (const chunk of chunks) { data.set(chunk, offset); offset += chunk.byteLength; }
  try { const body = JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(data)); if (!body || typeof body !== 'object' || Array.isArray(body)) throw new Error(); return body; }
  catch { throw new WorkspaceError(400, 'A valid JSON object is required.', 'invalid_json'); }
}
async function access(db, id, memberId, ownerOnly = false) {
  const workspace = await db.prepare(accessSql).bind(memberId, memberId, validId(id), memberId, memberId).first();
  if (!workspace) throw new WorkspaceError(404, 'The workspace was not found or is not shared with this account.', 'not_found');
  if (ownerOnly && workspace.role !== 'owner') throw new WorkspaceError(403, 'Only the workspace owner can do this.', 'owner_required');
  return workspace;
}
function publicWorkspace(row) {
  return { id: row.id, title: row.title, kind: row.kind, body: JSON.parse(row.body), revision: row.revision, role: row.role || 'owner', createdAt: row.created_at, updatedAt: row.updated_at, archived: row.archived_at !== null };
}
async function limits(db, member) {
  const account = await accountStatus(db, member);
  return { ...account, maxBodyBytes: account.premium ? 524288 : 65536, memberLimit: account.premium ? 20 : 2, historyLimit: account.premium ? 100 : 10 };
}
function inviteToken() { return 'ws-' + [...crypto.getRandomValues(new Uint8Array(32))].map(byte => byte.toString(16).padStart(2, '0')).join(''); }

export async function onRequest({ request, env }) {
  try {
    if (!env.DB) throw new WorkspaceError(503, 'Workspace storage is unavailable.', 'database_unavailable');
    if (!['GET','POST'].includes(request.method)) throw new WorkspaceError(405, 'This method is not supported.', 'method');
    const url = new URL(request.url);
    if (request.method === 'POST' && request.headers.get('origin') !== url.origin) throw new WorkspaceError(403, 'Submit this action from the site itself.', 'origin');
    const member = await accountFromRequest(request, env.DB);
    if (!member) throw new WorkspaceError(401, 'Create an account or recover your account before using cloud workspaces.', 'authentication');
    await limit(env.DB, member.id, request.method === 'GET' ? 'workspace-read' : 'workspace-write', request.method === 'GET' ? 120 : 40);
    const suffix = url.pathname.replace(/^\/api\/commerce\/workspaces\/?/, '').replace(/\/+$/, '');
    const parts = suffix ? suffix.split('/') : [];
    const db = env.DB;
    if (request.method === 'GET') {
      if (!parts.length) {
        const rows = await db.prepare(`SELECT w.id,w.title,w.kind,w.revision,w.updated_at AS updatedAt,w.archived_at AS archivedAt,
          CASE WHEN w.owner_id=? THEN 'owner' ELSE m.role END AS role
          FROM commerce_workspaces w LEFT JOIN commerce_workspace_members m ON m.workspace_id=w.id AND m.member_id=?
          WHERE (w.owner_id=? OR m.member_id IS NOT NULL) AND (w.archived_at IS NULL OR (w.owner_id=? AND ?=1)) ORDER BY w.updated_at DESC LIMIT 300`)
          .bind(member.id,member.id,member.id,member.id,url.searchParams.get('archived')==='true'?1:0).all();
        return result({ workspaces: rows.results, account: await limits(db, member) });
      }
      const row = await access(db,parts[0],member.id);
      if (parts.length === 1) return result({workspace:publicWorkspace(row)});
      if (parts[1] === 'members' && parts.length === 2) {
        if (row.role !== 'owner') throw new WorkspaceError(403,'Only the owner can view the access list.','owner_required');
        const members = await db.prepare('SELECT member_id AS id,role,created_at AS joinedAt FROM commerce_workspace_members WHERE workspace_id=? ORDER BY created_at').bind(row.id).all();
        return result({members:members.results,ownerId:row.owner_id});
      }
      if (parts[1] === 'history') {
        if (row.role === 'viewer') throw new WorkspaceError(403,'Version history requires owner or editor access.','editor_required');
        if (parts.length === 2) {
          const versions = await db.prepare('SELECT revision,title,created_at AS savedAt,author_id AS authorId FROM commerce_workspace_versions WHERE workspace_id=? ORDER BY revision DESC LIMIT 100').bind(row.id).all();
          return result({versions:versions.results});
        }
        if (parts.length === 3 && /^\d+$/.test(parts[2])) {
          const version = await db.prepare('SELECT revision,title,body,created_at AS savedAt FROM commerce_workspace_versions WHERE workspace_id=? AND revision=?').bind(row.id,Number(parts[2])).first();
          if (!version) throw new WorkspaceError(404,'That saved version is unavailable.','version_missing');
          return result({version:{...version,body:JSON.parse(version.body)}});
        }
      }
      throw new WorkspaceError(404,'The workspace action was not found.','not_found');
    }
    const body = await readBody(request), now = Date.now();
    if (!parts.length) {
      if (!kinds.has(body.kind)) throw new WorkspaceError(400,'Choose a catalog, review, or field workspace.','invalid_kind');
      const account = await limits(db,member), title=titleOf(body.title), document=documentOf(body.body,account.maxBodyBytes), id=crypto.randomUUID(), mutation=crypto.randomUUID();
      const created = await db.batch([
        db.prepare(`INSERT INTO commerce_workspaces(id,owner_id,kind,title,body,revision,created_at,updated_at,last_write_id)
          SELECT ?,?,?,?,?,1,?,?,? WHERE
          (SELECT COUNT(*) FROM commerce_projects WHERE member_id=?)+(SELECT COUNT(*) FROM commerce_workspaces WHERE owner_id=? AND archived_at IS NULL)+(SELECT COUNT(*) FROM release_workspaces WHERE owner_id=? AND archived_at IS NULL)<?`)
          .bind(id,member.id,body.kind,title,document,now,now,mutation,member.id,member.id,member.id,account.projectLimit),
        db.prepare('INSERT INTO commerce_workspace_versions(workspace_id,revision,title,body,author_id,created_at) SELECT id,revision,title,body,owner_id,updated_at FROM commerce_workspaces WHERE id=? AND last_write_id=?').bind(id,mutation)
      ]);
      if (!created[0].meta.changes) throw new WorkspaceError(409,'The shared project and workspace allowance has been reached.','project_limit');
      return result({workspace:publicWorkspace(await access(db,id,member.id))},201);
    }
    if (parts[0] === 'join' && parts.length === 1) {
      if (typeof body.token !== 'string' || !/^ws-[a-f0-9]{64}$/.test(body.token)) throw new WorkspaceError(400,'Invalid invitation token.','invalid_invite');
      const tokenHash=await hash(body.token), invite=await db.prepare('SELECT i.*,w.owner_id,w.archived_at FROM commerce_workspace_invites i JOIN commerce_workspaces w ON w.id=i.workspace_id WHERE token_hash=?').bind(tokenHash).first();
      if (!invite || invite.expires_at<=now || invite.archived_at!==null) throw new WorkspaceError(404,'This invitation has expired or is unavailable.','invite_expired');
      if (invite.owner_id===member.id) throw new WorkspaceError(409,'You already own this workspace.','already_owner');
      if (invite.claimed_by) {
        const current=await db.prepare('SELECT role FROM commerce_workspace_members WHERE workspace_id=? AND member_id=?').bind(invite.workspace_id,member.id).first();
        if(invite.claimed_by===member.id&&current)return result({workspaceId:invite.workspace_id,role:current.role});
        throw new WorkspaceError(409,'This invitation has already been used.','invite_used');
      }
      const existing=await db.prepare('SELECT role FROM commerce_workspace_members WHERE workspace_id=? AND member_id=?').bind(invite.workspace_id,member.id).first();
      if(existing)return result({workspaceId:invite.workspace_id,role:existing.role});
      const ownerLimits=await limits(db,{id:invite.owner_id}),claim=crypto.randomUUID();
      const joined=await db.batch([
        db.prepare(`UPDATE commerce_workspace_invites SET claimed_by=?,claim_id=? WHERE token_hash=? AND claimed_by IS NULL AND expires_at>? AND
          EXISTS(SELECT 1 FROM commerce_workspaces WHERE id=? AND archived_at IS NULL) AND
          (SELECT COUNT(*) FROM commerce_workspace_members WHERE workspace_id=?)<?`)
          .bind(member.id,claim,tokenHash,now,invite.workspace_id,invite.workspace_id,ownerLimits.memberLimit),
        db.prepare(`INSERT OR IGNORE INTO commerce_workspace_members(workspace_id,member_id,role,created_at)
          SELECT workspace_id,?,role,? FROM commerce_workspace_invites WHERE token_hash=? AND claimed_by=? AND claim_id=?`)
          .bind(member.id,now,tokenHash,member.id,claim)
      ]);
      if(!joined[0].meta.changes||!joined[1].meta.changes)throw new WorkspaceError(409,'The invitation was used or the workspace member allowance is full.','invite_unavailable');
      return result({workspaceId:invite.workspace_id,role:invite.role});
    }
    const row=await access(db,parts[0],member.id);
    if(parts.length===1){
      if(row.archived_at!==null)throw new WorkspaceError(409,'Restore the workspace before editing it.','archived');
      if(row.role==='viewer')throw new WorkspaceError(403,'This account has read-only access.','editor_required');
      if(!Number.isSafeInteger(body.revision)||body.revision<1)throw new WorkspaceError(400,'Include the revision you edited.','invalid_revision');
      const account=await limits(db,{id:row.owner_id}),title=titleOf(body.title),document=documentOf(body.body,account.maxBodyBytes),mutation=crypto.randomUUID();
      const saved=await db.batch([
        db.prepare(`UPDATE commerce_workspaces SET title=?,body=?,revision=revision+1,updated_at=?,last_write_id=? WHERE id=? AND revision=? AND archived_at IS NULL
          AND (owner_id=? OR EXISTS(SELECT 1 FROM commerce_workspace_members WHERE workspace_id=? AND member_id=? AND role='editor'))`)
          .bind(title,document,now,mutation,row.id,body.revision,member.id,row.id,member.id),
        db.prepare('INSERT INTO commerce_workspace_versions(workspace_id,revision,title,body,author_id,created_at) SELECT id,revision,title,body,?,updated_at FROM commerce_workspaces WHERE id=? AND last_write_id=?').bind(member.id,row.id,mutation),
        db.prepare('DELETE FROM commerce_workspace_versions WHERE workspace_id=? AND revision<=(SELECT revision-? FROM commerce_workspaces WHERE id=? AND last_write_id=?)').bind(row.id,account.historyLimit,row.id,mutation)
      ]);
      if(!saved[0].meta.changes)throw new WorkspaceError(409,'A newer revision exists or your edit permission changed. Reload before saving.','revision_conflict');
      return result({workspace:publicWorkspace(await access(db,row.id,member.id))});
    }
    if(row.role!=='owner')throw new WorkspaceError(403,'Only the workspace owner can manage sharing or archiving.','owner_required');
    if(parts[1]==='invites'&&parts.length===2){
      if(row.archived_at!==null)throw new WorkspaceError(409,'Archived workspaces cannot be shared.','archived');
      if(!['viewer','editor'].includes(body.role))throw new WorkspaceError(400,'Choose viewer or editor access.','invalid_role');
      await limit(db,member.id,'workspace-invite',20,3600);
      const token=inviteToken(),expiresAt=now+7*86400000;
      await db.prepare('INSERT INTO commerce_workspace_invites(token_hash,workspace_id,role,created_by,created_at,expires_at) VALUES(?,?,?,?,?,?)').bind(await hash(token),row.id,body.role,member.id,now,expiresAt).run();
      return result({token,expiresAt},201);
    }
    if(parts[1]==='members'&&parts.length===3){
      const target=validId(parts[2]);
      if(body.role==='remove'){await db.prepare('DELETE FROM commerce_workspace_members WHERE workspace_id=? AND member_id=?').bind(row.id,target).run();return result({ok:true});}
      if(!['viewer','editor'].includes(body.role))throw new WorkspaceError(400,'Choose viewer, editor, or remove.','invalid_role');
      const changed=await db.prepare('UPDATE commerce_workspace_members SET role=? WHERE workspace_id=? AND member_id=?').bind(body.role,row.id,target).run();
      if(!changed.meta.changes)throw new WorkspaceError(404,'That invited member was not found.','member_missing');
      return result({ok:true});
    }
    if(parts[1]==='archive'&&parts.length===2){await db.prepare('UPDATE commerce_workspaces SET archived_at=?,updated_at=? WHERE id=? AND owner_id=?').bind(now,now,row.id,member.id).run();return result({ok:true});}
    if(parts[1]==='restore'&&parts.length===2){
      if(row.archived_at===null)return result({ok:true});
      const account=await limits(db,member);
      const restored=await db.prepare(`UPDATE commerce_workspaces SET archived_at=NULL,updated_at=? WHERE id=? AND owner_id=? AND archived_at IS NOT NULL
        AND (SELECT COUNT(*) FROM commerce_projects WHERE member_id=?)+(SELECT COUNT(*) FROM commerce_workspaces WHERE owner_id=? AND archived_at IS NULL)+(SELECT COUNT(*) FROM release_workspaces WHERE owner_id=? AND archived_at IS NULL)<?`)
        .bind(now,row.id,member.id,member.id,member.id,member.id,account.projectLimit).run();
      if(!restored.meta.changes)throw new WorkspaceError(409,'Make room in your project allowance before restoring this workspace.','project_limit');
      return result({ok:true});
    }
    throw new WorkspaceError(404,'The workspace action was not found.','not_found');
  }catch(error){
    if(error instanceof WorkspaceError)return result({error:error.message,code:error.code},error.status);
    if(Number.isInteger(error?.status)&&error.status>=400&&error.status<500)return result({error:error.message,code:error.code||'request_failed'},error.status);
    console.error('workspace_api_failure',{type:error?.name||'Error'});
    return result({error:'Workspace storage could not complete the action. Try again.',code:'internal_error'},500);
  }
}
