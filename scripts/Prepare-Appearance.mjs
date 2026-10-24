import { readdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const defaultPublic = fileURLToPath(new URL('../public/', import.meta.url));
const marker = 'data-binsite-appearance';
const origin = 'https://appearance.invalid';
const rawNames = new Set(['script', 'style', 'title', 'textarea', 'xmp', 'iframe', 'noembed', 'noframes', 'noscript']);
const headNames = new Set(['base', 'basefont', 'bgsound', 'link', 'meta', 'title', 'noscript', 'noframes', 'style', 'script', 'template']);

// Attribute offsets are unnecessary: only the managed asset tags are normalized.
// All other source text, including scripts, styles and quoted markup, stays intact.
function attributes(source, offset) {
  const result = [];
  const pattern = /([^\s=/>]+)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s>]+)))?/g;
  pattern.lastIndex = offset;
  let match;
  while ((match = pattern.exec(source))) {
    if (match[1] === '<' || match[1] === '>') continue;
    result.push({ name: match[1].toLowerCase(), raw: match[0], value: match[2] ?? match[3] ?? match[4] ?? '' });
  }
  return result;
}

function tagAt(html, start) {
  const prefix = /^<\s*(\/?)\s*([a-z][\w:-]*)\b/i.exec(html.slice(start));
  if (!prefix) return null;
  let quote = '', end = start + prefix[0].length;
  for (; end < html.length; end++) {
    const character = html[end];
    if (quote) { if (character === quote) quote = ''; }
    else if (character === '"' || character === "'") quote = character;
    else if (character === '>') break;
  }
  if (end === html.length) return null;
  end++;
  const source = html.slice(start, end);
  return { start, end, fullEnd: end, name: prefix[2].toLowerCase(), originalName: prefix[2], closing: !!prefix[1], nameEnd: prefix[0].length, source, attributes: attributes(source.slice(0, -1), prefix[0].length), selfClosing: /\/\s*>$/.test(source) };
}

function tokens(html) {
  const result = [], templates = [];
  const lower = html.toLowerCase();
  let position = 0;
  while (position < html.length) {
    const start = html.indexOf('<', position);
    if (start < 0) break;
    if (html.startsWith('<!--', start)) {
      const close = html.indexOf('-->', start + 4), end = close < 0 ? html.length : close + 3;
      result.push({ kind: 'comment', start, end, fullEnd: end, inert: templates.length > 0 }); position = end; continue;
    }
    if (/^<!|^<\?/.test(html.slice(start, start + 2))) {
      const close = html.indexOf('>', start + 2), end = close < 0 ? html.length : close + 1;
      result.push({ kind: 'declaration', start, end, fullEnd: end, inert: templates.length > 0 }); position = end; continue;
    }
    const token = tagAt(html, start);
    if (!token) { position = start + 1; continue; }
    token.inert = templates.length > 0;
    result.push(token);
    if (token.name === 'template') {
      if (token.closing) { const open = templates.pop(); if (open) open.fullEnd = token.end; }
      else if (!token.selfClosing) templates.push(token);
    }
    position = token.end;
    if (!token.closing && rawNames.has(token.name)) {
      let search = position, close;
      while ((close = lower.indexOf('</' + token.name, search)) >= 0) {
        const ending = tagAt(html, close);
        if (ending?.closing && ending.name === token.name) { token.fullEnd = ending.end; position = ending.end; break; }
        search = close + 2;
      }
      if (close < 0) { token.fullEnd = html.length; position = html.length; }
    }
  }
  return result;
}

function value(token, name) { return token.attributes?.find(attribute => attribute.name === name)?.value; }
function decode(value) {
  return value.replace(/&(?:#(x[0-9a-f]+|[0-9]+)|(amp|quot|apos|lt|gt));/gi, (match, number, named) => {
    if (number) { const point = number[0].toLowerCase() === 'x' ? parseInt(number.slice(1), 16) : parseInt(number, 10); return point > 0 && point <= 0x10ffff ? String.fromCodePoint(point) : '\ufffd'; }
    return { amp: '&', quot: '"', apos: "'", lt: '<', gt: '>' }[named.toLowerCase()];
  });
}
function escape(value) { return value.replaceAll('&', '&amp;').replaceAll('"', '&quot;').replaceAll('<', '&lt;'); }

function managedTag(token, updates, remove = []) {
  const pending = new Map(Object.entries(updates)), excluded = new Set(remove), output = [];
  for (const attribute of token?.attributes ?? []) {
    if (excluded.has(attribute.name)) continue;
    if (Object.hasOwn(updates, attribute.name)) {
      if (!pending.has(attribute.name)) continue;
      const update = pending.get(attribute.name); pending.delete(attribute.name);
      output.push(update === null ? attribute.name : `${attribute.name}="${escape(update)}"`);
    } else output.push(attribute.raw);
  }
  for (const [name, update] of pending) output.push(update === null ? name : `${name}="${escape(update)}"`);
  return `<${token?.originalName ?? (Object.hasOwn(updates, 'href') ? 'link' : 'script')} ${output.join(' ')}${token?.selfClosing && Object.hasOwn(updates, 'href') ? ' />' : '>'}`;
}

function headEnd(html, list, htmlTag) {
  const explicitEnd = list.find(token => !token.inert && token.name === 'head' && token.closing);
  if (explicitEnd) return explicitEnd.start;
  const start = list.find(token => !token.inert && token.name === 'head' && !token.closing);
  let cursor = start?.end ?? htmlTag.end;
  for (const token of list) {
    if (token.start < cursor) continue;
    const text = html.slice(cursor, token.start);
    const content = text.search(/[^\s\uFEFF]/);
    if (content >= 0) return cursor + content;
    if (token.kind || token.inert) { cursor = token.fullEnd; continue; }
    if (token.name === 'head' && !token.closing) { cursor = token.end; continue; }
    if (!headNames.has(token.name)) return token.start;
    cursor = token.fullEnd;
  }
  const trailing = html.slice(cursor), content = trailing.search(/[^\s\uFEFF]/);
  return content < 0 ? html.length : cursor + content;
}

/** Adds only the dark-default attribute and two managed dependencies, respecting effective base URLs. */
export function transformHtml(input, relativeFile = 'index.html') {
  let html = input, list = tokens(html);
  let htmlTag = list.find(token => !token.inert && token.name === 'html' && !token.closing);
  if (!htmlTag) {
    const doctype = list.find(token => token.kind === 'declaration' && /^<!doctype\b/i.test(html.slice(token.start, token.end)));
    const at = doctype?.end ?? (html.charCodeAt(0) === 0xfeff ? 1 : 0);
    html = html.slice(0, at) + '<html data-bs-theme="dark">' + html.slice(at);
    list = tokens(html); htmlTag = list.find(token => !token.inert && token.name === 'html' && !token.closing);
  } else if (!htmlTag.attributes.some(attribute => attribute.name === 'data-bs-theme')) {
    const at = htmlTag.start + htmlTag.nameEnd;
    html = html.slice(0, at) + ' data-bs-theme="dark"' + html.slice(at);
    list = tokens(html); htmlTag = list.find(token => !token.inert && token.name === 'html' && !token.closing);
  } else if (htmlTag.attributes.filter(attribute => attribute.name === 'data-bs-theme').some(attribute => attribute.value !== 'dark') || htmlTag.attributes.filter(attribute => attribute.name === 'data-bs-theme').length > 1) {
    html = html.slice(0, htmlTag.start) + managedTag(htmlTag, { 'data-bs-theme': 'dark' }) + html.slice(htmlTag.end);
    list = tokens(html); htmlTag = list.find(token => !token.inert && token.name === 'html' && !token.closing);
  }
  const end = headEnd(html, list, htmlTag);
  const headStart = list.find(token => !token.inert && token.name === 'head' && !token.closing)?.end ?? htmlTag.end;
  const base = list.find(token => !token.inert && token.name === 'base' && !token.closing && token.start >= headStart && token.start < end && value(token, 'href') !== undefined);
  const documentUrl = new URL('/' + relativeFile.split(path.sep).join('/').replace(/^\/+/, ''), origin);
  const effectiveBase = base ? new URL(decode(value(base, 'href')), documentUrl) : documentUrl;
  if (effectiveBase.origin !== origin) throw new Error(`Cannot inject local relative appearance dependencies through an external base href in ${relativeFile}.`);
  const folder = effectiveBase.pathname.endsWith('/') ? effectiveBase.pathname.slice(1) : path.posix.dirname(effectiveBase.pathname.slice(1));
  const asset = extension => path.posix.relative(folder, `assets/appearance.${extension}`) || `appearance.${extension}`;
  const isManaged = (token, name, extension) => {
    if (token.inert || token.closing || token.name !== name) return false;
    if (value(token, marker) !== undefined) return true;
    const href = value(token, name === 'link' ? 'href' : 'src');
    if (href === undefined) return false;
    try { return new URL(decode(href), effectiveBase).pathname === `/assets/appearance.${extension}`; } catch { return false; }
  };
  const styles = list.filter(token => isManaged(token, 'link', 'css'));
  const scripts = list.filter(token => isManaged(token, 'script', 'js'));
  const edits = [], append = [];
  for (const [existing, kind, extension] of [[styles, 'style', 'css'], [scripts, 'script', 'js']]) {
    const source = existing[0];
    const first = source && source.start >= headStart && source.start >= (base?.end ?? headStart) && source.fullEnd <= end ? source : null;
    for (const token of existing) if (token !== first) edits.push({ start: token.start, end: token.fullEnd, text: '' });
    const opening = kind === 'style'
      ? managedTag(source, { rel: 'stylesheet', href: asset(extension), [marker]: kind }, ['integrity'])
      : managedTag(source, { src: asset(extension), defer: null, [marker]: kind }, ['type', 'async', 'nomodule', 'integrity']);
    const contents = kind === 'script' ? (source && source.fullEnd > source.end ? html.slice(source.end, source.fullEnd) : '</script>') : '';
    const text = opening + contents;
    if (first) edits.push({ start: first.start, end: first.fullEnd, text }); else append.push(text);
  }
  if (append.length) {
    const newline = html.includes('\r\n') ? '\r\n' : '\n';
    edits.push({ start: end, end, text: newline + append.join(newline) + newline });
  }
  edits.sort((first, second) => second.start - first.start || second.end - first.end);
  for (const edit of edits) html = html.slice(0, edit.start) + edit.text + html.slice(edit.end);
  return html;
}

/** Visits physical HTML files only; framework output and directory symlinks are excluded. */
export async function prepareAppearance(publicRoot = defaultPublic) {
  const root = path.resolve(publicRoot);
  let ownedRoots = new Set();
  try {
    const catalog = JSON.parse(await readFile(path.join(root, 'catalog.json'), 'utf8'));
    ownedRoots = new Set(catalog.filter(item => item.runtime === 'mounted').map(item => item.slug));
  } catch (error) { if (error.code !== 'ENOENT') throw error; }
  let htmlCount = 0, updatedCount = 0;
  async function visit(directory) {
    const entries = await readdir(directory, { withFileTypes: true });
    entries.sort((first, second) => first.name.localeCompare(second.name, 'en'));
    for (const entry of entries) {
      if (entry.name.toLowerCase() === '_framework') continue;
      const filename = path.join(directory, entry.name);
      if (entry.isDirectory() && directory === root && ownedRoots.has(entry.name)) continue;
      if (entry.isDirectory()) await visit(filename);
      else if (entry.isFile() && /\.html?$/i.test(entry.name)) {
        const input = await readFile(filename, 'utf8');
        const output = transformHtml(input, path.relative(root, filename));
        htmlCount++;
        if (output !== input) { await writeFile(filename, output, 'utf8'); updatedCount++; }
      }
    }
  }
  await visit(root);
  return { htmlCount, updatedCount };
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
  const result = await prepareAppearance(process.argv[2] ?? defaultPublic);
  console.log(`Appearance dependencies prepared: ${result.htmlCount} HTML files inspected; ${result.updatedCount} updated. This is a file count, not a product count.`);
}
