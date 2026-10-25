import { cleanupExpiredReleaseData } from '../../integrations/mounted/release/server/maintenance.mjs';

export default {
  async scheduled(event, env) {
    const results = await cleanupExpiredReleaseData(env.DB, event.scheduledTime);
    console.log('Expired release provider records removed', { changes: results.map(result => result.meta?.changes ?? 0) });
  },
  fetch() {
    return new Response('Not found', { status: 404, headers: { 'Cache-Control': 'no-store' } });
  }
};
