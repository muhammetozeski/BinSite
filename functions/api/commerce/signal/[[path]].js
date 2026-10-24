import missions from '../../../../content/signal-missions.json' with {type:'json'};
import {accountFromRequest,hash,limit} from '../[[path]].js';
import {engineVersion,execute,consistentCandidates,analyzeTarget,finish,sameBoard} from '../../../../server/signal-engine.js';

const encoder=new TextEncoder(),day=86400000,guestCookie='binsite_signal';
const uuid=/^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
class SignalError extends Error{constructor(status,message,code='invalid_request'){super(message);this.status=status;this.code=code;}}
function json(value,status=200,cookie=null){const headers={'Content-Type':'application/json; charset=utf-8','Cache-Control':'no-store','X-Content-Type-Options':'nosniff','Referrer-Policy':'no-referrer'};if(cookie)headers['Set-Cookie']=cookie;return new Response(JSON.stringify(value),{status,headers});}
function text(value,label,max,min=1){if(typeof value!=='string')throw new SignalError(400,`${label} must be text.`);const result=value.trim().normalize('NFC');if(result.length<min||result.length>max||/[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f]/.test(result))throw new SignalError(400,`${label} must contain ${min}–${max} characters.`);return result;}
function board(value){if(!Array.isArray(value)||value.length!==9||value.some(cell=>!Number.isInteger(cell)||cell<0||cell>3))throw new SignalError(400,'A board must contain exactly nine cells numbered zero through three.','invalid_board');return value.slice();}
function program(value){const result=text(value,'Stamp program',8);if(!/^[A-D]{1,8}$/.test(result))throw new SignalError(400,'Use one to eight uppercase A–D stamps.','invalid_program');return result;}
function mappingOf(value){if(!Array.isArray(value)||value.length!==4||value.some(op=>!Number.isInteger(op)||op<0||op>7)||new Set(value).size!==4)throw new SignalError(400,'Assign four distinct operations numbered zero through seven.','invalid_mapping');return value.slice();}
function requestId(value){if(!uuid.test(value||''))throw new SignalError(400,'A fresh UUID request identifier is required.','invalid_request_id');return value;}
function challengeId(value){if(!uuid.test(value||'')&&!/^sf-0(?:0[1-9]|1\d|2[0-4])$/.test(value||''))throw new SignalError(400,'Invalid challenge identifier.','invalid_id');return value;}
function definitionOf(value){
  if(!value||typeof value!=='object'||Array.isArray(value)||!Array.isArray(value.examples)||value.examples.length<1||value.examples.length>6||!value.target)throw new SignalError(400,'Include one to six worked examples and a held-out target.','invalid_definition');
  const mapping=mappingOf(value.mapping);
  if(!Number.isInteger(value.queryLimit)||value.queryLimit<0||value.queryLimit>6)throw new SignalError(400,'Choose an experiment budget from zero through six.','invalid_budget');
  return {engineVersion,title:text(value.title,'Title',120),description:text(value.description,'Description',900),mapping,
    examples:value.examples.map(example=>{const input=board(example.input),stamps=program(example.program);return {input,program:stamps,output:execute(input,stamps,mapping)};}),
    target:{input:board(value.target.input),program:program(value.target.program)},queryLimit:value.queryLimit,
    hint:text(value.hint||'','Hint',600,0),explanation:text(value.explanation||'','Explanation',2400,0)};
}
const builtin=new Map(missions.map(value=>[value.id,{...definitionOf(value),id:value.id,parentId:null,authorAlias:'Original editorial mission',createdAt:Date.UTC(2026,9,24)}]));
function publicChallenge(value,owned=false,withdrawn=false){return {id:value.id,engineVersion,title:value.title,description:value.description,examples:value.examples,target:value.target,queryLimit:value.queryLimit,hint:value.hint,parentId:value.parentId||null,authorAlias:value.authorAlias,createdAt:value.createdAt,allowReuse:true,owned,withdrawn};}
async function bodyOf(request){
  if(!(request.headers.get('content-type')||'').toLowerCase().startsWith('application/json'))throw new SignalError(415,'Send this action as JSON.','content_type');
  const reader=request.body?.getReader();if(!reader)throw new SignalError(400,'A JSON body is required.');let size=0;const chunks=[];
  while(true){const {value,done}=await reader.read();if(done)break;size+=value.byteLength;if(size>24000){await reader.cancel();throw new SignalError(413,'The request exceeds the signal size limit.','body_limit');}chunks.push(value);}
  const bytes=new Uint8Array(size);let offset=0;for(const chunk of chunks){bytes.set(chunk,offset);offset+=chunk.byteLength;}
  try{const value=JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(bytes));if(!value||typeof value!=='object'||Array.isArray(value))throw new Error();return value;}catch{throw new SignalError(400,'A valid JSON object is required.','invalid_json');}
}
async function loadChallenge(db,id,memberId=null){
  challengeId(id);if(await db.prepare('SELECT challenge_id FROM signal_moderation WHERE challenge_id=?').bind(id).first())throw new SignalError(404,'This challenge is unavailable.','not_found');
  if(builtin.has(id))return {definition:builtin.get(id),owned:false,withdrawn:false};
  const row=await db.prepare('SELECT * FROM signal_challenges WHERE id=? AND (withdrawn_at IS NULL OR owner_id=?)').bind(id,memberId).first();
  if(!row)throw new SignalError(404,'This challenge is unavailable.','not_found');
  return {definition:JSON.parse(row.definition),owned:row.owner_id===memberId,withdrawn:row.withdrawn_at!==null};
}
function guestOf(request){const part=(request.headers.get('cookie')||'').split(';').map(value=>value.trim()).find(value=>value.startsWith(guestCookie+'='));const token=part?.slice(guestCookie.length+1);return /^[A-Za-z0-9_-]{43}$/.test(token||'')?token:null;}
function newGuest(){return btoa(String.fromCharCode(...crypto.getRandomValues(new Uint8Array(32)))).replace(/\+/g,'-').replace(/\//g,'_').replace(/=+$/g,'');}
async function replay(db,owner,action,id,digest){const row=await db.prepare('SELECT * FROM signal_requests WHERE owner_key=? AND action=? AND request_id=?').bind(owner,action,id).first();if(!row)return null;if(row.payload_hash!==digest)throw new SignalError(409,'This request identifier was already used with different input.','request_conflict');return {value:JSON.parse(row.response),status:row.status};}
async function attemptKey(id,env){const key=await crypto.subtle.importKey('raw',encoder.encode(env.COMMERCE_RATE_SALT),{name:'HMAC',hash:'SHA-256'},false,['sign']);return [...new Uint8Array(await crypto.subtle.sign('HMAC',key,encoder.encode('signal-attempt-v1|'+id)))].map(byte=>byte.toString(16).padStart(2,'0')).join('');}
function sameKey(left,right){if(!/^[a-f0-9]{64}$/.test(left||''))return false;let difference=0;for(let index=0;index<64;index++)difference|=left.charCodeAt(index)^right.charCodeAt(index);return difference===0;}
async function ownedAttempt(db,id,guestHash,request,env){if(!uuid.test(id||''))throw new SignalError(404,'This attempt is unavailable in this browser session.','not_found');const row=await db.prepare('SELECT * FROM signal_attempts WHERE id=? AND expires_at>?').bind(id,Date.now()).first();if(!row||row.guest_hash!==guestHash&&!sameKey(request.headers.get('X-Signal-Attempt-Key'),await attemptKey(id,env)))throw new SignalError(404,'This attempt is unavailable or has expired.','not_found');return row;}
async function stateOf(db,row){const actions=await db.prepare("SELECT response FROM signal_actions WHERE attempt_id=? AND kind='query' ORDER BY sequence").bind(row.id).all();return {attemptId:row.id,challenge:publicChallenge(JSON.parse(row.snapshot)),observations:actions.results.map(item=>JSON.parse(item.response).observation),queriesRemaining:row.query_limit-row.used_queries,finished:row.finished_at!==null,expiresAt:row.expires_at,result:row.result?JSON.parse(row.result):null};}

export async function onRequest({request,env}){
  let cookie=null;
  try{
    if(!env.DB||!env.COMMERCE_RATE_SALT)throw new SignalError(503,'Signal experiments are temporarily unavailable.','configuration');
    if(!['GET','POST'].includes(request.method))throw new SignalError(405,'This method is not supported.','method');
    const url=new URL(request.url),db=env.DB,parts=url.pathname.slice('/api/commerce/signal'.length).replace(/^\/+|\/+$/g,'').split('/').filter(Boolean),member=await accountFromRequest(request,db);
    let guest=guestOf(request),guestHash=guest?await hash(guest):null;
    const ip=await hash(env.COMMERCE_RATE_SALT+'|signal|'+(request.headers.get('CF-Connecting-IP')||'unknown'));
    if(request.method==='POST'&&request.headers.get('origin')!==url.origin)throw new SignalError(403,'Submit this action from the site itself.','origin');
    await limit(db,ip,'signal-ip-'+request.method,request.method==='GET'?600:180);
    await limit(db,guestHash||member?.id||ip,'signal-'+request.method,request.method==='GET'?150:60);
    if(request.method==='GET'){
      if(parts.length===1&&parts[0]==='challenges'){
        const hidden=await db.prepare('SELECT challenge_id FROM signal_moderation').all(),ids=new Set(hidden.results.map(row=>row.challenge_id));
        return json({challenges:[...builtin.values()].filter(value=>!ids.has(value.id)).map(value=>publicChallenge(value))});
      }
      if(parts.length===2&&parts[0]==='challenges'){const found=await loadChallenge(db,parts[1],member?.id);return json({challenge:publicChallenge(found.definition,found.owned,found.withdrawn)});}
      if(parts.length===2&&parts[0]==='attempts'){const row=await ownedAttempt(db,parts[1],guestHash,request,env);return json({...await stateOf(db,row),attemptKey:await attemptKey(row.id,env)});}
      throw new SignalError(404,'This signal action was not found.','not_found');
    }
    const body=await bodyOf(request),now=Date.now();
    if(parts[0]==='challenges'&&parts.length===1){
      if(!member)throw new SignalError(401,'Create or recover a Creator account before publishing a challenge.','authentication');
      if(body.allowReuse!==true)throw new SignalError(400,'Confirm public reuse and attributed forks before publishing.','reuse_permission');
      const nonce=requestId(body.requestId),parentId=body.parentId?challengeId(body.parentId):null;
      const draft=definitionOf({...body,mapping:parentId&&body.mapping===undefined?[0,1,2,3]:body.mapping}),alias=text(body.authorAlias,'Author alias',40);
      const signature=await hash(JSON.stringify({...draft,mapping:parentId&&body.mapping===undefined?null:draft.mapping,examples:draft.examples.map(example=>({input:example.input,program:example.program})),parentId,authorAlias:alias})),owner='m:'+member.id,cached=await replay(db,owner,'publish',nonce,signature);
      if(cached)return json(cached.value,cached.status);
      const inherited=parentId?(await loadChallenge(db,parentId)).definition.mapping:draft.mapping;
      if(parentId&&body.mapping!==undefined&&!sameBoard(draft.mapping,inherited))throw new SignalError(400,'A fork must preserve the source language. Create an independent challenge for a different mapping.','parent_mapping');
      const definition={...definitionOf({...draft,mapping:inherited}),id:'',parentId,authorAlias:alias,createdAt:now};
      await limit(db,member.id,'signal-publish-day',10,86400);await limit(db,ip,'signal-ip-publish-day',40,86400);
      definition.id=crypto.randomUUID();const mutation=crypto.randomUUID(),response={challenge:publicChallenge(definition,true,false)};
      const written=await db.batch([
        db.prepare(`INSERT INTO signal_challenges(id,owner_id,definition,created_at,last_write_id) SELECT ?,?,?,?,?
          WHERE (SELECT COUNT(*) FROM signal_challenges WHERE owner_id=? AND withdrawn_at IS NULL)<50
          AND NOT EXISTS(SELECT 1 FROM signal_requests WHERE owner_key=? AND action='publish' AND request_id=?)`).bind(definition.id,member.id,JSON.stringify(definition),now,mutation,member.id,owner,nonce),
        db.prepare("INSERT INTO signal_requests(owner_key,action,request_id,payload_hash,response,status,created_at) SELECT ?,'publish',?,?,?,201,? FROM signal_challenges WHERE id=? AND last_write_id=?").bind(owner,nonce,signature,JSON.stringify(response),now,definition.id,mutation)
      ]);
      if(!written[0].meta.changes){const prior=await replay(db,owner,'publish',nonce,signature);if(prior)return json(prior.value,prior.status);throw new SignalError(409,'This account already owns fifty active public challenges.','public_limit');}
      return json(response,201);
    }
    if(parts[0]==='challenges'&&parts.length===3&&parts[2]==='attempts'){
      challengeId(parts[1]);
      if(!guest){guest=newGuest();guestHash=await hash(guest);cookie=`${guestCookie}=${guest}; Path=/api/commerce/signal; Max-Age=2592000; HttpOnly; Secure; SameSite=Strict`;}
      const nonce=requestId(body.requestId),owner='g:'+guestHash,action='start:'+parts[1],signature=await hash(parts[1]),cached=await replay(db,owner,action,nonce,signature);
      if(cached)return json({...cached.value,attemptKey:await attemptKey(cached.value.attemptId,env)},cached.status,cookie);
      const found=await loadChallenge(db,parts[1]);if(found.withdrawn)throw new SignalError(404,'This challenge is unavailable.','not_found');
      await limit(db,guestHash,'signal-start-day',30,86400);await limit(db,ip,'signal-ip-start-day',100,86400);
      const id=crypto.randomUUID(),mutation=crypto.randomUUID(),definition=found.definition,candidates=consistentCandidates(definition.examples);
      if(!candidates.length)throw new Error('No valid language in mission.');
      const response={attemptId:id,challenge:publicChallenge(definition),observations:[],queriesRemaining:definition.queryLimit,finished:false,expiresAt:now+7*day,result:null};
      const inserted=await db.batch([
        db.prepare(`INSERT INTO signal_attempts(id,guest_hash,snapshot,candidates,query_limit,created_at,expires_at,last_write_id) SELECT ?,?,?,?,?,?,?,?
          WHERE NOT EXISTS(SELECT 1 FROM signal_requests WHERE owner_key=? AND action=? AND request_id=?)`).bind(id,guestHash,JSON.stringify(definition),JSON.stringify(candidates),definition.queryLimit,now,now+7*day,mutation,owner,action,nonce),
        db.prepare('INSERT INTO signal_requests(owner_key,action,request_id,payload_hash,response,status,created_at) SELECT ?,?,?,?,?,201,? FROM signal_attempts WHERE id=? AND last_write_id=?').bind(owner,action,nonce,signature,JSON.stringify(response),now,id,mutation)
      ]);
      if(!inserted[0].meta.changes){const prior=await replay(db,owner,action,nonce,signature);if(prior)return json({...prior.value,attemptKey:await attemptKey(prior.value.attemptId,env)},prior.status,cookie);throw new SignalError(409,'The attempt could not be started.','start_conflict');}
      return json({...response,attemptKey:await attemptKey(id,env)},201,cookie);
    }
    if(parts[0]==='attempts'&&parts.length===3&&['query','finish'].includes(parts[2])){
      const row=await ownedAttempt(db,parts[1],guestHash,request,env),nonce=requestId(body.requestId),kind=parts[2],definition=JSON.parse(row.snapshot);
      const payload=kind==='query'?{input:board(body.input),program:program(body.program)}:{answer:board(body.answer)},signature=await hash(JSON.stringify({kind,...payload}));
      const prior=await db.prepare('SELECT * FROM signal_actions WHERE attempt_id=? AND request_id=?').bind(row.id,nonce).first();
      if(prior){if(prior.payload_hash!==signature)throw new SignalError(409,'This request identifier was already used with different input.','request_conflict');return json(JSON.parse(prior.response));}
      if(row.finished_at!==null)throw new SignalError(409,'This attempt already has its final result. Start a separate attempt for another answer.','finished');
      const candidates=JSON.parse(row.candidates);let next=candidates,response;
      if(kind==='query'){
        if(row.used_queries>=row.query_limit)throw new SignalError(409,'The experiment budget is exhausted. Reconstruct the target from the evidence already recorded.','query_budget');
        const observation={...payload,output:execute(payload.input,payload.program,definition.mapping)};next=consistentCandidates([observation],candidates);
        const analysis=analyzeTarget(next,definition.target);response={observation,queriesRemaining:row.query_limit-row.used_queries-1,candidateCount:analysis.candidateCount,targetOutcomeCount:analysis.targetOutcomeCount};
      }else response={...finish(candidates,definition.target,payload.answer,definition.mapping),answer:payload.answer,explanation:definition.explanation};
      const mutation=crypto.randomUUID();
      const updated=await db.batch([
        db.prepare(`UPDATE signal_attempts SET candidates=?,used_queries=used_queries+?,revision=revision+1,finished_at=?,result=?,last_write_id=?
          WHERE id=? AND guest_hash=? AND revision=? AND finished_at IS NULL AND expires_at>?
          AND NOT EXISTS(SELECT 1 FROM signal_actions WHERE attempt_id=? AND request_id=?)`)
          .bind(JSON.stringify(next),kind==='query'?1:0,kind==='finish'?now:null,kind==='finish'?JSON.stringify(response):null,mutation,row.id,row.guest_hash,row.revision,now,row.id,nonce),
        db.prepare('INSERT INTO signal_actions(attempt_id,request_id,kind,payload_hash,response,sequence,created_at) SELECT id,?,?,?,?,revision,? FROM signal_attempts WHERE id=? AND last_write_id=?').bind(nonce,kind,signature,JSON.stringify(response),now,row.id,mutation)
      ]);
      if(!updated[0].meta.changes){const previous=await db.prepare('SELECT * FROM signal_actions WHERE attempt_id=? AND request_id=?').bind(row.id,nonce).first();if(previous){if(previous.payload_hash!==signature)throw new SignalError(409,'The request identifier conflicts with saved input.','request_conflict');return json(JSON.parse(previous.response));}throw new SignalError(409,'Another action changed this attempt. Reload its saved evidence before continuing.','attempt_conflict');}
      return json(response);
    }
    if(parts[0]==='challenges'&&parts.length===3&&['withdraw','report'].includes(parts[2])){
      if(!member)throw new SignalError(401,'Use a Creator account to manage or report a published challenge.','authentication');
      const found=await loadChallenge(db,parts[1],member.id);
      if(parts[2]==='withdraw'){
        if(!found.owned)throw new SignalError(403,'Only the author account can withdraw this challenge.','owner_required');
        await db.prepare('UPDATE signal_challenges SET withdrawn_at=COALESCE(withdrawn_at,?) WHERE id=? AND owner_id=?').bind(now,parts[1],member.id).run();return json({ok:true});
      }
      if(!['spam','unsafe','rights','privacy'].includes(body.reason))throw new SignalError(400,'Choose a supported report reason.','invalid_reason');
      await db.prepare(`INSERT INTO signal_reports(challenge_id,member_id,reason,note,created_at) VALUES(?,?,?,?,?)
        ON CONFLICT(challenge_id,member_id) DO UPDATE SET reason=excluded.reason,note=excluded.note,created_at=excluded.created_at,resolved_at=NULL`).bind(parts[1],member.id,body.reason,text(body.note||'','Report note',1000,0),now).run();return json({ok:true});
    }
    throw new SignalError(404,'This signal action was not found.','not_found');
  }catch(error){
    if(error instanceof SignalError||Number.isInteger(error.status)&&error.status>=400&&error.status<500)return json({error:error.message,code:error.code||'request_failed'},error.status,cookie);
    console.error('Signal service failure',{type:error?.name||'Error'});return json({error:'The signal action could not be saved. Keep your local reconstruction and retry the same request identifier or reload the saved attempt.',code:'storage_failure'},500,cookie);
  }
}
