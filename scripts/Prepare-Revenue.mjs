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
  const modified = html.replace(/<\/body>/i, '<script src="/assets/revenue.js" data-binsite-revenue defer></script></body>');
  if (modified === html) throw new Error(`Cannot insert revenue controls: ${relative}`);
  await writeFile(filename, modified);
}
console.log(`Prepared optional revenue footer on ${paths.length} entry points; ad network loading remains disabled until configured.`);
