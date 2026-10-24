const databaseName = 'binsite-shift-cover';
const storageKey = 'binsite.shift-cover.project.v1';
const maximumBytes = 2 * 1024 * 1024;

function openDatabase() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(databaseName, 1);
    request.onupgradeneeded = () => { if (!request.result.objectStoreNames.contains('projects')) request.result.createObjectStore('projects'); };
    request.onerror = () => reject(new Error('IndexedDB could not be opened.'));
    request.onblocked = () => reject(new Error('IndexedDB is blocked by another page.'));
    request.onsuccess = () => resolve(request.result);
  });
}

async function databaseValue(value, write = false) {
  const database = await openDatabase();
  try {
    return await new Promise((resolve, reject) => {
      const transaction = database.transaction('projects', write ? 'readwrite' : 'readonly');
      const request = write ? transaction.objectStore('projects').put(value, 'current') : transaction.objectStore('projects').get('current');
      let result;
      request.onsuccess = () => { result = request.result; };
      transaction.oncomplete = () => resolve(write ? undefined : result);
      transaction.onerror = () => reject(new Error('The local database transaction failed.'));
      transaction.onabort = () => reject(new Error('The local database transaction was aborted.'));
    });
  } finally { database.close(); }
}

export async function saveProject(json) {
  if (typeof json !== 'string' || new TextEncoder().encode(json).byteLength > maximumBytes) throw new Error('The project exceeds the local JSON limit.');
  try {
    await databaseValue(json, true);
    if (await databaseValue() !== json) throw new Error('The database write could not be verified.');
    return 'IndexedDB';
  } catch (databaseError) {
    try {
      localStorage.setItem(storageKey, json);
      if (localStorage.getItem(storageKey) !== json) throw new Error('The fallback write could not be verified.');
      return 'localStorage fallback';
    } catch (fallbackError) { throw new AggregateError([databaseError, fallbackError], 'Local saving failed; keep this page open and download a JSON backup.'); }
  }
}

export async function loadProject() {
  const values = [];
  let databaseFailure, fallbackFailure;
  try { const value = await databaseValue(); if (typeof value === 'string' && value) values.push(value); } catch (error) { databaseFailure = error; }
  try { const value = localStorage.getItem(storageKey); if (value) values.push(value); } catch (error) { fallbackFailure = error; }
  if (!values.length && databaseFailure && fallbackFailure) throw new AggregateError([databaseFailure, fallbackFailure], 'Local storage could not be read.');
  const timestamp = value => { try { const time = Date.parse(JSON.parse(value).updatedAt); return Number.isFinite(time) ? time : 0; } catch { return 0; } };
  values.sort((first, second) => timestamp(second) - timestamp(first));
  return values[0] ?? null;
}

export function downloadText(contents, filename, contentType) {
  if (typeof contents !== 'string' || !/^[a-z0-9-]+\.(json|csv|ics|html)$/.test(filename)) throw new Error('Invalid export data.');
  const url = URL.createObjectURL(new Blob([contents], { type: contentType }));
  const link = document.createElement('a');
  try { link.href = url; link.download = filename; document.body.append(link); link.click(); }
  finally { link.remove(); setTimeout(() => URL.revokeObjectURL(url), 1500); }
}
