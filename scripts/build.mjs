import { readFile, writeFile, readdir, access } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import { productUrl } from './product-url.mjs';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const entries = [];
for (const filename of (await readdir(path.join(root, 'catalog'))).filter(name => name.endsWith('.json')).sort()) {
  const batch = JSON.parse(await readFile(path.join(root, 'catalog', filename), 'utf8'));
  if (!Array.isArray(batch)) throw new Error(`Catalog must be an array: ${filename}`);
  entries.push(...batch);
}
const ids = new Set(), slugs = new Set(), urls = new Set();
for (const entry of entries) {
  if (!Number.isInteger(entry.id) || entry.id < 1 || ids.has(entry.id) || !/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(entry.slug) || slugs.has(entry.slug)) throw new Error(`Duplicate or invalid product: ${JSON.stringify(entry)}`);
  if (!entry.name || !entry.description || !Array.isArray(entry.features) || !entry.features.length) throw new Error(`Missing product content: ${entry.slug}`);
  if (entry.directoryGroup && !['tools', 'games'].includes(entry.directoryGroup)) throw new Error(`Invalid directory group: ${entry.slug}`);
  ids.add(entry.id); slugs.add(entry.slug);
  const url = productUrl(entry, 'https://muhammetozeski-binsite.pages.dev').replace(/\/$/, '');
  if (urls.has(url)) throw new Error(`Duplicate product URL: ${entry.slug}`);
  urls.add(url);
  if (!entry.url) await access(path.join(root, 'public', entry.slug, 'index.html'));
  if (entry.guide) {
    if (entry.url) throw new Error(`External products manage their own guides: ${entry.slug}`);
    if (entry.guide.slug !== `${entry.slug}/guide` || typeof entry.guide.title !== 'string' || !entry.guide.title.trim()) throw new Error(`Invalid product guide: ${entry.slug}`);
    await access(path.join(root, 'public', entry.guide.slug, 'index.html'));
  }
}
entries.sort((a, b) => a.id - b.id);
await writeFile(path.join(root, 'public', 'catalog.json'), JSON.stringify(entries, null, 2) + '\n');
const escape = value => String(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const group = entry => entry.directoryGroup || (entry.category === 'Games' ? 'games' : 'tools');
const priority = ['youtube-tools', 'catalog-studio', 'review-room', 'field-work-pack', 'video-kurgu', 'ses-atolyesi', 'kagit-katla'];
const ordered = [...entries].sort((a, b) => {
  const left = priority.indexOf(a.slug), right = priority.indexOf(b.slug);
  return (left < 0 ? 999 : left) - (right < 0 ? 999 : right) || a.id - b.id;
});
function cards(kind) {
  return ordered.filter(entry => group(entry) === kind).map(entry => {
    const href = escape(entry.url ? productUrl(entry) : `/${entry.slug}/`);
    return `<article class="product" data-category="${escape(entry.category)}" data-search="${escape([entry.name, entry.description, ...entry.features].join(' ').toLocaleLowerCase('en-US'))}"><h3><a href="${href}">${escape(entry.name)}</a></h3><p>${escape(entry.description)}</p><a class="open" href="${href}" aria-label="Open ${escape(entry.name)}">Open <span aria-hidden="true">→</span></a>${entry.guide ? `<a class="guide" href="/${entry.guide.slug}/">Guide</a>` : ''}</article>`;
  }).join('\n');
}
const categories = [...new Set(entries.map(entry => entry.category))];
await writeFile(path.join(root, 'public', 'index.html'), `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="description" content="Edit media, create something, collaborate or play. Find a tool by what you want to do."><title>BinSite — Tools and games</title><link rel="stylesheet" href="/assets/catalog.css"></head><body>
<header><a class="brand" href="/">BinSite</a><nav aria-label="Browse"><a href="#tools">Tools</a><a href="#games">Games</a></nav></header>
<main><section class="intro"><h1>What do you want to do?</h1><p>Edit a video, make something together, or take a break with a game.</p></section>
<section class="find" aria-label="Find a tool or game"><div><label for="search">Search</label><input id="search" type="search" placeholder="Video, audio, stories, puzzles"></div><div><label for="category">Category</label><select id="category"><option value="">All categories</option>${categories.map(category => `<option>${escape(category)}</option>`).join('')}</select></div></section>
<p id="results" role="status" aria-live="polite"></p>
<section id="tools" class="product-group" data-product-group><h2>Tools &amp; communities</h2><div class="projects">${cards('tools')}</div></section>
<section id="games" class="product-group" data-product-group><h2>Games &amp; playful experiments</h2><div class="projects">${cards('games')}</div></section>
<p id="empty" hidden>No matches. Try another search.</p></main>
<footer><a href="/privacy/">Privacy</a></footer><script src="/assets/catalog.js" defer></script></body></html>\n`);
const sitemapPaths = entries.filter(entry => !entry.url).flatMap(entry => entry.guide ? [entry.slug, entry.guide.slug] : [entry.slug]);
await writeFile(path.join(root, 'public', 'sitemap.xml'), `<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>https://muhammetozeski-binsite.pages.dev/</loc></url>${sitemapPaths.map(slug => `<url><loc>https://muhammetozeski-binsite.pages.dev/${slug}/</loc></url>`).join('')}</urlset>\n`);
console.log(`Built directory: ${entries.length} products grouped into tools and games.`);
