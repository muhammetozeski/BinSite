const databaseName = 'binsite-field-work-v1';
const fallbackKey = 'binsite-field-work-fallback-v1';
const maximumImageBytes = 1500000;
let databasePromise;

function database() {
    if (!databasePromise) databasePromise = new Promise((resolve, reject) => {
        const request = indexedDB.open(databaseName, 1);
        request.onupgradeneeded = () => request.result.createObjectStore('projects');
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => { databasePromise = null; reject(new Error('IndexedDB could not be opened.')); };
        request.onblocked = () => { databasePromise = null; reject(new Error('Another browser tab blocked the local database.')); };
    });
    return databasePromise;
}

async function readDatabase() {
    const db = await database();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction('projects', 'readonly');
        const request = transaction.objectStore('projects').get('workspace');
        request.onsuccess = () => resolve(typeof request.result === 'string' ? request.result : null);
        request.onerror = () => reject(new Error('The local project could not be read.'));
    });
}

export async function loadProject() {
    const candidates = [];
    try { const value = await readDatabase(); if (value) candidates.push(value); } catch { /* A small-project fallback may still be available. */ }
    try { const value = localStorage.getItem(fallbackKey); if (value) candidates.push(value); } catch { /* The page remains usable in memory when storage is denied. */ }
    candidates.sort((a, b) => timestamp(b) - timestamp(a));
    return candidates[0] || null;
}

function timestamp(raw) {
    try { return Date.parse(JSON.parse(raw).updatedAt) || 0; } catch { return 0; }
}

export async function saveProject(json) {
    try {
        const db = await database();
        await new Promise((resolve, reject) => {
            const transaction = db.transaction('projects', 'readwrite');
            transaction.objectStore('projects').put(json, 'workspace');
            transaction.oncomplete = resolve;
            transaction.onerror = transaction.onabort = () => reject(new Error('The project could not be committed to IndexedDB.'));
        });
        try { localStorage.removeItem(fallbackKey); } catch { /* IndexedDB has already committed successfully. */ }
        return 'IndexedDB';
    } catch {
        try {
            localStorage.setItem(fallbackKey, json);
            if (localStorage.getItem(fallbackKey) !== json) throw new Error();
            return 'localStorage fallback';
        } catch { throw new Error('Local storage is unavailable or full. Download project JSON before closing this page.'); }
    }
}

function decodeBase64(value) {
    const raw = atob(value), bytes = new Uint8Array(raw.length);
    for (let i = 0; i < raw.length; i++) bytes[i] = raw.charCodeAt(i);
    return bytes;
}

async function decodeImage(blob) {
    if (typeof createImageBitmap === 'function') {
        try { return await createImageBitmap(blob); } catch { /* Image decoding below supports browsers with partial bitmap support. */ }
    }
    const url = URL.createObjectURL(blob);
    try {
        const image = new Image(); image.src = url;
        await image.decode();
        return image;
    } finally { URL.revokeObjectURL(url); }
}

export async function preparePhoto(base64, contentType, fileName) {
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(contentType)) throw new Error('Choose a JPEG, PNG, or WebP image.');
    const bytes = decodeBase64(base64);
    if (!bytes.length || bytes.length > 8 * 1024 * 1024) throw new Error('A source photo may be at most 8 MB.');
    const digest = await crypto.subtle.digest('SHA-256', bytes);
    const sourceSha256 = Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('');
    const image = await decodeImage(new Blob([bytes], { type: contentType }));
    try {
        const width = image.width || image.naturalWidth, height = image.height || image.naturalHeight;
        if (!width || !height || width * height > 40000000) throw new Error('Choose an image with at most 40 million pixels.');
        const scale = Math.min(1, 1600 / width, 1600 / height), canvas = document.createElement('canvas');
        canvas.width = Math.max(1, Math.round(width * scale)); canvas.height = Math.max(1, Math.round(height * scale));
        let dataUrl = '', byteLength = 0;
        for (let attempt = 0; attempt < 5; attempt++) {
            const context = canvas.getContext('2d');
            context.fillStyle = '#ffffff'; context.fillRect(0, 0, canvas.width, canvas.height);
            context.drawImage(image, 0, 0, canvas.width, canvas.height);
            dataUrl = canvas.toDataURL('image/jpeg', attempt === 0 ? .82 : .72);
            byteLength = decodeBase64(dataUrl.split(',')[1]).length;
            if (byteLength <= maximumImageBytes) break;
            canvas.width = Math.max(1, Math.round(canvas.width * .8)); canvas.height = Math.max(1, Math.round(canvas.height * .8));
        }
        if (byteLength > maximumImageBytes) throw new Error('This image could not be reduced to the 1.5 MB evidence limit. Choose a smaller image.');
        return { fileName: String(fileName || 'selected-photo.jpg').replace(/^.*[\\/]/, '').slice(0, 200), dataUrl, contentType: 'image/jpeg', sourceSha256, width: canvas.width, height: canvas.height, byteLength };
    } finally { if (typeof image.close === 'function') image.close(); }
}

export async function validatePhoto(photo) {
    if (!photo || !/^data:image\/(jpeg|png|webp);base64,[A-Za-z0-9+/=]+$/.test(photo.dataUrl || '')) throw new Error('The imported photo has no valid embedded image.');
    const parts = photo.dataUrl.split(','), bytes = decodeBase64(parts[1]);
    if (bytes.length !== photo.byteLength || bytes.length > maximumImageBytes) throw new Error('Imported photo bytes do not match their metadata.');
    const image = await decodeImage(new Blob([bytes], { type: photo.contentType }));
    try {
        if ((image.width || image.naturalWidth) !== photo.width || (image.height || image.naturalHeight) !== photo.height) throw new Error('Imported image dimensions do not match its metadata.');
    } finally { if (typeof image.close === 'function') image.close(); }
}

export function downloadText(text, name, contentType) {
    const url = URL.createObjectURL(new Blob([text], { type: contentType + ';charset=utf-8' }));
    const anchor = document.createElement('a'); anchor.href = url; anchor.download = name; anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export async function copyText(value) {
    try { await navigator.clipboard.writeText(value); return true; } catch { return false; }
}

export async function printReport(html) {
    const tab = window.open('', '_blank');
    if (!tab) throw new Error('The browser blocked the report window. Download HTML and print that file instead.');
    tab.opener = null;
    tab.document.open(); tab.document.write(html); tab.document.close();
    await Promise.all([...tab.document.images].map(image => image.decode().catch(() => { throw new Error('A report image could not be decoded. Download the HTML report to inspect it.'); })));
    tab.focus(); tab.print();
}

export async function prepareOffline() {
    if (!('serviceWorker' in navigator) || !window.isSecureContext) throw new Error('Offline reopening requires HTTPS and service-worker support. The currently open local workflow still works without the network.');
    const registration = await navigator.serviceWorker.register('/field-work-sw.js', { scope: '/field-work-pack/' });
    const worker = registration.active || registration.waiting || registration.installing;
    if (!worker) throw new Error('The offline worker was not created. Try again after the published app is available.');
    if (worker.state !== 'activated') await new Promise((resolve, reject) => {
        const changed = () => { if (worker.state === 'activated') resolve(); if (worker.state === 'redundant') reject(new Error('The offline worker could not activate.')); };
        worker.addEventListener('statechange', changed);
        changed();
    });
    const urls = new Set(['/field-work-pack/', '/field-work.js', '/social.css', '/social.js', '/BinSite.Social.styles.css', '/_framework/blazor.webassembly.js']);
    for (const entry of performance.getEntriesByType('resource')) {
        const url = new URL(entry.name);
        if (url.origin === location.origin && url.pathname.startsWith('/_framework/')) urls.add(url.pathname + url.search);
    }
    return new Promise((resolve, reject) => {
        const channel = new MessageChannel();
        channel.port1.onmessage = event => event.data?.ok ? resolve(event.data.count) : reject(new Error(event.data?.error || 'Offline resources could not be cached.'));
        worker.postMessage({ type: 'prepare', urls: [...urls] }, [channel.port2]);
    });
}
