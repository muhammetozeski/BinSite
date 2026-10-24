const cacheName = 'binsite-field-work-offline-v1';
const requiredPaths = ['/field-work-pack/', '/field-work.js', '/social.css', '/social.js', '/BinSite.Social.styles.css', '/_framework/blazor.webassembly.js'];
const allowed = url => url.origin === self.location.origin && ((url.pathname === '/field-work-pack/' && !url.search) || requiredPaths.slice(1).includes(url.pathname) || url.pathname.startsWith('/_framework/'));
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('message', event => {
    if (event.data?.type !== 'prepare' || !event.ports[0]) return;
    event.waitUntil((async () => {
        try {
            const cache = await caches.open(cacheName);
            const urls = [...new Set(event.data.urls || [])].map(value => new URL(value, self.location.origin)).filter(allowed);
            if (!requiredPaths.every(path => urls.some(url => url.pathname === path)) || !urls.some(url => url.pathname.startsWith('/_framework/') && url.pathname.endsWith('.wasm'))) throw new Error('Reconnect and reload this page, then choose Make available offline.');
            for (const url of urls) {
                const response = await fetch(new Request(url, { credentials: 'omit', cache: 'force-cache' }));
                if (!response.ok || response.redirected) throw new Error('This page could not be saved for offline use. Reconnect, reload it and try again.');
                await cache.put(url, response);
            }
            for (const url of urls) if (!await cache.match(url)) throw new Error('Offline setup was incomplete. Reconnect, reload this page and try again.');
            event.ports[0].postMessage({ ok: true, count: urls.length });
        } catch (error) { event.ports[0].postMessage({ ok: false, error: error.message }); }
    })());
});
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET' || !allowed(new URL(event.request.url))) return;
    event.respondWith((async () => {
        const cache = await caches.open(cacheName);
        try {
            const response = await fetch(event.request);
            if (response.ok && !response.redirected) await cache.put(event.request, response.clone());
            return response;
        } catch {
            const stored = await cache.match(event.request);
            if (stored) return stored;
            if (new URL(event.request.url).pathname === '/field-work-pack/') return new Response('Reconnect and open Field Work Pack, then choose Make available offline.', { status: 503, headers: { 'Content-Type': 'text/plain;charset=utf-8' } });
            throw new Error('This page is not ready for offline use.');
        }
    })());
});
