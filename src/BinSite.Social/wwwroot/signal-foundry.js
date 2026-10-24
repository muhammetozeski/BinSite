const prefix = 'signal-foundry-v1:';
export function read(key) { return localStorage.getItem(prefix + key); }
export function write(key, value) {
  if (typeof value !== 'string' || value.length > 50000) throw new Error('The local draft exceeds 50,000 characters. Download a project instead.');
  localStorage.setItem(prefix + key, value);
}
export async function readProject(input) {
  const file = input.files?.[0];
  if (!file) return null;
  try {
    if (file.size > 50000) throw new Error('Choose a challenge JSON smaller than 50 KB.');
    return await file.text();
  } finally { input.value = ''; }
}
export function download(name, content, mime) {
  const url = URL.createObjectURL(new Blob([content], { type: mime }));
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = name.replace(/[^A-Za-z0-9._-]/g, '-');
  anchor.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
export async function copy(text) {
  try { await navigator.clipboard.writeText(text); return true; }
  catch { return false; }
}
