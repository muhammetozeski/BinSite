import { accountFromRequest, hash, limit } from '../[[path]].js';

const encoder = new TextEncoder();
const categories = new Set(['work','creative','learning','community','everyday']);
const outcomes = new Set(['completed','partial','blocked']);
const reasons = new Set(['spam','unsafe','rights','privacy']);
const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
class ProcedureError extends Error {
  constructor(status,error,code='invalid_request') { super(error); this.status=status; this.code=code; }
}
function json(value,status=200) {
  return new Response(JSON.stringify(value),{status,headers:{'Content-Type':'application/json; charset=utf-8','Cache-Control':'no-store','X-Content-Type-Options':'nosniff','Referrer-Policy':'no-referrer'}});
}
function text(value,label,maximum,minimum=1) {
  if(typeof value!=='string')throw new ProcedureError(400,`${label} must be text.`,'invalid_field');
  const cleaned=value.trim().normalize('NFC');
  if(cleaned.length<minimum||cleaned.length>maximum||/[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f]/u.test(cleaned))throw new ProcedureError(400,`${label} must contain ${minimum}–${maximum} characters.`,'invalid_field');
  return cleaned;
}
function object(value,keys,label) {
  if(!value||typeof value!=='object'||Array.isArray(value)||Object.keys(value).some(key=>!keys.includes(key)))throw new ProcedureError(400,`${label} contains unsupported fields. Publish only procedure text, not private run data.`,'invalid_document');
  return value;
}
function list(value,maximum,label,convert) {
  if(!Array.isArray(value)||value.length>maximum)throw new ProcedureError(400,`${label} exceeds its item limit.`,'invalid_document');
  return value.map(convert);
}
function documentOf(value) {
  object(value,['formatVersion','title','summary','category','tags','inputs','artifact','steps'],'The procedure');
  if(value.formatVersion!==1||!categories.has(value.category))throw new ProcedureError(400,'Choose a supported procedure category and format.','invalid_document');
  const steps=list(value.steps,40,'Steps',step=>{
    object(step,['id','title','action','doneWhen','minutes'],'A step');
    if(typeof step.id!=='string'||!/^[A-Za-z0-9_-]{1,64}$/.test(step.id)||!Number.isInteger(step.minutes)||step.minutes<0||step.minutes>1440)throw new ProcedureError(400,'A step needs a stable identifier and minutes between zero and 1440.','invalid_step');
    return {id:step.id,title:text(step.title,'Step title',120),action:text(step.action,'Step action',2000),doneWhen:text(step.doneWhen,'Completion check',500),minutes:step.minutes};
  });
  if(!steps.length||new Set(steps.map(step=>step.id)).size!==steps.length)throw new ProcedureError(400,'Include 1–40 steps with distinct identifiers.','invalid_step');
  const tags=list(value.tags,6,'Tags',tag=>text(tag,'Tag',24));
  if(new Set(tags.map(tag=>tag.toLocaleLowerCase('en'))).size!==tags.length)throw new ProcedureError(400,'Tags must be distinct.','invalid_document');
  const document={formatVersion:1,title:text(value.title,'Title',120),summary:text(value.summary,'Summary',600),category:value.category,tags,inputs:list(value.inputs,20,'Inputs',input=>text(input,'Input',300)),artifact:text(value.artifact,'Output artifact',1200),steps};
  if(encoder.encode(JSON.stringify(document)).length>49152)throw new ProcedureError(413,'A public procedure can contain at most 48 KB of text.','document_limit');
  return document;
}
async function bodyOf(request) {
  if(!(request.headers.get('content-type')||'').toLowerCase().startsWith('application/json'))throw new ProcedureError(415,'Send this action as JSON.','content_type');
  const reader=request.body?.getReader();if(!reader)throw new ProcedureError(400,'A JSON body is required.','missing_body');
  const chunks=[];let length=0;
  while(true){const {value,done}=await reader.read();if(done)break;length+=value.byteLength;if(length>60000){await reader.cancel();throw new ProcedureError(413,'The request is too large.','body_limit');}chunks.push(value);}
  const data=new Uint8Array(length);let offset=0;for(const chunk of chunks){data.set(chunk,offset);offset+=chunk.byteLength;}
  try{const value=JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(data));if(!value||typeof value!=='object'||Array.isArray(value))throw new Error();return value;}
  catch{throw new ProcedureError(400,'A valid JSON object is required.','invalid_json');}
}
function identifier(value){if(!uuidPattern.test(value||''))throw new ProcedureError(400,'Invalid procedure identifier.','invalid_id');return value;}
function revisionOf(value){if(!Number.isInteger(value)||value<1||value>100)throw new ProcedureError(400,'A published revision between 1 and 100 is required.','invalid_revision');return value;}
function playbook(row){return {id:row.id,revision:row.revision,document:JSON.parse(row.document),parentId:row.parent_id,parentRevision:row.parent_revision,authorAlias:row.version_alias,createdAt:row.created_at,updatedAt:row.published_at,owned:row.owned===1,withdrawn:row.withdrawn_at!==null,hidden:row.hidden_at!==null};}
async function read(db,id,memberId,revision=null){
  const row=await db.prepare(`SELECT p.*,v.revision,v.document,v.author_alias AS version_alias,v.published_at,CASE WHEN p.owner_id=? THEN 1 ELSE 0 END AS owned
    FROM procedure_playbooks p JOIN procedure_versions v ON v.playbook_id=p.id AND v.revision=COALESCE(?,p.current_revision)
    WHERE p.id=? AND ((p.withdrawn_at IS NULL AND p.hidden_at IS NULL) OR p.owner_id=?)`).bind(memberId,revision,identifier(id),memberId).first();
  if(!row)throw new ProcedureError(404,'This published procedure is unavailable.','not_found');return row;
}
async function responseFor(db,row,memberId){
  const counts=await db.prepare('SELECT kind,COUNT(*) AS count FROM procedure_outcomes WHERE playbook_id=? AND revision=? GROUP BY kind').bind(row.id,row.revision).all();
  const summary={completed:0,partial:0,blocked:0};for(const entry of counts.results)summary[entry.kind]=entry.count;
  const own=memberId?await db.prepare('SELECT kind,note FROM procedure_outcomes WHERE playbook_id=? AND revision=? AND member_id=?').bind(row.id,row.revision,memberId).first():null;
  return {playbook:playbook(row),outcomes:summary,myOutcome:own};
}

export async function onRequest({request,env}) {
  try {
    if(!env.DB||!env.COMMERCE_RATE_SALT)throw new ProcedureError(503,'Procedure sharing is temporarily unavailable.','configuration');
    if(!['GET','POST'].includes(request.method))throw new ProcedureError(405,'This method is not supported.','method');
    const url=new URL(request.url),db=env.DB,member=await accountFromRequest(request,db),memberId=member?.id||null;
    const digest=await hash(env.COMMERCE_RATE_SALT+'|procedure|'+(request.headers.get('CF-Connecting-IP')||'unknown'));
    if(request.method==='POST'){
      if(request.headers.get('origin')!==url.origin)throw new ProcedureError(403,'Submit this action from the site itself.','origin');
      if(!member)throw new ProcedureError(401,'Create or recover a Creator account before publishing or submitting a public outcome.','authentication');
      await limit(db,memberId,'procedure-write',20);await limit(db,digest,'procedure-ip-write',60);
    }else await limit(db,memberId||digest,'procedure-read',120);
    const suffix=url.pathname.slice('/api/commerce/procedures'.length).replace(/^\/+|\/+$/g,''),parts=suffix?suffix.split('/'):[];
    if(request.method==='GET'){
      if(!parts.length){
        const mine=url.searchParams.get('mine')==='true';if(mine&&!member)throw new ProcedureError(401,'Sign in to list your published procedures.','authentication');
        const rawOffset=url.searchParams.get('offset')||'0';if(!/^\d{1,4}$/.test(rawOffset)||Number(rawOffset)>2000)throw new ProcedureError(400,'The listing offset is invalid.','invalid_offset');
        const search=text(url.searchParams.get('search')||'','Search',120,0),category=url.searchParams.get('category')||'';
        if(category&&!categories.has(category))throw new ProcedureError(400,'Choose a supported category.','invalid_category');
        const pattern='%'+search.replace(/[!%_]/g,value=>'!'+value)+'%';
        const found=await db.prepare(`SELECT id,title,summary,category,tags,current_revision AS revision,parent_id AS parentId,parent_revision AS parentRevision,author_alias AS authorAlias,created_at AS createdAt,updated_at AS updatedAt,CASE WHEN owner_id=? THEN 1 ELSE 0 END AS owned,withdrawn_at AS withdrawnAt,hidden_at AS hiddenAt
          FROM procedure_playbooks WHERE ((?=1 AND owner_id=?) OR (?=0 AND withdrawn_at IS NULL AND hidden_at IS NULL))
          AND (?='' OR category=?) AND (?='' OR title LIKE ? ESCAPE '!' OR summary LIKE ? ESCAPE '!' OR tags LIKE ? ESCAPE '!') ORDER BY updated_at DESC,id LIMIT 21 OFFSET ?`)
          .bind(memberId,mine?1:0,memberId,mine?1:0,category,category,search,pattern,pattern,pattern,Number(rawOffset)).all();
        return json({items:found.results.slice(0,20).map(row=>({...row,tags:JSON.parse(row.tags),owned:row.owned===1,withdrawn:row.withdrawnAt!==null,hidden:row.hiddenAt!==null,withdrawnAt:undefined,hiddenAt:undefined})),hasMore:found.results.length>20});
      }
      if(parts.length!==1)throw new ProcedureError(404,'This procedure action was not found.','not_found');
      const selected=url.searchParams.has('revision')?revisionOf(Number(url.searchParams.get('revision'))):null;
      return json(await responseFor(db,await read(db,parts[0],memberId,selected),memberId));
    }
    const body=await bodyOf(request),now=Date.now();
    if(!parts.length){
      if(body.allowReuse!==true)throw new ProcedureError(400,'Confirm permission for public copying, forking and attributed exports before publishing.','reuse_permission');
      const document=documentOf(body.document),alias=text(body.authorAlias,'Author alias',40);let parentId=null,parentRevision=null;
      if(body.parent!==null&&body.parent!==undefined){object(body.parent,['id','revision'],'The source reference');const parent=await read(db,body.parent.id,null,revisionOf(body.parent.revision));parentId=parent.id;parentRevision=parent.revision;}
      await limit(db,memberId,'procedure-publish-day',10,86400);await limit(db,digest,'procedure-publish-ip-day',40,86400);
      const id=crypto.randomUUID(),mutation=crypto.randomUUID();
      const inserted=await db.batch([
        db.prepare(`INSERT INTO procedure_playbooks(id,owner_id,author_alias,title,summary,category,tags,parent_id,parent_revision,created_at,updated_at,last_write_id)
          SELECT ?,?,?,?,?,?,?,?,?,?,?,? WHERE (SELECT COUNT(*) FROM procedure_playbooks WHERE owner_id=? AND withdrawn_at IS NULL)<50`)
          .bind(id,memberId,alias,document.title,document.summary,document.category,JSON.stringify(document.tags),parentId,parentRevision,now,now,mutation,memberId),
        db.prepare('INSERT INTO procedure_versions(playbook_id,revision,document,author_alias,published_at) SELECT id,1,?,?,? FROM procedure_playbooks WHERE id=? AND last_write_id=?').bind(JSON.stringify(document),alias,now,id,mutation)
      ]);
      if(!inserted[0].meta.changes)throw new ProcedureError(409,'This account already has fifty active public procedures. Withdraw an older one before adding another.','public_limit');
      return json({playbook:playbook(await read(db,id,memberId))},201);
    }
    const id=identifier(parts[0]);
    if(parts.length===1){
      const row=await read(db,id,memberId);if(row.owner_id!==memberId)throw new ProcedureError(403,'Only the author account can publish a new version. Fork this procedure to create your own.','owner_required');
      if(row.withdrawn_at!==null||row.hidden_at!==null)throw new ProcedureError(409,'A withdrawn or moderated procedure cannot publish another version.','unavailable');
      if(body.allowReuse!==true)throw new ProcedureError(400,'Confirm public reuse permission before publishing.','reuse_permission');
      const expected=revisionOf(body.revision),document=documentOf(body.document),alias=text(body.authorAlias,'Author alias',40),mutation=crypto.randomUUID();
      if(row.current_revision>=100)throw new ProcedureError(409,'This procedure has reached its one-hundred-version limit. Fork a new edition.','revision_limit');
      await limit(db,memberId,'procedure-publish-day',10,86400);await limit(db,digest,'procedure-publish-ip-day',40,86400);
      const updated=await db.batch([
        db.prepare(`UPDATE procedure_playbooks SET author_alias=?,title=?,summary=?,category=?,tags=?,current_revision=current_revision+1,updated_at=?,last_write_id=?
          WHERE id=? AND owner_id=? AND current_revision=? AND withdrawn_at IS NULL AND hidden_at IS NULL`).bind(alias,document.title,document.summary,document.category,JSON.stringify(document.tags),now,mutation,id,memberId,expected),
        db.prepare('INSERT INTO procedure_versions(playbook_id,revision,document,author_alias,published_at) SELECT id,current_revision,?,?,? FROM procedure_playbooks WHERE id=? AND last_write_id=?').bind(JSON.stringify(document),alias,now,id,mutation)
      ]);
      if(!updated[0].meta.changes)throw new ProcedureError(409,'A newer version is already published. Preserve your local copy, then reload or fork deliberately.','revision_conflict');
      return json({playbook:playbook(await read(db,id,memberId))});
    }
    if(parts.length!==2)throw new ProcedureError(404,'This procedure action was not found.','not_found');
    if(parts[1]==='withdraw'){
      const row=await read(db,id,memberId);if(row.owner_id!==memberId)throw new ProcedureError(403,'Only the author account can withdraw this procedure.','owner_required');
      await db.prepare('UPDATE procedure_playbooks SET withdrawn_at=COALESCE(withdrawn_at,?),updated_at=? WHERE id=? AND owner_id=?').bind(now,now,id,memberId).run();return json({ok:true});
    }
    if(parts[1]==='outcomes'){
      const revision=revisionOf(body.revision);if(!outcomes.has(body.kind))throw new ProcedureError(400,'Choose completed, partial or blocked for your own reported outcome.','invalid_outcome');
      const note=text(body.note??'','Outcome note',1000,0);
      const saved=await db.prepare(`INSERT INTO procedure_outcomes(playbook_id,revision,member_id,kind,note,updated_at)
        SELECT ?,?,?,?,?,? WHERE EXISTS(SELECT 1 FROM procedure_playbooks p JOIN procedure_versions v ON v.playbook_id=p.id WHERE p.id=? AND v.revision=? AND p.withdrawn_at IS NULL AND p.hidden_at IS NULL)
        ON CONFLICT(playbook_id,revision,member_id) DO UPDATE SET kind=excluded.kind,note=excluded.note,updated_at=excluded.updated_at`)
        .bind(id,revision,memberId,body.kind,note,now,id,revision).run();
      if(!saved.meta.changes)throw new ProcedureError(404,'This published procedure revision is unavailable.','not_found');return json({ok:true});
    }
    if(parts[1]==='report'){
      if(!reasons.has(body.reason))throw new ProcedureError(400,'Choose a supported report reason.','invalid_reason');
      const note=text(body.note??'','Report note',1000,0);
      const saved=await db.prepare(`INSERT INTO procedure_reports(playbook_id,member_id,reason,note,created_at,resolved_at)
        SELECT ?,?,?,?,?,NULL WHERE EXISTS(SELECT 1 FROM procedure_playbooks WHERE id=? AND withdrawn_at IS NULL AND hidden_at IS NULL)
        ON CONFLICT(playbook_id,member_id) DO UPDATE SET reason=excluded.reason,note=excluded.note,created_at=excluded.created_at,resolved_at=NULL`).bind(id,memberId,body.reason,note,now,id).run();
      if(!saved.meta.changes)throw new ProcedureError(404,'This published procedure is unavailable.','not_found');return json({ok:true});
    }
    throw new ProcedureError(404,'This procedure action was not found.','not_found');
  }catch(error){
    if(Number.isInteger(error.status)&&error.status>=400&&error.status<500||error instanceof ProcedureError)return json({error:error.message,code:error.code||'request_failed'},error.status);
    console.error('Procedure service failure',{type:error?.name||'Error'});return json({error:'Procedure sharing could not complete this action. Your local work remains available.',code:'storage_failure'},500);
  }
}
