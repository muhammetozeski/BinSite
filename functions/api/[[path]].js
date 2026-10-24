const COOKIE_NAME = 'binsite_visitor';
const MAX_BODY_BYTES = 12000;
const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
const TABLES = { story: 'stories', node: 'story_nodes', poll: 'polls', capsule: 'capsules' };

class ApiError extends Error {
  constructor(status, message) { super(message); this.status = status; }
}

async function hash(value) {
  const bytes = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(value));
  return Array.from(new Uint8Array(bytes), byte => byte.toString(16).padStart(2, '0')).join('');
}

function textField(value, label, min, max) {
  if (typeof value !== 'string') throw new ApiError(400, `${label} must be text.`);
  const clean = value.trim().normalize('NFC');
  if (clean.length < min || clean.length > max) throw new ApiError(400, `${label} must contain ${min}–${max} characters.`);
  if (/[\u0000-\u0008\u000b\u000c\u000e-\u001f]/u.test(clean)) throw new ApiError(400, 'Unsupported control character.');
  if (/(.)\1{39,}/u.test(clean) || (clean.match(/https?:\/\//giu) || []).length > 2) {
    throw new ApiError(400, 'Reduce repeated text or excessive links.');
  }
  return clean;
}

function uuid(value) {
  if (typeof value !== 'string' || !UUID_PATTERN.test(value)) throw new ApiError(400, 'Invalid link identifier.');
  return value;
}

async function readJson(request) {
  if (!(request.headers.get('content-type') || '').toLowerCase().startsWith('application/json')) {
    throw new ApiError(415, 'Send the request as JSON.');
  }
  if (Number(request.headers.get('content-length')) > MAX_BODY_BYTES) throw new ApiError(413, 'The submitted text is too large.');
  const reader = request.body?.getReader();
  if (!reader) throw new ApiError(400, 'The request body is missing.');
  const chunks = [];
  let total = 0;
  while (true) {
    const { value, done } = await reader.read();
    if (done) break;
    total += value.byteLength;
    if (total > MAX_BODY_BYTES) { await reader.cancel(); throw new ApiError(413, 'The submitted text is too large.'); }
    chunks.push(value);
  }
  const bytes = new Uint8Array(total);
  let position = 0;
  for (const chunk of chunks) { bytes.set(chunk, position); position += chunk.byteLength; }
  try {
    const data = JSON.parse(new TextDecoder('utf-8', { fatal: true }).decode(bytes));
    if (!data || typeof data !== 'object' || Array.isArray(data)) throw new Error('Object expected');
    return data;
  } catch { throw new ApiError(400, 'The request could not be read.'); }
}

async function rateLimit(db, action, visitorHash, ipHash, maximum, seconds = 3600) {
  const now = Math.floor(Date.now() / 1000);
  const bucket = Math.floor(now / seconds);
  const expiry = (bucket + 1) * seconds;
  const statement = 'INSERT INTO social_rate_limits (rate_key, count, expires_at) VALUES (?, 1, ?) ON CONFLICT(rate_key) DO UPDATE SET count = count + 1 WHERE count < ? RETURNING count';
  const results = await db.batch([
    db.prepare('DELETE FROM social_rate_limits WHERE expires_at < ?').bind(now),
    db.prepare(statement).bind(`${action}:v:${visitorHash}:${bucket}`, expiry, maximum),
    db.prepare(statement).bind(`${action}:i:${ipHash}:${bucket}`, expiry, maximum * 3)
  ]);
  if (!results[1].results?.length || !results[2].results?.length) throw new ApiError(429, 'The short-term limit has been reached. Try again later.');
}

async function storyDetail(db, id) {
  const story = await db.prepare('SELECT id, title, created_at AS createdAt FROM stories WHERE id = ? AND hidden = 0').bind(id).first();
  if (!story) throw new ApiError(404, 'The story was not found or was hidden after reports.');
  const nodes = await db.prepare('SELECT id, parent_id AS parentId, CASE WHEN hidden = 0 THEN body ELSE NULL END AS body, depth, created_at AS createdAt, hidden FROM story_nodes WHERE story_id = ? ORDER BY created_at, id LIMIT 300').bind(id).all();
  return { story, nodes: nodes.results };
}

async function pollDetail(db, id, visitorHash) {
  const poll = await db.prepare('SELECT id, question, option_a AS optionA, option_b AS optionB, created_at AS createdAt FROM polls WHERE id = ? AND hidden = 0').bind(id).first();
  if (!poll) throw new ApiError(404, 'The duel was not found or was hidden after reports.');
  const totals = await db.prepare('SELECT COALESCE(SUM(CASE WHEN choice = 0 THEN 1 ELSE 0 END), 0) AS a, COALESCE(SUM(CASE WHEN choice = 1 THEN 1 ELSE 0 END), 0) AS b FROM poll_votes WHERE poll_id = ?').bind(id).first();
  const own = await db.prepare('SELECT choice FROM poll_votes WHERE poll_id = ? AND visitor_hash = ?').bind(id, visitorHash).first();
  return { poll, totals, myVote: own ? own.choice : null };
}

async function dispatch(request, env, visitorHash, ipHash) {
  if (!env.DB) throw new ApiError(503, 'Shared storage is currently unavailable.');
  const db = env.DB;
  const url = new URL(request.url);
  const route = url.pathname.replace(/^\/api\/?/u, '').replace(/\/+$/u, '');
  const parts = route.split('/');
  const method = request.method;
  if (!['GET', 'POST'].includes(method)) throw new ApiError(405, 'This request method is not supported.');
  if (method === 'POST' && request.headers.get('origin') !== url.origin) throw new ApiError(403, 'Submit this action from the site itself.');

  if (method === 'GET') {
    await rateLimit(db, 'read', visitorHash, ipHash, 120, 60);
    if (route === 'stories') {
      const query = (url.searchParams.get('q') || '').trim().slice(0, 80).replace(/[!%_]/gu, '!$&');
      const result = await db.prepare("SELECT s.id, s.title, s.created_at AS createdAt, n.body AS opening, (SELECT COUNT(*) FROM story_nodes c WHERE c.story_id = s.id AND c.hidden = 0) AS nodeCount FROM stories s JOIN story_nodes n ON n.story_id = s.id AND n.parent_id IS NULL AND n.hidden = 0 WHERE s.hidden = 0 AND s.title LIKE ? ESCAPE '!' ORDER BY s.created_at DESC LIMIT 30").bind(`%${query}%`).all();
      return { stories: result.results };
    }
    if (parts.length === 2 && parts[0] === 'stories') return storyDetail(db, uuid(parts[1]));
    if (route === 'polls') {
      const result = await db.prepare('SELECT p.id, p.question, p.option_a AS optionA, p.option_b AS optionB, p.created_at AS createdAt, (SELECT COUNT(*) FROM poll_votes v WHERE v.poll_id = p.id) AS voteCount FROM polls p WHERE p.hidden = 0 ORDER BY p.created_at DESC LIMIT 24').all();
      return { polls: result.results };
    }
    if (parts.length === 2 && parts[0] === 'polls') return pollDetail(db, uuid(parts[1]), visitorHash);
    if (parts.length === 2 && parts[0] === 'capsules') {
      const capsule = await db.prepare('SELECT id, title, unlock_at AS unlockAt, created_at AS createdAt, CASE WHEN unlock_at <= ? THEN body ELSE NULL END AS body FROM capsules WHERE id = ? AND hidden = 0').bind(Date.now(), uuid(parts[1])).first();
      if (!capsule) throw new ApiError(404, 'The capsule was not found or was hidden after reports.');
      return { capsule, locked: capsule.body === null, serverNow: Date.now() };
    }
    throw new ApiError(404, 'This address was not found.');
  }

  const body = await readJson(request);
  if (route === 'stories') {
    const title = textField(body.title, 'Title', 4, 100);
    const opening = textField(body.body, 'Opening paragraph', 30, 1600);
    await rateLimit(db, 'story-create', visitorHash, ipHash, 6);
    const id = crypto.randomUUID(), nodeId = crypto.randomUUID(), now = Date.now();
    await db.batch([
      db.prepare('INSERT INTO stories (id, title, creator_hash, created_at) VALUES (?, ?, ?, ?)').bind(id, title, visitorHash, now),
      db.prepare('INSERT INTO story_nodes (id, story_id, parent_id, body, depth, creator_hash, created_at) VALUES (?, ?, NULL, ?, 0, ?, ?)').bind(nodeId, id, opening, visitorHash, now)
    ]);
    return { id, nodeId };
  }
  if (parts.length === 3 && parts[0] === 'stories' && parts[2] === 'nodes') {
    const storyId = uuid(parts[1]), parentId = uuid(body.parentId);
    const paragraph = textField(body.body, 'Paragraf', 30, 1600);
    const parent = await db.prepare('SELECT n.depth FROM story_nodes n JOIN stories s ON s.id = n.story_id WHERE n.id = ? AND n.story_id = ? AND n.hidden = 0 AND s.hidden = 0').bind(parentId, storyId).first();
    if (!parent) throw new ApiError(404, 'The parent branch was not found.');
    if (parent.depth >= 24) throw new ApiError(409, 'This branch has reached 24 continuations. Branch from an earlier paragraph.');
    await rateLimit(db, 'story-node', visitorHash, ipHash, 18);
    const id = crypto.randomUUID();
    const result = await db.prepare('INSERT INTO story_nodes (id, story_id, parent_id, body, depth, creator_hash, created_at) SELECT ?, ?, ?, ?, ?, ?, ? WHERE (SELECT COUNT(*) FROM story_nodes WHERE story_id = ?) < 300').bind(id, storyId, parentId, paragraph, parent.depth + 1, visitorHash, Date.now(), storyId).run();
    if (!result.meta.changes) throw new ApiError(409, 'This story has reached 300 paragraphs. Start a new story.');
    return { id };
  }
  if (route === 'polls') {
    const question = textField(body.question, 'Duel question', 8, 240);
    const optionA = textField(body.optionA, 'First idea', 2, 140);
    const optionB = textField(body.optionB, 'Second idea', 2, 140);
    if (optionA.toLocaleLowerCase('tr') === optionB.toLocaleLowerCase('tr')) throw new ApiError(400, 'Write two different ideas.');
    await rateLimit(db, 'poll-create', visitorHash, ipHash, 6);
    const id = crypto.randomUUID();
    await db.prepare('INSERT INTO polls (id, question, option_a, option_b, creator_hash, created_at) VALUES (?, ?, ?, ?, ?, ?)').bind(id, question, optionA, optionB, visitorHash, Date.now()).run();
    return { id };
  }
  if (parts.length === 3 && parts[0] === 'polls' && parts[2] === 'votes') {
    const id = uuid(parts[1]);
    if (![0, 1].includes(body.choice)) throw new ApiError(400, 'Choose one of the two ideas.');
    await rateLimit(db, 'poll-vote', visitorHash, ipHash, 40);
    const result = await db.prepare('INSERT OR IGNORE INTO poll_votes (poll_id, visitor_hash, choice, created_at) SELECT ?, ?, ?, ? WHERE EXISTS (SELECT 1 FROM polls WHERE id = ? AND hidden = 0)').bind(id, visitorHash, body.choice, Date.now(), id).run();
    if (!result.meta.changes) {
      const existing = await db.prepare('SELECT id FROM polls WHERE id = ? AND hidden = 0').bind(id).first();
      throw new ApiError(existing ? 409 : 404, existing ? 'This browser identity has already voted in this duel.' : 'The duel was not found.');
    }
    return pollDetail(db, id, visitorHash);
  }
  if (route === 'capsules') {
    const title = textField(body.title, 'Capsule title', 3, 100);
    const message = textField(body.body, 'Message', 10, 5000);
    const unlockAt = typeof body.unlockAt === 'string' ? Date.parse(body.unlockAt) : NaN;
    const now = Date.now();
    if (!Number.isFinite(unlockAt) || unlockAt < now + 60000 || unlockAt > now + 10 * 366 * 86400000) {
      throw new ApiError(400, 'Choose an opening time at least one minute and at most ten years ahead.');
    }
    await rateLimit(db, 'capsule-create', visitorHash, ipHash, 5);
    const id = crypto.randomUUID();
    await db.prepare('INSERT INTO capsules (id, title, body, unlock_at, creator_hash, created_at) VALUES (?, ?, ?, ?, ?, ?)').bind(id, title, message, unlockAt, visitorHash, now).run();
    return { id, unlockAt, serverNow: now };
  }
  if (route === 'reports') {
    const kind = body.kind, id = uuid(body.id);
    if (!Object.hasOwn(TABLES, kind)) throw new ApiError(400, 'Invalid report type.');
    const reason = textField(body.reason, 'Report reason', 8, 300);
    const table = TABLES[kind];
    const existing = await db.prepare(`SELECT id FROM ${table} WHERE id = ? AND hidden = 0`).bind(id).first();
    if (!existing) throw new ApiError(404, 'Content was not found.');
    await rateLimit(db, 'report', visitorHash, ipHash, 10);
    await db.batch([
      db.prepare('INSERT OR IGNORE INTO social_reports (kind, target_id, ip_hash, reason, created_at) VALUES (?, ?, ?, ?, ?)').bind(kind, id, ipHash, reason, Date.now()),
      db.prepare(`UPDATE ${table} SET hidden = 1 WHERE id = ? AND (SELECT COUNT(*) FROM social_reports WHERE kind = ? AND target_id = ?) >= 3`).bind(id, kind, id)
    ]);
    return { reported: true };
  }
  throw new ApiError(404, 'This address was not found.');
}

export async function onRequest(context) {
  const { request, env } = context;
  const cookieValue = (request.headers.get('cookie') || '').split(';').map(part => part.trim()).find(part => part.startsWith(`${COOKIE_NAME}=`))?.slice(COOKIE_NAME.length + 1);
  const visitor = /^[a-f0-9]{64}$/u.test(cookieValue || '') ? cookieValue : Array.from(crypto.getRandomValues(new Uint8Array(32)), byte => byte.toString(16).padStart(2, '0')).join('');
  const secure = new URL(request.url).protocol === 'https:' ? '; Secure' : '';
  const headers = {
    'Content-Type': 'application/json; charset=utf-8',
    'Cache-Control': 'no-store, private',
    'X-Content-Type-Options': 'nosniff',
    'Referrer-Policy': 'no-referrer',
    'Set-Cookie': `${COOKIE_NAME}=${visitor}; Path=/; HttpOnly; SameSite=Strict; Max-Age=31536000${secure}`
  };
  try {
    const visitorHash = await hash(`visitor:${visitor}`);
    const ipHash = await hash(`ip:${request.headers.get('cf-connecting-ip') || 'local'}`);
    const data = await dispatch(request, env, visitorHash, ipHash);
    return new Response(JSON.stringify(data), { status: 200, headers });
  } catch (error) {
    const status = error instanceof ApiError ? error.status : 500;
    if (status === 500) console.error('social_api_failure', { name: error?.name || 'Error' });
    return new Response(JSON.stringify({ error: status === 500 ? 'The action could not be completed. Please try again.' : error.message }), { status, headers });
  }
}
