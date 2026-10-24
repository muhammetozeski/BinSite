import { readFile, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const catalog = JSON.parse(await readFile(path.join(root, 'public/catalog.json'), 'utf8'));
let variables = {};
if (process.env.CLOUDFLARE_API_TOKEN && process.env.CLOUDFLARE_ACCOUNT_ID) {
  const hosting = JSON.parse(await readFile(path.join(root, 'docs/hosting.json'), 'utf8'));
  const response = await fetch(`https://api.cloudflare.com/client/v4/accounts/${process.env.CLOUDFLARE_ACCOUNT_ID}/pages/projects/${hosting.projectName}`, { headers: { Authorization: `Bearer ${process.env.CLOUDFLARE_API_TOKEN}` } });
  const data = await response.json();
  if (!response.ok || !data.success) throw new Error('Could not read public revenue configuration from hosting.');
  variables = data.result.deployment_configs.production.env_vars || {};
}
const publisher = /^ca-pub-\d{16}$/.test(variables.ADSENSE_CLIENT?.value || '') ? variables.ADSENSE_CLIENT.value : null;
const slot = /^\d{5,20}$/.test(variables.ADSENSE_SLOT?.value || '') ? variables.ADSENSE_SLOT.value : null;
const enabled = Boolean(publisher && slot && variables.ADSENSE_CONSENT_READY?.value === 'true');
await writeFile(path.join(root, 'public/revenue-config.json'), JSON.stringify({ adSense: { enabled, client: enabled ? publisher : null, slot: enabled ? slot : null }, membership: '/uyelik/', advertising: '/advertising/', privacy: '/privacy/' }, null, 2) + '\n');
const paths = ['index.html', ...catalog.map(product => `${product.slug}/index.html`)];
for (const relative of paths) {
  const filename = path.join(root, 'public', relative);
  const html = await readFile(filename, 'utf8');
  if (html.includes('data-binsite-revenue')) continue;
  // HTML permits omitted body/html end tags. Ignore raw-text elements and comments while locating insertion points.
  const masked = html.replace(/<!--[\s\S]*?-->|<(script|style|textarea|title)\b[^>]*>[\s\S]*?<\/\1\s*>/gi, value => ' '.repeat(value.length));
  const closing = /<\/body\s*>/i.exec(masked) || /<\/html\s*>/i.exec(masked);
  const position = closing ? closing.index : html.length;
  const script = '<script src="/assets/revenue.js" data-binsite-revenue defer></script>';
  const modified = html.slice(0, position) + script + html.slice(position);
  await writeFile(filename, modified);
}
console.log(`Prepared optional revenue footer on ${paths.length} entry points; ad network loading remains disabled until configured.`);
