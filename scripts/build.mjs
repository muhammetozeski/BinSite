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
}
entries.sort((a, b) => a.id - b.id);
await writeFile(path.join(root, 'public', 'catalog.json'), JSON.stringify(entries, null, 2) + '\n');
const escape = value => String(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[char]);
const cards = entries.map(entry => `<article data-category="${escape(entry.category)}" data-search="${escape([entry.name, entry.description, ...entry.features].join(' ').toLocaleLowerCase('tr-TR'))}"><div class="eyebrow">${escape(entry.category)} · ${entry.id.toString().padStart(3, '0')}</div><h2><a href="/${entry.slug}/">${escape(entry.name)}</a></h2><p>${escape(entry.description)}</p><ul>${entry.features.map(feature => `<li>${escape(feature)}</li>`).join('')}</ul><a class="open" href="/${entry.slug}/">Aç ve dene →</a></article>`).join('\n');
const categories = [...new Set(entries.map(entry => entry.category))];
await writeFile(path.join(root, 'public', 'index.html'), `<!doctype html>
<html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="description" content="Bağımsız oyunlar, yaratıcı araçlar ve birlikte üretilen sosyal dünyalar. Birini seç, dene, kendi hikâyeni başlat."><title>BinSite — Yeni bir şey dene</title><link rel="stylesheet" href="/assets/catalog.css"></head><body>
<header><a class="brand" href="/">BinSite<span>üret · oyna · paylaş</span></a><p class="counter">${entries.length} çalışan proje / 1.000 proje hedefi</p></header>
<main><section class="intro"><p class="eyebrow">HER ADRES AYRI BİR DÜNYA</p><h1>Bugün neye<br>merak saracaksın?</h1><p>Bir oyunla başla, kendi eserini üret veya başkasının hikâyesine bir dal ekle. Buradaki her proje kendi adresinde bağımsız çalışır.</p></section>
<section class="find" aria-label="Proje bul"><div><label for="search">İlgini çeken şeyi ara</label><input id="search" type="search" placeholder="Hikâye, ritim, bulmaca, çizim"></div><div><label for="category">Tür</label><select id="category"><option value="">Hepsi</option>${categories.map(category => `<option>${escape(category)}</option>`).join('')}</select></div><button id="surprise" type="button">Beni şaşırt</button></section>
<p id="results" role="status">${entries.length} proje gösteriliyor.</p><section id="projects" class="projects">${cards}</section><p id="empty" hidden>Aramana uyan bir proje bulunamadı. Başka bir kelime dene.</p>
<section class="about"><h2>Yeni fikirler, çalışan ürünler</h2><p>Bu koleksiyon 1.000 projeye doğru büyüyor. Sayaç yalnızca bu yayında bulunan ürünleri gösterir. Yerel çalışan araçlar verilerini tarayıcında tutar; ortak sosyal ürünler sunucu kullanır ve veri kullanımını kendi ekranlarında açıklar.</p></section></main>
<footer>BinSite · Bağımsız internet deneyleri. Başlangıç: 24 Ekim 2026.</footer><script src="/assets/catalog.js" defer></script></body></html>\n`);
await writeFile(path.join(root, 'public', 'sitemap.xml'), `<?xml version="1.0" encoding="UTF-8"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>https://muhammetozeski-binsite.pages.dev/</loc></url>${entries.map(entry => `<url><loc>https://muhammetozeski-binsite.pages.dev/${entry.slug}/</loc></url>`).join('')}</urlset>\n`);
console.log(`Built directory: ${entries.length} independent products; no placeholder projects counted.`);
