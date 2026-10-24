export function productUrl(entry, origin) {
  if (entry.appearance !== undefined && (!entry.appearance || typeof entry.appearance !== 'object' || !/^data-[a-z][a-z0-9-]*$/.test(entry.appearance.themeAttribute) || !/^data-[a-z][a-z0-9-]*$/.test(entry.appearance.assetAttribute))) throw new Error(`Invalid appearance evidence attributes: ${entry.slug}`);
  if (entry.url === undefined) return new URL(`/${entry.slug}/`, origin).href;
  if (typeof entry.url !== 'string') throw new Error(`Invalid external product URL: ${entry.slug}`);
  let url;
  try { url = new URL(entry.url); } catch { throw new Error(`Invalid external product URL: ${entry.slug}`); }
  if (url.protocol !== 'https:' || url.username || url.password || url.search || url.hash) throw new Error(`External product URLs require HTTPS without credentials, query or fragment: ${entry.slug}`);
  return url.href;
}
