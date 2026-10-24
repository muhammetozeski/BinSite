import { readFile, writeFile, readdir, access } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const entries = [];
for (const filename of (await readdir(path.join(root, 'catalog'))).filter(name => name.endsWith('.json')).sort()) {
  const batch = JSON.parse(await readFile(path.join(root, 'catalog', filename), 'utf8'));
  if (!Array.isArray(batch)) throw new Error(`Catalog must be an array: ${filename}`);
  entries.push(...batch);
}
const ids = new Set();
const slugs = new Set();
for (const entry of entries) {
  if (!Number.isInteger(entry.id) || entry.id < 1 || ids.has(entry.id) || !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(entry.slug) || slugs.has(entry.slug)) throw new Error(`Duplicate or invalid product: ${JSON.stringify(entry)}`);
  if (!entry.name || !entry.description || !Array.isArray(entry.features) || !entry.features.length) throw new Error(`Missing product content: ${entry.slug}`);
  ids.add(entry.id); slugs.add(entry.slug);
  await access(path.join(root, 'public', entry.slug, 'index.html'));
  if (entry.guide) {
    if (entry.guide.slug !== `${entry.slug}/guide` || typeof entry.guide.title !== 'string' || !entry.guide.title.trim()) throw new Error(`Invalid product guide: ${entry.slug}`);
    await access(path.join(root, 'public', entry.guide.slug, 'index.html'));
  }
}
entries.sort((a, b) => a.id - b.id);
await writeFile(path.join(root, 'public', 'catalog.json'), JSON.stringify(entries, null, 2) + '\n');
const escape = value => String(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const cards = entries.map(entry => `<tr data-category="${escape(entry.category)}" data-search="${escape([entry.name, entry.description, ...entry.features].join(' ').toLocaleLowerCase('en-US'))}"><td><div class="eyebrow">${escape(entry.category)} · ${entry.id.toString().padStart(3, '0')}</div><h2><a href="/${entry.slug}/">${escape(entry.name)}</a></h2></td><td><p>${escape(entry.description)}</p><ul>${entry.features.map(feature => `<li>${escape(feature)}</li>`).join('')}</ul></td><td><a class="open" href="/${entry.slug}/">Open →</a>${entry.guide ? `<p><a href="/${entry.guide.slug}/">${escape(entry.guide.title)}</a></p>` : ''}</td></tr>`).join('\n');
const categories = [...new Set(entries.map(entry => entry.category))];
await writeFile(path.join(root, 'public', 'index.html'), `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="description" content="Working tools, games, creative editors and shared communities. Browse the directory and open an independent app."><title>BinSite — Tools, games and communities</title><link rel="stylesheet" href="/assets/catalog.css"></head><body>
<header><a class="brand" href="/">BinSite</a><p class="counter">${entries.length} published products · target: 1,000</p></header>
<main><section class="intro"><h1>Tools, games and communities</h1><p>Search by what you want to do. Each product opens at its own address and controls its own interface.</p></section>
<section class="find" aria-label="Find a product"><div><label for="search">Search products</label><input id="search" type="search" placeholder="Animation, audio, puzzles, writing"></div><div><label for="category">Category</label><select id="category"><option value="">All categories</option>${categories.map(category => `<option>${escape(category)}</option>`).join('')}</select></div><button id="surprise" type="button">Open a random product</button></section>
<p id="results" role="status">${entries.length} products shown.</p><table id="projects" class="projects"><thead><tr><th scope="col">Product</th><th scope="col">What it does</th><th scope="col">Launch</th></tr></thead><tbody>${cards}</tbody></table><p id="empty" hidden>No matching products. Try a different search.</p>
<section class="about"><h2>Storage and access</h2><p>Standalone tools store data in your browser or let you download a project. Shared communities use a server and explain their data rules on their own pages. The directory counts implemented product entries; traffic and revenue are reported only when measured.</p></section></main>
<footer>BinSite · Independent web products.</footer><script src="/assets/catalog.js" defer></script></body></html>\n`);
const sitemapPaths = entries.flatMap(entry => entry.guide ? [entry.slug, entry.guide.slug] : [entry.slug]);
await writeFile(path.join(root, 'public', 'sitemap.xml'), `<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>https://muhammetozeski-binsite.pages.dev/</loc></url>${sitemapPaths.map(slug => `<url><loc>https://muhammetozeski-binsite.pages.dev/${slug}/</loc></url>`).join('')}</urlset>\n`);
console.log(`Built directory: ${entries.length} independent products; no placeholder projects counted.`);
