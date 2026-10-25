import { randomBytes } from 'node:crypto';
import { readFile } from 'node:fs/promises';

const hosting = JSON.parse(await readFile(new URL('../docs/hosting.json', import.meta.url), 'utf8'));
const token = process.env.CLOUDFLARE_API_TOKEN;
if (!token) throw new Error('Cloudflare deployment credentials are required.');
let suppliedGoogle;
if (process.env.RELEASE_GOOGLE_WEB_CLIENT) {
  try { suppliedGoogle = JSON.parse(process.env.RELEASE_GOOGLE_WEB_CLIENT); }
  catch { throw new Error('The supplied Google web-client credential is not valid JSON.'); }
  if (!suppliedGoogle || typeof suppliedGoogle !== 'object' || Array.isArray(suppliedGoogle)) throw new Error('The supplied Google web-client credential is invalid.');
}
const googleId = typeof suppliedGoogle?.clientId === 'string' ? suppliedGoogle.clientId.trim() : undefined;
const googleSecret = typeof suppliedGoogle?.clientSecret === 'string' ? suppliedGoogle.clientSecret.trim() : undefined;
if (suppliedGoogle && (!googleId || !googleSecret)) throw new Error('The supplied Google web-client credential is incomplete; existing hosting credentials were preserved.');
if (Boolean(googleId) !== Boolean(googleSecret)) throw new Error('Both Google web-client credentials are required; existing hosting credentials were preserved.');
if (googleId && (!googleId.endsWith('.apps.googleusercontent.com') || /\s/.test(googleId) || googleId.length > 300 || googleSecret.length > 1000)) throw new Error('The supplied Google web-client configuration is invalid.');
async function api(path, options = {}) {
  const response = await fetch(`https://api.cloudflare.com/client/v4/accounts/${hosting.accountId}/${path}`, {
    method: options.method ?? 'GET',
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    ...(options.body ? { body: JSON.stringify(options.body) } : {})
  });
  const data = await response.json();
  if (!response.ok || !data.success) {
    const codes = (data.errors ?? []).map(error => error.code).filter(Number.isInteger).join(',');
    throw new Error(`Release Deck deployment configuration failed (${response.status}; codes: ${codes || 'unavailable'}).`);
  }
  return data.result;
}

if (process.argv.includes('--confirm-maintenance')) {
  const schedules = await api('workers/scripts/binsite-maintenance/schedules');
  const cronEntries = Array.isArray(schedules) ? schedules : schedules.schedules;
  if (!Array.isArray(cronEntries) || !cronEntries.some(item => item.cron === '17 * * * *')) throw new Error('The provider-data cleanup schedule was not confirmed.');
  await api(`pages/projects/${hosting.projectName}`, { method: 'PATCH', body: { deployment_configs: { production: { env_vars: {
    RELEASE_PROVIDER_MAINTENANCE_READY: { type: 'plain_text', value: 'true' }
  } } } } });
  console.log('Confirmed the hourly provider-data cleanup schedule.');
} else {
  const schema = await readFile(new URL('../integrations/mounted/release/schema/release-deck.sql', import.meta.url), 'utf8');
  const result = await api(`d1/database/${hosting.databaseId}/query`, { method: 'POST', body: { sql: schema } });
  if (!Array.isArray(result) || !result.length || result.some(statement => statement.success === false)) throw new Error('The additive Release Deck schema was not applied.');
  const project = await api(`pages/projects/${hosting.projectName}`);
  const existing = project.deployment_configs?.production?.env_vars?.RELEASE_TOKEN_ENCRYPTION_KEY;
  if (existing && existing.type !== 'secret_text') throw new Error('The existing release encryption binding has an unexpected type; it was preserved.');
  if (!existing) {
    await api(`pages/projects/${hosting.projectName}`, { method: 'PATCH', body: { deployment_configs: { production: { env_vars: {
      RELEASE_TOKEN_ENCRYPTION_KEY: { type: 'secret_text', value: randomBytes(32).toString('base64url') }
    } } } } });
  }
  if (googleId && googleSecret) {
    await api(`pages/projects/${hosting.projectName}`, { method: 'PATCH', body: { deployment_configs: { production: { env_vars: {
      RELEASE_GOOGLE_CLIENT_ID: { type: 'plain_text', value: googleId },
      RELEASE_GOOGLE_CLIENT_SECRET: { type: 'secret_text', value: googleSecret }
    } } } } });
  }
  console.log('Applied the additive release schema; the dedicated encryption binding is present. Supplied Google credentials were configured when present.');
}
