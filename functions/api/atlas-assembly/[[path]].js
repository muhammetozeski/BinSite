import { handleAtlasAssembly } from '../../../integrations/mounted/atlas/server/handler.mjs';

export function onRequest(context) {
  return handleAtlasAssembly({
    request: context.request,
    env: { DB: context.env.DB, SITE_ORIGIN: 'https://muhammetozeski-binsite.pages.dev' }
  });
}
