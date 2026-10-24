import { randomBytes } from 'node:crypto';
import { readFile } from 'node:fs/promises';

// Only inserts this migration's two keys. Never round-trip redacted secret values.
const hosting = JSON.parse(await readFile(new URL('../docs/hosting.json', import.meta.url), 'utf8'));
const token = process.env.CLOUDFLARE_API_TOKEN;
if (!token) throw new Error('Cloudflare deployment credentials are required.');
const targets = [
  { name: hosting.projectName, key: 'INTERLOCK_GATEWAY_SECRET' },
  { name: 'muhammetozeski-interlock-expeditions', key: 'TRUSTED_GATEWAY_SECRET' }
];
const endpoint = name => `https://api.cloudflare.com/client/v4/accounts/${hosting.accountId}/pages/projects/${name}`;
async function api(name, method = 'GET', body) {
  const response = await fetch(endpoint(name), {
    method,
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
    ...(body ? { body: JSON.stringify(body) } : {})
  });
  const result = await response.json();
  if (!response.ok || !result.success) throw new Error(`Gateway configuration ${method} failed for ${name} (${response.status}).`);
  return result.result;
}
const projects = await Promise.all(targets.map(target => api(target.name)));
if (targets.every((target, index) => projects[index].deployment_configs?.production?.env_vars?.[target.key]?.type === 'secret_text')) {
  console.log('Both gateway secret bindings already exist; no secrets were changed.');
} else {
  const secret = randomBytes(48).toString('base64url');
  for (const target of targets) {
    await api(target.name, 'PATCH', { deployment_configs: { production: { env_vars: {
      [target.key]: { type: 'secret_text', value: secret }
    } } } });
  }
  console.log('Configured the two gateway bindings. Existing private salts and provider credentials were preserved.');
}
