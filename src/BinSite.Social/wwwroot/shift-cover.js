const databaseName = 'binsite-shift-cover';
const storageKey = 'binsite.shift-cover.project.v1';
const maximumBytes = 2 * 1024 * 1024;

function openDatabase() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(databaseName, 1);
    request.onupgradeneeded = () => { if (!request.result.objectStoreNames.contains('projects')) request.result.createObjectStore('projects'); };
    request.onerror = () => reject(new Error('Saved rosters could not be opened.'));
    request.onblocked = () => reject(new Error('Close other Shift & Cover Desk tabs, then try again.'));
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
      transaction.onerror = () => reject(new Error('This roster could not be saved on this device.'));
      transaction.onabort = () => reject(new Error('Saving this roster was interrupted.'));
    });
  } finally { database.close(); }
}

export async function saveProject(json) {
  if (typeof json !== 'string' || new TextEncoder().encode(json).byteLength > maximumBytes) throw new Error('The project exceeds the 2 MB file limit.');
  try {
    await databaseValue(json, true);
    if (await databaseValue() !== json) throw new Error('The saved roster could not be checked.');
    return 'IndexedDB';
  } catch (databaseError) {
    try {
      localStorage.setItem(storageKey, json);
      if (localStorage.getItem(storageKey) !== json) throw new Error('The saved roster could not be checked.');
      return 'localStorage fallback';
    } catch (fallbackError) { throw new AggregateError([databaseError, fallbackError], 'Saving failed. Keep this page open and save a project file.'); }
  }
}

export async function loadProject() {
  const values = [];
  let databaseFailure, fallbackFailure;
  try { const value = await databaseValue(); if (typeof value === 'string' && value) values.push(value); } catch (error) { databaseFailure = error; }
  try { const value = localStorage.getItem(storageKey); if (value) values.push(value); } catch (error) { fallbackFailure = error; }
  if (!values.length && databaseFailure && fallbackFailure) throw new AggregateError([databaseFailure, fallbackFailure], 'Saved rosters could not be opened.');
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
