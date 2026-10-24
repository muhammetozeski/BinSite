import { handle } from '../../../integrations/mounted/freight/server/handler.mjs';
import { mountedEnvironment, unavailableMount } from '../../../server/mounted-environment.mjs';

export async function onRequest(context) {
  const env = await mountedEnvironment(context.env, 'freight');
  return env ? handle(context.request, env) : unavailableMount();
}
