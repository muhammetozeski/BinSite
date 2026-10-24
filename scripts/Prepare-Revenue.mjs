import { readFile, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const catalog = JSON.parse(await readFile(path.join(root, 'public/catalog.json'), 'utf8'));
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
