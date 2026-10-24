export function onRequestGet({ env }) {
  const publisher = /^ca-pub-(\d{16})$/.exec(env.ADSENSE_CLIENT || '');
  if (!publisher) return new Response('# No advertising publisher is configured.\n', { headers: { 'Content-Type': 'text/plain; charset=utf-8', 'Cache-Control': 'public, max-age=300' } });
  return new Response(`google.com, pub-${publisher[1]}, DIRECT, f08c47fec0942fa0\n`, { headers: { 'Content-Type': 'text/plain; charset=utf-8', 'Cache-Control': 'public, max-age=300' } });
}
