const idPattern=/^[A-Za-z0-9_-]{11}$/;
const hosts=new Set(['youtube.com','www.youtube.com','m.youtube.com','music.youtube.com']);
const imageHosts=new Set(['i.ytimg.com','img.youtube.com']);
export const maximumLinks=20;
export function videoId(input){
 const value=String(input??'').trim();if(idPattern.test(value))return value;if(value.length>1000)throw new Error('Link is too long.');
 let url;try{url=new URL(/^(?:www\.|m\.|music\.)?(?:youtube\.com|youtu\.be)\//i.test(value)?'https://'+value:value);}catch{throw new Error('Use a YouTube video link or video ID.');}
 if(!['https:','http:'].includes(url.protocol)||url.username||url.password||url.port)throw new Error('Use a public YouTube video link.');
 let id=null;const parts=url.pathname.split('/').filter(Boolean);if(url.hostname==='youtu.be')id=parts[0];else if(hosts.has(url.hostname)){if(url.pathname==='/watch')id=url.searchParams.get('v');else if(['shorts','live','embed','v'].includes(parts[0]))id=parts[1];}else if(['youtube-nocookie.com','www.youtube-nocookie.com'].includes(url.hostname)&&parts[0]==='embed')id=parts[1];else throw new Error('This is not a YouTube video link.');
 if(!idPattern.test(id??''))throw new Error('This link does not point to a video.');return id;
}
export function parseLinks(input){
 if(typeof input!=='string'||input.length>16000)throw new Error('Paste up to 20 video links.');
 const values=input.trim().split(/[\s,;]+/).filter(Boolean);if(!values.length)throw new Error('Paste at least one video link.');if(values.length>maximumLinks)throw new Error('Check up to 20 links at a time.');const first=new Map();
 return values.map((value,index)=>{try{const id=videoId(value),duplicateOf=first.has(id)?first.get(id):null;if(!first.has(id))first.set(id,index+1);return{index:index+1,input:value,id,url:'https://www.youtube.com/watch?v='+id,duplicateOf,status:'pending',message:''};}catch(error){return{index:index+1,input:value,id:null,url:null,duplicateOf:null,status:'invalid',message:error.message};}});
}
export function safeThumbnail(value,id){try{const url=new URL(value);if(url.protocol!=='https:'||url.username||url.password||!imageHosts.has(url.hostname)||!url.pathname.split('/').includes(id))return null;return url.href;}catch{return null;}}
function channelUrl(value){try{const url=new URL(value);return url.protocol==='https:'&&hosts.has(url.hostname)&&!url.username&&!url.password?url.href:null;}catch{return null;}}
export async function metadata(id,signal){
 if(!idPattern.test(id))throw new Error('Invalid video ID.');const url=new URL('https://www.youtube.com/oembed');url.searchParams.set('url','https://www.youtube.com/watch?v='+id);url.searchParams.set('format','json');
 const timeout=AbortSignal.timeout(12000),requestSignal=signal?AbortSignal.any([signal,timeout]):timeout;
 try{const response=await fetch(url,{credentials:'omit',signal:requestSignal,redirect:'error',headers:{Accept:'application/json'}});const checkedAt=new Date().toISOString();if(!response.ok)return{status:response.status===404||response.status===401||response.status===403?'unavailable':'error',message:response.status===404?'Video details could not be loaded.':response.status===401||response.status===403?'YouTube did not return details for this video.':'YouTube could not answer this check.',httpStatus:response.status,checkedAt};
  const body=await response.json();if(typeof body.title!=='string'||!body.title.trim()||body.title.length>1000||typeof body.author_name!=='string'||body.author_name.length>500)return{status:'error',message:'YouTube returned incomplete video details.',httpStatus:response.status,checkedAt};
  const thumbnailUrl=safeThumbnail(body.thumbnail_url,id);return{status:'available',message:'Video details found.',httpStatus:200,checkedAt,title:body.title,channel:body.author_name,channelUrl:channelUrl(body.author_url),thumbnailUrl,width:Number.isInteger(body.thumbnail_width)&&body.thumbnail_width>0&&body.thumbnail_width<=4096?body.thumbnail_width:null,height:Number.isInteger(body.thumbnail_height)&&body.thumbnail_height>0&&body.thumbnail_height<=4096?body.thumbnail_height:null};
 }catch(error){return{status:signal?.aborted?'canceled':'error',message:signal?.aborted?'Canceled.':timeout.aborted?'YouTube took too long to answer.':'Could not load video details. Try again.',httpStatus:null,checkedAt:new Date().toISOString()};}
}
export async function inspect(input,signal){const rows=parseLinks(input),requests=new Map();let cursor=0;async function worker(){while(cursor<rows.length){const row=rows[cursor++];if(row.status==='invalid')continue;if(signal?.aborted){row.status='canceled';row.message='Canceled.';continue;}let request=requests.get(row.id);if(!request){request=metadata(row.id,signal);requests.set(row.id,request);}Object.assign(row,await request);}}await Promise.all(Array.from({length:Math.min(4,rows.length)},()=>worker()));return{rows,canceled:signal?.aborted===true,checkedAt:new Date().toISOString()};}
export function csv(rows){const keys=['input','id','url','status','httpStatus','title','channel','channelUrl','thumbnailUrl','width','height','checkedAt'];const cell=value=>{let text=String(value??'');if(/^[\s]*[=+@-]/.test(text))text="'"+text;return '"'+text.replaceAll('"','""')+'"';};return '\ufeff'+keys.map(cell).join(',')+'\r\n'+rows.map(row=>keys.map(key=>cell(row[key])).join(',')).join('\r\n');}
