import { onRequest as handle } from '../../../integrations/mounted/stagefold/functions/api/[[path]].js';
import { mountedEnvironment, unavailableMount } from '../../../server/mounted-environment.mjs';

export async function onRequest(context) {
  const env = await mountedEnvironment(context.env, 'stagefold');
  return env ? handle({ ...context, env }) : unavailableMount();
}
