import { handleRequest } from '../../../integrations/mounted/retry/server/handler.mjs';
import { mountedEnvironment, unavailableMount } from '../../../server/mounted-environment.mjs';

export async function onRequest(context) {
  const env = await mountedEnvironment(context.env, 'retry');
  return env ? handleRequest({ ...context, env }, { basePath: '/retry-assurance' }) : unavailableMount();
}
