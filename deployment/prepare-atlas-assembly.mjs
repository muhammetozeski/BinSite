import { readFile } from 'node:fs/promises';

const token = process.env.CLOUDFLARE_API_TOKEN;
if (!token) throw new Error('Cloudflare deployment credentials are required.');
const hosting = JSON.parse(await readFile(new URL('../docs/hosting.json', import.meta.url), 'utf8'));
const schema = await readFile(new URL('../integrations/mounted/atlas/schema/atlas-assembly.sql', import.meta.url), 'utf8');
const response = await fetch(`https://api.cloudflare.com/client/v4/accounts/${hosting.accountId}/d1/database/${hosting.databaseId}/query`, {
  method: 'POST',
  headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
  body: JSON.stringify({ sql: schema })
});
const data = await response.json();
if (!response.ok || data.success !== true) {
  const codes = (data.errors ?? []).map(error => error.code).filter(Number.isInteger).join(',');
  throw new Error(`Atlas room storage setup failed (${response.status}; codes: ${codes || 'unavailable'}).`);
}
if (!Array.isArray(data.result) || !data.result.length || data.result.some(result => result.success !== true)) throw new Error('The additive puzzle-room schema was not confirmed.');
console.log('Applied the additive Atlas Assembly room schema.');
