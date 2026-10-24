import { readFile, readdir, stat } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import vm from 'node:vm';
import { spawnSync } from 'node:child_process';
import { productUrl } from './product-url.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
let scripts = 0;
let htmlFiles = 0;
async function inspect(directory) {
  for (const name of await readdir(directory)) {
    const file = path.join(directory, name);
    if ((await stat(file)).isDirectory()) { await inspect(file); continue; }
    if (name.endsWith('.html')) {
      htmlFiles++;
      const html = await readFile(file, 'utf8');
      if (!/<html\b[^>]*lang\s*=\s*["']en(?:-[a-z]+)?["']/i.test(html)) throw new Error(`Product documents must use English: ${file}`);
      for (const match of html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/gi)) {
        if (/\bsrc\s*=|application\/(?:ld\+)?json/i.test(match[1])) continue;
        if (/type\s*=\s*["']module["']/i.test(match[1])) {
          const result = spawnSync(process.execPath, ['--check', '--input-type=module'], { input: match[2], encoding: 'utf8' });
          if (result.status !== 0) throw new Error(`${file}: ${result.stderr}`);
        } else new vm.Script(match[2], { filename: file });
        scripts++;
      }
    } else if (/\.(?:js|mjs)$/.test(name)) {
      const result = spawnSync(process.execPath, ['--check', file], { encoding: 'utf8' });
      if (result.status !== 0) throw new Error(result.stderr);
      scripts++;
    }
  }
}
await inspect(path.join(root, 'public'));
try { await inspect(path.join(root, 'functions')); } catch (error) { if (error.code !== 'ENOENT') throw error; }
const catalog = JSON.parse(await readFile(path.join(root, 'public', 'catalog.json'), 'utf8'));
for (const entry of catalog) {
  productUrl(entry, 'https://muhammetozeski-binsite.pages.dev');
  if (!entry.url) await stat(path.join(root, 'public', entry.slug, 'index.html'));
}
console.log(`Checked ${htmlFiles} HTML files, ${scripts} script sources, ${catalog.length} product paths. Browser interactions are not covered by this syntax check.`);
