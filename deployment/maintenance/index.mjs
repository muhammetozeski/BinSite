import { cleanupExpiredReleaseData } from '../../integrations/mounted/release/server/maintenance.mjs';
import { cleanupExpiredAtlasData } from '../../integrations/mounted/atlas/server/maintenance.mjs';

export default {
  async scheduled(event, env) {
    const tasks = [
      ['release', () => cleanupExpiredReleaseData(env.DB, event.scheduledTime)],
      ['atlas', () => cleanupExpiredAtlasData(env.DB, event.scheduledTime)]
    ];
    const outcomes = await Promise.allSettled(tasks.map(([, cleanup]) => Promise.resolve().then(cleanup)));
    const failed = [];
    outcomes.forEach((outcome, index) => {
      const product = tasks[index][0];
      if (outcome.status === 'fulfilled') console.log('Expired product records removed', { product, changes: outcome.value.map(result => result.meta?.changes ?? 0) });
      else { failed.push(product); console.error('Scheduled product cleanup failed', { product }); }
    });
    if (failed.length) throw new Error(`Scheduled cleanup failed for: ${failed.join(', ')}.`);
  },
  fetch() {
    return new Response('Not found', { status: 404, headers: { 'Cache-Control': 'no-store' } });
  }
};
