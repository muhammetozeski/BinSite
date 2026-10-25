import { handleReleaseDeck } from '../../../../integrations/mounted/release/server/handler.mjs';
import { accountFromRequest, accountStatus, hash, limit } from '../[[path]].js';

export function onRequest(context) {
  const env = {
    DB: context.env.DB,
    GOOGLE_CLIENT_ID: context.env.RELEASE_GOOGLE_CLIENT_ID,
    GOOGLE_CLIENT_SECRET: context.env.RELEASE_GOOGLE_CLIENT_SECRET,
    TOKEN_ENCRYPTION_KEY: context.env.RELEASE_TOKEN_ENCRYPTION_KEY,
    PROVIDER_MAINTENANCE_READY: context.env.RELEASE_PROVIDER_MAINTENANCE_READY,
    SITE_ORIGIN: 'https://muhammetozeski-binsite.pages.dev'
  };
  return handleReleaseDeck({ ...context, env }, { accountFromRequest, accountStatus, hash, limit });
}
