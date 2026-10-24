export async function onRequestGet({ env }) {
  const publisher = /^ca-pub-\d{16}$/.test(env.ADSENSE_CLIENT || '') ? env.ADSENSE_CLIENT : null;
  const slot = /^\d{5,20}$/.test(env.ADSENSE_SLOT || '') ? env.ADSENSE_SLOT : null;
  const adsEnabled = Boolean(publisher && slot && env.ADSENSE_CONSENT_READY === 'true');
  return Response.json({
    adSense: { enabled: adsEnabled, client: adsEnabled ? publisher : null, slot: adsEnabled ? slot : null },
    membership: '/uyelik/',
    sponsorship: '/reklam/',
    privacy: '/gizlilik/'
  }, { headers: { 'Cache-Control': 'public, max-age=300', 'X-Content-Type-Options': 'nosniff' } });
}
