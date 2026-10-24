import definitions from '../integrations/mounted-products.json' with { type: 'json' };

export const mountedOrigin = 'https://muhammetozeski-binsite.pages.dev';

/** Pass only this product's bindings; a sibling payment key is never inherited. */
export async function mountedEnvironment(bindings, key) {
  const definition = definitions.find(item => item.key === key);
  if (!definition?.databaseBinding) throw new TypeError('Unknown direct-mounted product.');
  if (!bindings[definition.databaseBinding]) return null;
  const environment = {
    DB: bindings[definition.databaseBinding],
    APP_BASE_PATH: `/${definition.slug}/`,
    SITE_ORIGIN: mountedOrigin
  };
  for (const name of definition.environmentKeys) {
    const value = bindings[definition.environmentPrefix + name];
    if (value !== undefined) environment[name] = value;
  }
  if (definition.derivedRateKey && !environment[definition.derivedRateKey]) {
    const secret = bindings.COMMERCE_RATE_SALT;
    if (typeof secret !== 'string' || secret.length < 16) return null;
    const encoder = new TextEncoder();
    const hmacKey = await crypto.subtle.importKey('raw', encoder.encode(secret), { name: 'HMAC', hash: 'SHA-256' }, false, ['sign']);
    const digest = await crypto.subtle.sign('HMAC', hmacKey, encoder.encode(`BinSite mounted rate key v1:${definition.key}`));
    environment[definition.derivedRateKey] = Array.from(new Uint8Array(digest), byte => byte.toString(16).padStart(2, '0')).join('');
  }
  return environment;
}

export function unavailableMount() {
  return Response.json({ error: 'This product is temporarily unavailable. Please try again later.' }, { status: 503, headers: { 'Cache-Control': 'no-store' } });
}
