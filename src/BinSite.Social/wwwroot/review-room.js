const files = new Map();
const maximumBytes = 128 * 1024 * 1024;
const types = new Map([
  ['png', 'image/png'], ['jpg', 'image/jpeg'], ['jpeg', 'image/jpeg'], ['webp', 'image/webp'], ['gif', 'image/gif'], ['avif', 'image/avif'],
  ['mp4', 'video/mp4'], ['webm', 'video/webm'], ['ogg', 'video/ogg'], ['ogv', 'video/ogg']
]);
const supported = new Set(types.values());

function mediaType(file) {
  const extension = file.name.split('.').pop()?.toLowerCase();
  const type = supported.has(file.type) ? file.type : types.get(extension);
  if (!type) throw new Error('Choose PNG, JPEG, WebP, GIF, AVIF, MP4, WebM or Ogg media.');
  if (!file.size || file.size > maximumBytes) throw new Error('Each media file must contain data and be no larger than 128 MB.');
  return type;
}

async function dimensions(file, type) {
  const url = URL.createObjectURL(file);
  try {
    if (type.startsWith('image/')) return await new Promise((resolve, reject) => {
      const image = new Image();
      image.onload = () => resolve({ width: image.naturalWidth, height: image.naturalHeight, durationSeconds: null });
      image.onerror = () => reject(new Error('The browser could not decode this image. Choose another supported image file.'));
      image.src = url;
    });
    return await new Promise((resolve, reject) => {
      const video = document.createElement('video');
      video.preload = 'metadata';
      video.onloadedmetadata = () => {
        const duration = video.duration, width = video.videoWidth, height = video.videoHeight;
        video.removeAttribute('src'); video.load();
        if (!Number.isFinite(duration) || duration <= 0 || duration > 86400 || !width || !height) { reject(new Error('Choose a playable video stream with a finite duration of at most 24 hours.')); return; }
        resolve({ width, height, durationSeconds: duration });
      };
      video.onerror = () => reject(new Error('The browser could not read this video. Try MP4, WebM or Ogg with a supported codec.'));
      video.src = url;
    });
  } finally { URL.revokeObjectURL(url); }
}

async function describe(file) {
  const mimeType = mediaType(file);
  const header = new Uint8Array(await file.slice(0, 64).arrayBuffer());
  const ascii = new TextDecoder('ascii').decode(header);
  const matches = mimeType === 'image/png' ? header[0] === 137 && ascii.slice(1, 4) === 'PNG'
    : mimeType === 'image/jpeg' ? header[0] === 255 && header[1] === 216 && header[2] === 255
    : mimeType === 'image/gif' ? ascii.startsWith('GIF87a') || ascii.startsWith('GIF89a')
    : mimeType === 'image/webp' ? ascii.startsWith('RIFF') && ascii.slice(8, 12) === 'WEBP'
    : mimeType === 'image/avif' ? ascii.slice(4, 8) === 'ftyp' && /avif|avis/.test(ascii.slice(8))
    : mimeType === 'video/mp4' ? ascii.slice(4, 8) === 'ftyp'
    : mimeType === 'video/webm' ? header[0] === 26 && header[1] === 69 && header[2] === 223 && header[3] === 163
    : mimeType === 'video/ogg' && ascii.startsWith('OggS');
  if (!matches) throw new Error('The file contents do not match a supported image or video format.');
  const fingerprint = Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256', await file.arrayBuffer())), byte => byte.toString(16).padStart(2, '0')).join('');
  const metadata = await dimensions(file, mimeType);
  return { token: crypto.randomUUID(), name: file.name, sizeBytes: file.size, mimeType, kind: mimeType.startsWith('image/') ? 'image' : 'video', fingerprint, ...metadata };
}

export async function readSelection(input) {
  const selected = Array.from(input.files || []);
  input.value = '';
  if (!selected.length) return { files: [], error: null };
  if (selected.length > 8) return { files: [], error: 'Select at most eight files at a time.' };
  const records = [];
  try {
    for (const file of selected) records.push({ file, metadata: await describe(file) });
    for (const record of records) files.set(record.metadata.token, { file: record.file, url: null });
    return { files: records.map(record => record.metadata), error: null };
  } catch (error) { return { files: [], error: error.message || 'The media selection could not be read.' }; }
}

export async function readProject(input) {
  const file = input.files?.[0]; input.value = '';
  if (!file) return { text: null, error: null };
  if (file.size > 4 * 1024 * 1024) return { text: null, error: 'Choose a project JSON file no larger than 4 MB.' };
  try { return { text: await file.text(), error: null }; }
  catch { return { text: null, error: 'The project file could not be read.' }; }
}

function objectUrl(token) {
  const record = files.get(token);
  if (!record) return null;
  record.url ||= URL.createObjectURL(record.file);
  return record.url;
}

async function emit(reference, method, value) {
  try { await reference.invokeMethodAsync(method, value); }
  catch (error) { console.error('Review Room view callback failed', error?.name || 'Error'); }
}

export function mount(host, token, kind, annotations, reference, canAnnotate) {
  const previous = host.querySelector('video');
  if (previous) { previous.pause(); previous.removeAttribute('src'); previous.load(); }
  host.replaceChildren();
  const url = token ? objectUrl(token) : null;
  if (!url) { const message = document.createElement('p'); message.textContent = 'Media is not linked on this device. Use Reconnect saved media to match the original file by size and SHA-256.'; message.style.cssText = 'padding:24px;border:1px dashed #777'; host.append(message); return; }
  if (kind === 'video') {
    const video = document.createElement('video'); video.controls = true; video.preload = 'metadata'; video.playsInline = true;
    video.style.cssText = 'display:block;width:100%;max-height:65vh;background:#111'; video.src = url;
    video.onerror = () => { const message = document.createElement('p'); message.textContent = 'This browser cannot play the selected video codec. Its saved comments remain available.'; host.append(message); };
    host.append(video);
    return;
  }
  const stage = document.createElement('div'); stage.dataset.reviewImage = 'true'; stage.style.cssText = 'position:relative;display:inline-block;max-width:100%;line-height:0';
  const image = document.createElement('img'); image.src = url; image.alt = 'Selected local delivery version'; image.draggable = false;
  image.style.cssText = 'display:block;max-width:100%;max-height:65vh;width:auto;height:auto'; stage.append(image);
  if (canAnnotate) image.addEventListener('click', event => {
    const rect = image.getBoundingClientRect(); if (!rect.width || !rect.height) return;
    emit(reference, 'CaptureImagePoint', { x: Math.max(0, Math.min(1, (event.clientX - rect.left) / rect.width)), y: Math.max(0, Math.min(1, (event.clientY - rect.top) / rect.height)) });
  });
  for (const annotation of annotations || []) {
    if (!Number.isFinite(annotation.x) || !Number.isFinite(annotation.y)) continue;
    const pin = document.createElement('button'); pin.type = 'button'; pin.dataset.commentId = annotation.id;
    pin.textContent = String(annotation.number); pin.title = `${annotation.resolved ? 'Resolved' : 'Open'} comment ${annotation.number}`;
    pin.setAttribute('aria-label', pin.title);
    pin.style.cssText = `position:absolute;left:${annotation.x * 100}%;top:${annotation.y * 100}%;transform:translate(-50%,-50%);width:28px;height:28px;padding:0;border:2px solid #fff;border-radius:0;background:${annotation.resolved ? '#00652a' : '#c60000'};color:#fff;font:bold 13px system-ui;line-height:24px;cursor:pointer`;
    pin.addEventListener('click', event => { event.stopPropagation(); emit(reference, 'SelectMarker', annotation.id); });
    stage.append(pin);
  }
  host.append(stage);
}

export function draftPoint(host, x, y) {
  const stage = host.querySelector('[data-review-image]'); if (!stage || !Number.isFinite(x) || !Number.isFinite(y)) return;
  stage.querySelector('[data-draft-marker]')?.remove();
  const marker = document.createElement('span'); marker.dataset.draftMarker = 'true'; marker.textContent = '+'; marker.setAttribute('aria-label', 'Draft comment position');
  marker.style.cssText = `position:absolute;left:${x * 100}%;top:${y * 100}%;transform:translate(-50%,-50%);width:30px;height:30px;background:#ffcf00;color:#111;border:2px solid #111;font:bold 22px/26px system-ui;pointer-events:none`;
  stage.append(marker);
}

export function videoTime(host) {
  const video = host.querySelector('video');
  if (!video || !Number.isFinite(video.currentTime)) return null;
  video.pause();
  return video.currentTime;
}

export async function focusComment(host, id, seconds) {
  const video = host.querySelector('video');
  if (video && Number.isFinite(seconds)) {
    if (!video.readyState) await new Promise((resolve, reject) => {
      const loaded = () => { video.removeEventListener('error', failed); resolve(); };
      const failed = () => { video.removeEventListener('loadedmetadata', loaded); reject(new Error('The browser could not seek in this video.')); };
      video.addEventListener('loadedmetadata', loaded, { once: true }); video.addEventListener('error', failed, { once: true });
    });
    video.pause(); video.currentTime = Math.max(0, Math.min(video.duration, seconds));
  }
  for (const pin of host.querySelectorAll('button[data-comment-id]')) pin.style.outline = pin.dataset.commentId === id ? '3px solid #ffcf00' : 'none';
}

export function discard(tokens) {
  for (const token of tokens) { const record = files.get(token); if (record?.url) URL.revokeObjectURL(record.url); files.delete(token); }
}
export function releaseAll() { discard([...files.keys()]); }
export function dispose(host) { const video = host?.querySelector('video'); if (video) { video.pause(); video.removeAttribute('src'); video.load(); } host?.replaceChildren(); releaseAll(); }

export function download(name, content, mime) {
  const url = URL.createObjectURL(new Blob([content], { type: mime }));
  const link = document.createElement('a'); link.href = url; link.download = name; document.body.append(link); link.click(); link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1500);
}

function drawExample(canvas, id, revised) {
  const context = canvas.getContext('2d');
  if (!context) throw new Error('This browser cannot generate an editorial image.');
  const width = canvas.width, height = canvas.height;
  context.fillStyle = '#ffffff'; context.fillRect(0, 0, width, height);
  context.fillStyle = '#111111'; context.fillRect(0, 0, width, 16);
  context.font = 'bold 20px system-ui'; context.fillText('EDITORIAL DEMONSTRATION · FICTIONAL DELIVERY', 40, 55);
  if (id === 'workshop') {
    context.fillStyle = '#1746c7'; context.fillRect(40, 85, revised ? 920 : 570, 180);
    context.fillStyle = '#fff'; context.font = 'bold 60px system-ui'; context.fillText('MAKE SOMETHING', 65, 165); context.fillText('TOGETHER', 65, 235);
    context.fillStyle = '#111'; context.font = revised ? 'bold 40px system-ui' : '24px system-ui'; context.fillText('TUESDAY · 18:00', revised ? 40 : 640, revised ? 340 : 450);
    context.fillStyle = '#111'; context.fillRect(40, revised ? 395 : 320, 350, 70); context.fillStyle = '#fff'; context.font = 'bold 28px system-ui'; context.fillText('PLAN YOUR EVENING', 60, revised ? 441 : 366);
    context.fillStyle = '#111'; context.font = '24px system-ui'; context.fillText('A fictional community workshop announcement.', 40, 545);
  } else if (id === 'product') {
    context.fillStyle = '#111'; context.font = revised ? 'bold 58px system-ui' : 'bold 36px system-ui'; context.fillText('MODULAR DESK KIT', 40, revised ? 145 : 160);
    const top = revised ? 225 : 165; const labels = ['Arrange', 'Separate', 'Rebuild'];
    for (let i = 0; i < 3; i++) { const x = 40 + i * 315; context.fillStyle = i === 1 ? '#111' : '#1746c7'; context.fillRect(x, top, 280, 165); context.fillStyle = '#fff'; context.font = 'bold 28px system-ui'; context.fillText(labels[i], x + 20, top + 95); }
    context.fillStyle = '#111'; context.font = '24px system-ui'; context.fillText('Fictional product card. No real product claims.', 40, 530);
  } else if (id === 'schedule') {
    context.fillStyle = '#111'; context.font = 'bold 52px system-ui'; context.fillText('AN EVENING OF MAKING', 40, 135);
    const names = ['Welcome and ideas', 'Build a small prototype', 'Show what changed'];
    const times = ['18:00', '18:30', '19:30'];
    for (let i = 0; i < 3; i++) { const y = (revised ? 230 : 220) + i * (revised ? 105 : 65); context.fillStyle = '#1746c7'; context.font = 'bold 30px system-ui'; context.fillText(times[i], 45, y); context.fillStyle = '#111'; context.font = revised ? '30px system-ui' : '40px system-ui'; context.fillText(names[i], revised ? 200 : 140, y); context.strokeStyle = '#111'; context.beginPath(); context.moveTo(40, y + 25); context.lineTo(960, y + 25); context.stroke(); }
    context.font = '20px system-ui'; context.fillText('Original editorial example; not a real event listing.', 40, 560);
  } else throw new Error('Unknown editorial example.');
  context.fillStyle = '#111'; context.font = '18px system-ui'; context.fillText(revised ? 'VERSION 2 — REVISED' : 'VERSION 1 — FIRST DELIVERY', 620, 590);
}

export async function createExample(id) {
  if (!['workshop', 'product', 'schedule'].includes(id)) return { files: [], error: 'The editorial example was not found.' };
  const records = [];
  try {
    for (let index = 0; index < 2; index++) {
      const canvas = document.createElement('canvas'); canvas.width = 1000; canvas.height = 620; drawExample(canvas, id, index === 1);
      const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/png'));
      if (!blob) throw new Error('The browser could not generate an editorial image.');
      const file = new File([blob], `${id}-v${index + 1}.png`, { type: 'image/png' }); records.push({ file, metadata: await describe(file) });
    }
    for (const record of records) files.set(record.metadata.token, { file: record.file, url: null });
    return { files: records.map(record => record.metadata), error: null };
  } catch (error) { return { files: [], error: error.message || 'The example could not be generated.' }; }
}

export function confirmReplace() { return window.confirm('Replace the current review project? Save its JSON first if you want to keep it.'); }
