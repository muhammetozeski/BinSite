import { relayMountedRequest } from '../../../integrations/mounted/interlock/server/mount-gateway.mjs';

export function onRequest(context) {
  return relayMountedRequest({ ...context, env: { INTERLOCK_GATEWAY_SECRET: context.env.INTERLOCK_GATEWAY_SECRET } });
}
