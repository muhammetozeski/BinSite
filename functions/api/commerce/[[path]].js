const COOKIE = 'binsite_member';
const PLAN = 'creator-monthly';
const SESSION_MS = 30 * 86400000;
const STRIPE_VERSION = '2025-02-24.acacia';
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
const encoder = new TextEncoder();

class CommerceError extends Error {
  constructor(status, message, code = 'request_failed') { super(message); this.status = status; this.code = code; }
}

function json(value, status = 200, cookie = null) {
  const headers = { 'Content-Type': 'application/json; charset=utf-8', 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff', 'Referrer-Policy': 'no-referrer' };
  if (cookie) headers['Set-Cookie'] = cookie;
  return new Response(JSON.stringify(value), { status, headers });
}

function bytesToHex(value) { return Array.from(new Uint8Array(value), byte => byte.toString(16).padStart(2, '0')).join(''); }
async function hash(value) { return bytesToHex(await crypto.subtle.digest('SHA-256', encoder.encode(value))); }
async function hmac(secret, value) {
  const key = await crypto.subtle.importKey('raw', encoder.encode(secret), { name: 'HMAC', hash: 'SHA-256' }, false, ['sign']);
  return bytesToHex(await crypto.subtle.sign('HMAC', key, encoder.encode(value)));
}
function sameHex(a, b) {
  if (typeof a !== 'string' || typeof b !== 'string' || a.length !== b.length || !/^[0-9a-f]+$/i.test(a)) return false;
  let different = 0; for (let i = 0; i < a.length; i++) different |= a.toLowerCase().charCodeAt(i) ^ b.toLowerCase().charCodeAt(i);
  return different === 0;
}
function randomToken() { const bytes = crypto.getRandomValues(new Uint8Array(32)); return btoa(String.fromCharCode(...bytes)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, ''); }
function sessionCookie(token, clear = false) { return `${COOKIE}=${clear ? '' : token}; Path=/api/commerce; Max-Age=${clear ? 0 : SESSION_MS / 1000}; HttpOnly; Secure; SameSite=Strict`; }
function idOf(object) { return typeof object === 'string' ? object : object?.id || ''; }
function requireId(value) { if (typeof value !== 'string' || !UUID.test(value)) throw new CommerceError(400, 'Proje kimliği geçersiz.', 'invalid_id'); return value; }
function text(value, label, max) {
  if (typeof value !== 'string') throw new CommerceError(400, `${label} metin olmalı.`, 'invalid_field');
  const result = value.trim().normalize('NFC');
  if (!result || result.length > max || /[\u0000-\u001f\u007f]/u.test(result)) throw new CommerceError(400, `${label} 1–${max} karakter olmalı.`, 'invalid_field');
  return result;
}

async function rawBody(request, maximum) {
  const length = Number(request.headers.get('content-length'));
  if (Number.isFinite(length) && length > maximum) throw new CommerceError(413, 'Gönderilen veri sınırı aşıyor.', 'body_too_large');
  const reader = request.body?.getReader(); if (!reader) throw new CommerceError(400, 'İstek gövdesi eksik.', 'missing_body');
  let total = 0; const chunks = [];
  while (true) { const { value, done } = await reader.read(); if (done) break; total += value.byteLength; if (total > maximum) { await reader.cancel(); throw new CommerceError(413, 'Gönderilen veri sınırı aşıyor.', 'body_too_large'); } chunks.push(value); }
  const bytes = new Uint8Array(total); let offset = 0; for (const chunk of chunks) { bytes.set(chunk, offset); offset += chunk.length; }
  try { return new TextDecoder('utf-8', { fatal: true }).decode(bytes); } catch { throw new CommerceError(400, 'Metin kodlaması geçersiz.', 'invalid_encoding'); }
}
function parseObject(raw) { try { const value = JSON.parse(raw); if (!value || typeof value !== 'object' || Array.isArray(value)) throw new Error(); return value; } catch { throw new CommerceError(400, 'JSON nesnesi bekleniyor.', 'invalid_json'); } }
async function bodyJson(request) {
  if (!(request.headers.get('content-type') || '').toLowerCase().startsWith('application/json')) throw new CommerceError(415, 'İstek JSON biçiminde olmalı.', 'content_type');
  return parseObject(await rawBody(request, 70000));
}

function providers(env) { return { stripe: !!(env.STRIPE_SECRET_KEY && env.STRIPE_PRICE_ID && env.STRIPE_WEBHOOK_SECRET && env.STRIPE_PORTAL_CONFIGURATION_ID), lemon: !!(env.LEMON_API_KEY && env.LEMON_STORE_ID && env.LEMON_VARIANT_ID && env.LEMON_WEBHOOK_SECRET) }; }
function planInfo(env) {
  const amount = Number(env.CREATOR_MONTHLY_CENTS || 500);
  return { id: PLAN, name: 'Üretici Üyeliği', priceLabel: `${(amount / 100).toFixed(2)} USD / ay`, features: ['100 özel bulut projesi', 'Her projede 64 KB JSON verisi', 'Cihazlar arasında kurtarma anahtarıyla erişim', 'Üyelik sona erse de mevcut projeleri okuma ve silme'] };
}
async function limit(db, ip, action, maximum, seconds = 60) {
  const now = Date.now(), bucket = Math.floor(now / (seconds * 1000));
  const key = `${action}:${ip}:${bucket}`;
  const result = await db.prepare('INSERT INTO commerce_rate_limits (rate_key, count, expires_at) VALUES (?, 1, ?) ON CONFLICT(rate_key) DO UPDATE SET count = count + 1 WHERE count < ? RETURNING count').bind(key, (bucket + 1) * seconds * 1000, maximum).all();
  if (!result.results?.length) throw new CommerceError(429, 'Bu işlem için kısa süreli sınır doldu. Biraz sonra tekrar deneyin.', 'rate_limit');
  if (Math.random() < .02) await db.prepare('DELETE FROM commerce_rate_limits WHERE expires_at < ?').bind(now).run();
}
async function accountFromRequest(request, db) {
  const match = (request.headers.get('cookie') || '').split(';').map(part => part.trim()).find(part => part.startsWith(COOKIE + '='));
  const token = match?.slice(COOKIE.length + 1); if (!token || !/^[A-Za-z0-9_-]{43}$/.test(token)) return null;
  return db.prepare('SELECT m.id FROM commerce_sessions s JOIN commerce_members m ON m.id = s.member_id WHERE s.session_hash = ? AND s.expires_at > ?').bind(await hash(token), Date.now()).first();
}
async function accountStatus(db, member) {
  if (!member) return null;
  const subscriptions = await db.prepare('SELECT provider, status, premium, expires_at AS expiresAt, cancel_at_period_end AS cancelAtPeriodEnd FROM commerce_subscriptions WHERE member_id = ? ORDER BY expires_at DESC').bind(member.id).all();
  const active = subscriptions.results.filter(row => row.premium === 1 && row.expiresAt > Date.now());
  return { id: member.id, premium: active.length > 0, projectLimit: active.length ? 100 : 3, plan: active.length ? PLAN : 'free', expiresAt: active.length ? Math.max(...active.map(row => row.expiresAt)) : null, subscriptions: subscriptions.results.map(row => ({ ...row, premium: row.premium === 1 && row.expiresAt > Date.now(), cancelAtPeriodEnd: row.cancelAtPeriodEnd === 1 })) };
}
async function startSession(db, memberId, recovery = false) {
  const token = randomToken(), now = Date.now(), statement = db.prepare('INSERT INTO commerce_sessions (session_hash, member_id, created_at, expires_at) VALUES (?, ?, ?, ?)').bind(await hash(token), memberId, now, now + SESSION_MS);
  if (recovery) await db.batch([db.prepare('DELETE FROM commerce_sessions WHERE member_id = ?').bind(memberId), statement]);
  else await statement.run();
  return sessionCookie(token);
}

async function providerFetch(env, provider, endpoint, options = {}) {
  const stripe = provider === 'stripe', controller = new AbortController(), timer = setTimeout(() => controller.abort(), 12000);
  try {
    const headers = stripe ? { Authorization: `Bearer ${env.STRIPE_SECRET_KEY}`, 'Stripe-Version': STRIPE_VERSION } : { Authorization: `Bearer ${env.LEMON_API_KEY}`, Accept: 'application/vnd.api+json' };
    let body;
    if (options.body) { headers['Content-Type'] = stripe ? 'application/x-www-form-urlencoded' : 'application/vnd.api+json'; body = stripe ? new URLSearchParams(options.body).toString() : JSON.stringify(options.body); }
    if (options.idempotencyKey && stripe) headers['Idempotency-Key'] = options.idempotencyKey;
    const response = await fetch((stripe ? 'https://api.stripe.com/v1/' : 'https://api.lemonsqueezy.com/v1/') + endpoint, { method: options.method || (body ? 'POST' : 'GET'), headers, body, signal: controller.signal });
    if (!response.ok) throw new CommerceError(502, 'Ödeme sağlayıcısı işlemi şu anda tamamlayamadı.', 'provider_error');
    return await response.json();
  } catch (error) { if (error instanceof CommerceError) throw error; throw new CommerceError(502, 'Ödeme sağlayıcısına erişilemiyor. Tekrar deneyebilirsiniz.', 'provider_unavailable'); }
  finally { clearTimeout(timer); }
}
function externalUrl(value, provider, portal = false) {
  try { const url = new URL(value); const host = url.hostname; const allowed = provider === 'stripe' ? portal ? host === 'billing.stripe.com' : host === 'checkout.stripe.com' : host === 'lemonsqueezy.com' || host.endsWith('.lemonsqueezy.com'); if (url.protocol === 'https:' && allowed) return url.href; } catch {}
  throw new CommerceError(502, 'Sağlayıcı geçerli bir ödeme bağlantısı vermedi.', 'provider_url');
}
async function lock(db, key) {
  const token = crypto.randomUUID(), now = Date.now();
  const result = await db.prepare('INSERT INTO commerce_locks (lock_key, lease_token, expires_at) VALUES (?, ?, ?) ON CONFLICT(lock_key) DO UPDATE SET lease_token = excluded.lease_token, expires_at = excluded.expires_at WHERE expires_at < ? RETURNING lease_token').bind(key, token, now + 55000, now).all();
  if (!result.results?.length) throw new CommerceError(409, 'Bu işlem işleniyor. Biraz sonra tekrar deneyin.', 'processing');
  return async () => { await db.prepare('DELETE FROM commerce_locks WHERE lock_key = ? AND lease_token = ?').bind(key, token).run(); };
}
async function ownedAttempt(db, provider, reference, claimedMember) {
  if (!UUID.test(reference || '')) return null;
  const row = await db.prepare('SELECT id, member_id, provider_checkout_id FROM commerce_checkouts WHERE id = ? AND provider = ? AND plan_id = ?').bind(reference, provider, PLAN).first();
  if (row && claimedMember && row.member_id !== claimedMember) throw new CommerceError(400, 'Ödeme hesabı eşleşmiyor.', 'member_mismatch');
  return row;
}
async function knownMember(db, provider, subscriptionId, customerId, reference, claimedMember) {
  const known = await db.prepare('SELECT member_id FROM commerce_subscriptions WHERE provider = ? AND subscription_id = ?').bind(provider, subscriptionId).first();
  const attempt = await ownedAttempt(db, provider, reference, claimedMember);
  if (known && attempt && known.member_id !== attempt.member_id) throw new CommerceError(400, 'Abonelik hesabı eşleşmiyor.', 'member_mismatch');
  const memberId = known?.member_id || attempt?.member_id;
  if (!memberId) return null;
  const existingCustomer = await db.prepare('SELECT member_id FROM commerce_customers WHERE provider = ? AND customer_id = ?').bind(provider, customerId).first();
  if (existingCustomer && existingCustomer.member_id !== memberId) throw new CommerceError(400, 'Müşteri hesabı eşleşmiyor.', 'member_mismatch');
  return memberId;
}

async function storeSubscription(db, data) {
  const revoked = data.periodId ? await db.prepare('SELECT 1 AS found FROM commerce_revocations WHERE provider = ? AND subscription_id = ? AND period_id = ?').bind(data.provider, data.subscriptionId, data.periodId).first() : null;
  const premium = data.premium && !revoked && data.expiresAt > Date.now() ? 1 : 0;
  await db.batch([
    db.prepare('INSERT INTO commerce_customers (provider, customer_id, member_id) VALUES (?, ?, ?) ON CONFLICT(provider, customer_id) DO NOTHING').bind(data.provider, data.customerId, data.memberId),
    db.prepare('INSERT INTO commerce_subscriptions (provider, subscription_id, member_id, customer_id, plan_id, status, premium, expires_at, cancel_at_period_end, period_id, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?) ON CONFLICT(provider, subscription_id) DO UPDATE SET customer_id = excluded.customer_id, status = excluded.status, premium = excluded.premium, expires_at = excluded.expires_at, cancel_at_period_end = excluded.cancel_at_period_end, period_id = excluded.period_id, updated_at = excluded.updated_at WHERE member_id = excluded.member_id').bind(data.provider, data.subscriptionId, data.memberId, data.customerId, PLAN, data.status, premium, data.expiresAt, data.cancelAtPeriodEnd ? 1 : 0, data.periodId || '', Date.now())
  ]);
}
async function revokePeriod(db, provider, subscriptionId, periodId, reason) {
  if (!periodId) return;
  await db.prepare('INSERT OR IGNORE INTO commerce_revocations (provider, subscription_id, period_id, reason, created_at) VALUES (?, ?, ?, ?, ?)').bind(provider, subscriptionId, periodId, reason, Date.now()).run();
}

async function syncStripe(env, subscriptionId, eventInfo = {}) {
  if (!/^sub_[A-Za-z0-9]+$/.test(subscriptionId)) return;
  const release = await lock(env.DB, 'stripe:' + subscriptionId);
  try {
    const sub = await providerFetch(env, 'stripe', 'subscriptions/' + subscriptionId + '?expand[]=latest_invoice.charge');
    const item = sub.items?.data?.find(value => idOf(value.price) === env.STRIPE_PRICE_ID);
    if (!item || sub.livemode !== env.STRIPE_SECRET_KEY.startsWith('sk_live_')) return;
    const customerId = idOf(sub.customer), memberId = await knownMember(env.DB, 'stripe', sub.id, customerId, sub.metadata?.checkout_ref, sub.metadata?.member_id);
    if (!memberId) return;
    let invoice = sub.latest_invoice;
    if (typeof invoice === 'string') invoice = await providerFetch(env, 'stripe', 'invoices/' + encodeURIComponent(invoice) + '?expand[]=charge');
    const periodId = idOf(invoice), charge = invoice?.charge;
    const currentFullRefund = charge && typeof charge === 'object' && (charge.refunded || (charge.amount > 0 && charge.amount_refunded >= charge.amount));
    if (currentFullRefund) await revokePeriod(env.DB, 'stripe', sub.id, periodId, 'full_refund');
    if (eventInfo.revokeInvoice) await revokePeriod(env.DB, 'stripe', sub.id, eventInfo.revokeInvoice, eventInfo.reason || 'full_refund');
    const expiresAt = Number(sub.current_period_end || item.current_period_end || 0) * 1000;
    const paid = !!(invoice?.paid && invoice.amount_paid > 0);
    const active = ['active', 'past_due', 'unpaid'].includes(sub.status) && paid && !sub.pause_collection;
    let data = { provider: 'stripe', subscriptionId: sub.id, customerId, memberId, status: sub.status, premium: active, expiresAt, periodId, cancelAtPeriodEnd: sub.cancel_at_period_end };
    if (!active && ['past_due', 'unpaid'].includes(sub.status)) {
      const previous = await env.DB.prepare('SELECT premium, expires_at, period_id FROM commerce_subscriptions WHERE provider = ? AND subscription_id = ?').bind('stripe', sub.id).first();
      if (previous?.premium && previous.expires_at > Date.now()) data = { ...data, premium: true, expiresAt: previous.expires_at, periodId: previous.period_id };
    }
    await storeSubscription(env.DB, data);
  } finally { await release(); }
}
async function stripeEvent(env, event) {
  const object = event.data?.object; if (!object?.id || event.account) return;
  if (event.type.startsWith('customer.subscription.')) { await syncStripe(env, object.id); return; }
  if (['checkout.session.completed', 'checkout.session.async_payment_succeeded'].includes(event.type)) {
    const session = await providerFetch(env, 'stripe', 'checkout/sessions/' + encodeURIComponent(object.id));
    const attempt = await ownedAttempt(env.DB, 'stripe', session.metadata?.checkout_ref, session.client_reference_id);
    if (!attempt || (attempt.provider_checkout_id && attempt.provider_checkout_id !== session.id)) return;
    if (session.mode === 'subscription' && session.payment_status === 'paid') await syncStripe(env, idOf(session.subscription));
    return;
  }
  if (event.type.startsWith('invoice.')) {
    const invoice = await providerFetch(env, 'stripe', 'invoices/' + encodeURIComponent(object.id));
    await syncStripe(env, idOf(invoice.subscription || invoice.parent?.subscription_details?.subscription)); return;
  }
  if (['charge.refunded', 'charge.dispute.created'].includes(event.type)) {
    const chargeId = event.type === 'charge.dispute.created' ? idOf(object.charge) : object.id;
    const charge = await providerFetch(env, 'stripe', 'charges/' + encodeURIComponent(chargeId));
    if (event.type !== 'charge.dispute.created' && !(charge.refunded || (charge.amount > 0 && charge.amount_refunded >= charge.amount))) return;
    if (!charge.invoice) return;
    const invoice = await providerFetch(env, 'stripe', 'invoices/' + encodeURIComponent(idOf(charge.invoice)));
    await syncStripe(env, idOf(invoice.subscription || invoice.parent?.subscription_details?.subscription), { revokeInvoice: invoice.id, reason: event.type === 'charge.dispute.created' ? 'chargeback' : 'full_refund' });
  }
}

async function syncLemon(env, subscriptionId, custom = {}, revokeInvoice = null, revokeOrder = null) {
  if (!/^\d+$/.test(String(subscriptionId))) return;
  const release = await lock(env.DB, 'lemon:' + subscriptionId);
  try {
    const response = await providerFetch(env, 'lemon', 'subscriptions/' + subscriptionId), sub = response.data, a = sub?.attributes;
    if (!a || String(a.store_id) !== env.LEMON_STORE_ID || String(a.variant_id) !== env.LEMON_VARIANT_ID || !!a.test_mode !== (env.LEMON_TEST_MODE === 'true')) return;
    const memberId = await knownMember(env.DB, 'lemon', sub.id, String(a.customer_id), custom.checkout_ref, custom.member_id);
    if (!memberId) return;
    const invoices = await providerFetch(env, 'lemon', 'subscription-invoices?filter[subscription_id]=' + sub.id + '&page[size]=1');
    const invoice = invoices.data?.[0], payment = invoice?.attributes, periodId = invoice?.id || '';
    if (revokeInvoice) await revokePeriod(env.DB, 'lemon', sub.id, revokeInvoice, 'full_refund');
    if (revokeOrder && String(a.order_id) === String(revokeOrder) && payment?.billing_reason === 'initial') await revokePeriod(env.DB, 'lemon', sub.id, periodId, 'full_refund');
    const fullRefund = payment?.refunded && (payment.refunded_amount === undefined || payment.refunded_amount >= payment.total);
    if (fullRefund) await revokePeriod(env.DB, 'lemon', sub.id, periodId, 'full_refund');
    const expiresAt = Date.parse(a.ends_at || a.renews_at || '') || 0;
    const paid = payment?.status === 'paid' && payment.total > 0 && !fullRefund;
    let data = { provider: 'lemon', subscriptionId: sub.id, memberId, customerId: String(a.customer_id), status: a.status, premium: ['active', 'cancelled'].includes(a.status) && paid, expiresAt, periodId, cancelAtPeriodEnd: !!a.cancelled || a.status === 'cancelled' };
    if (!data.premium && ['past_due', 'unpaid'].includes(a.status)) {
      const previous = await env.DB.prepare('SELECT premium, expires_at, period_id FROM commerce_subscriptions WHERE provider = ? AND subscription_id = ?').bind('lemon', sub.id).first();
      if (previous?.premium && previous.expires_at > Date.now()) data = { ...data, premium: true, expiresAt: previous.expires_at, periodId: previous.period_id };
    }
    await storeSubscription(env.DB, data);
  } finally { await release(); }
}
async function lemonEvent(env, event) {
  const type = event.meta?.event_name || '', data = event.data, a = data?.attributes, custom = event.meta?.custom_data || {};
  if (!a || String(a.store_id) !== env.LEMON_STORE_ID || !!a.test_mode !== (env.LEMON_TEST_MODE === 'true')) return;
  if (data.type === 'subscriptions' && type.startsWith('subscription_')) { await syncLemon(env, data.id, custom); return; }
  if (data.type === 'subscription-invoices' && type.startsWith('subscription_payment_')) {
    const current = await providerFetch(env, 'lemon', 'subscription-invoices/' + data.id), invoice = current.data?.attributes;
    const fullRefund = invoice?.refunded && (invoice.refunded_amount === undefined || invoice.refunded_amount >= invoice.total);
    if (invoice) await syncLemon(env, invoice.subscription_id, custom, fullRefund ? data.id : null); return;
  }
  if (type === 'order_refunded' && data.type === 'orders') {
    const current = await providerFetch(env, 'lemon', 'orders/' + data.id), order = current.data?.attributes;
    if (!order?.refunded || (order.refunded_amount !== undefined && order.refunded_amount < order.total)) return;
    const rows = await env.DB.prepare('SELECT subscription_id FROM commerce_subscriptions WHERE provider = ? AND customer_id = ?').bind('lemon', String(order.customer_id)).all();
    for (const row of rows.results) await syncLemon(env, row.subscription_id, custom, null, data.id);
  }
}

async function verifyWebhook(request, env, provider, raw) {
  if (provider === 'stripe') {
    if (!env.STRIPE_WEBHOOK_SECRET || !env.STRIPE_SECRET_KEY) throw new CommerceError(503, 'Stripe bildirimleri henüz yapılandırılmadı.', 'provider_unconfigured');
    const fields = (request.headers.get('stripe-signature') || '').split(',').map(part => part.split('='));
    const timestamp = fields.find(([key]) => key === 't')?.[1], signatures = fields.filter(([key]) => key === 'v1').map(([, value]) => value);
    if (!/^\d+$/.test(timestamp || '') || Math.abs(Date.now() / 1000 - Number(timestamp)) > 300) throw new CommerceError(400, 'Bildirim imzası veya zamanı geçersiz.', 'webhook_signature');
    const expected = await hmac(env.STRIPE_WEBHOOK_SECRET, timestamp + '.' + raw);
    if (!signatures.some(signature => sameHex(signature, expected))) throw new CommerceError(400, 'Bildirim imzası geçersiz.', 'webhook_signature');
  } else {
    if (!env.LEMON_WEBHOOK_SECRET || !env.LEMON_API_KEY) throw new CommerceError(503, 'Lemon Squeezy bildirimleri henüz yapılandırılmadı.', 'provider_unconfigured');
    if (!sameHex(request.headers.get('x-signature'), await hmac(env.LEMON_WEBHOOK_SECRET, raw))) throw new CommerceError(400, 'Bildirim imzası geçersiz.', 'webhook_signature');
  }
}
async function webhook(request, env, provider) {
  const raw = await rawBody(request, 262144); await verifyWebhook(request, env, provider, raw); const event = parseObject(raw);
  if (provider === 'stripe' && (typeof event.id !== 'string' || !/^evt_[A-Za-z0-9]+$/.test(event.id))) throw new CommerceError(400, 'Bildirim kimliği geçersiz.', 'webhook_event');
  const eventId = provider === 'stripe' ? event.id : await hash(raw), now = Date.now(), lease = crypto.randomUUID();
  const claimed = await env.DB.prepare('INSERT INTO commerce_events (provider, event_id, state, lease_token, lease_until, received_at) VALUES (?, ?, ?, ?, ?, ?) ON CONFLICT(provider, event_id) DO UPDATE SET lease_token = excluded.lease_token, lease_until = excluded.lease_until WHERE state = ? AND lease_until < ? RETURNING event_id').bind(provider, eventId, 'pending', lease, now + 55000, now, 'pending', now).all();
  if (!claimed.results?.length) { const existing = await env.DB.prepare('SELECT state FROM commerce_events WHERE provider = ? AND event_id = ?').bind(provider, eventId).first(); if (existing?.state === 'done') return json({ received: true, duplicate: true }); throw new CommerceError(409, 'Bildirim işleniyor; tekrar gönderilebilir.', 'processing'); }
  try {
    if (provider === 'stripe') await stripeEvent(env, event); else await lemonEvent(env, event);
    await env.DB.prepare('UPDATE commerce_events SET state = ?, processed_at = ?, lease_until = 0 WHERE provider = ? AND event_id = ? AND lease_token = ?').bind('done', Date.now(), provider, eventId, lease).run();
    return json({ received: true });
  } catch (error) { await env.DB.prepare('UPDATE commerce_events SET lease_until = 0 WHERE provider = ? AND event_id = ? AND lease_token = ?').bind(provider, eventId, lease).run(); throw error; }
}

async function checkout(request, env, member, body, origin) {
  const provider = body.provider;
  if (!['stripe', 'lemon'].includes(provider) || body.plan !== PLAN) throw new CommerceError(400, 'Plan veya sağlayıcı geçersiz.', 'invalid_plan');
  if (!providers(env)[provider]) throw new CommerceError(503, 'Bu ödeme yöntemi henüz etkin değil. Şu anda ödeme alınmaz.', 'provider_unconfigured');
  const status = await accountStatus(env.DB, member); if (status.premium) throw new CommerceError(409, 'Zaten aktif bir üyeliğiniz var. Yeni ödeme yerine abonelik yönetimini kullanın.', 'already_subscribed');
  const release = await lock(env.DB, 'checkout:' + member.id);
  try {
    const existing = await env.DB.prepare('SELECT id FROM commerce_checkouts WHERE member_id = ? AND created_at > ? ORDER BY created_at DESC LIMIT 1').bind(member.id, Date.now() - 90000).first();
    if (existing) throw new CommerceError(409, 'Az önce bir ödeme sayfası açıldı. İşlemi oradan tamamlayın veya kısa süre sonra yeniden deneyin.', 'checkout_pending');
    const reference = crypto.randomUUID();
    await env.DB.prepare('INSERT INTO commerce_checkouts (id, member_id, provider, plan_id, created_at) VALUES (?, ?, ?, ?, ?)').bind(reference, member.id, provider, PLAN, Date.now()).run();
    let response;
    if (provider === 'stripe') {
      const customer = await env.DB.prepare('SELECT customer_id FROM commerce_customers WHERE provider = ? AND member_id = ? LIMIT 1').bind('stripe', member.id).first();
      const values = { mode: 'subscription', 'line_items[0][price]': env.STRIPE_PRICE_ID, 'line_items[0][quantity]': '1', client_reference_id: member.id, 'metadata[checkout_ref]': reference, 'metadata[member_id]': member.id, 'subscription_data[metadata][checkout_ref]': reference, 'subscription_data[metadata][member_id]': member.id, success_url: origin + '/uyelik/?checkout=success', cancel_url: origin + '/uyelik/?checkout=cancelled' };
      if (customer) values.customer = customer.customer_id;
      response = await providerFetch(env, 'stripe', 'checkout/sessions', { body: values, idempotencyKey: reference });
      await env.DB.prepare('UPDATE commerce_checkouts SET provider_checkout_id = ? WHERE id = ?').bind(response.id, reference).run();
      return { url: externalUrl(response.url, provider) };
    }
    response = await providerFetch(env, 'lemon', 'checkouts', { body: { data: { type: 'checkouts', attributes: { checkout_data: { custom: { member_id: member.id, checkout_ref: reference } }, checkout_options: { skip_trial: true }, product_options: { enabled_variants: [Number(env.LEMON_VARIANT_ID)], redirect_url: origin + '/uyelik/?checkout=success' }, expires_at: new Date(Date.now() + 30 * 60000).toISOString(), test_mode: env.LEMON_TEST_MODE === 'true' }, relationships: { store: { data: { type: 'stores', id: env.LEMON_STORE_ID } }, variant: { data: { type: 'variants', id: env.LEMON_VARIANT_ID } } } } } });
    await env.DB.prepare('UPDATE commerce_checkouts SET provider_checkout_id = ? WHERE id = ?').bind(response.data.id, reference).run();
    return { url: externalUrl(response.data.attributes.url, provider) };
  } finally { await release(); }
}
async function portal(env, member, provider, origin) {
  if (!['stripe', 'lemon'].includes(provider) || !providers(env)[provider]) throw new CommerceError(503, 'Bu abonelik yönetimi henüz etkin değil.', 'provider_unconfigured');
  const row = await env.DB.prepare('SELECT customer_id, subscription_id FROM commerce_subscriptions WHERE provider = ? AND member_id = ? ORDER BY updated_at DESC LIMIT 1').bind(provider, member.id).first();
  if (!row) throw new CommerceError(404, 'Bu sağlayıcıda aboneliğiniz bulunamadı.', 'subscription_missing');
  if (provider === 'stripe') { const session = await providerFetch(env, 'stripe', 'billing_portal/sessions', { body: { customer: row.customer_id, configuration: env.STRIPE_PORTAL_CONFIGURATION_ID, return_url: origin + '/uyelik/' } }); return { url: externalUrl(session.url, provider, true) }; }
  const result = await providerFetch(env, 'lemon', 'subscriptions/' + row.subscription_id);
  return { url: externalUrl(result.data.attributes.urls.customer_portal, provider, true) };
}
function projectFields(body) {
  const title = text(body.title, 'Başlık', 100), kind = text(body.kind, 'Proje türü', 40);
  if (!/^[a-z][a-z0-9-]{0,39}$/.test(kind) || !body.body || typeof body.body !== 'object' || Array.isArray(body.body)) throw new CommerceError(400, 'Proje türü veya JSON gövdesi geçersiz.', 'invalid_project');
  const value = JSON.stringify(body.body); if (encoder.encode(value).byteLength > 65536) throw new CommerceError(413, 'Bir proje en fazla 64 KB veri saklayabilir.', 'project_too_large');
  return { title, kind, value };
}

export async function onRequest(context) {
  const { request, env } = context;
  try {
    if (!env.DB) throw new CommerceError(503, 'Üyelik kayıt hizmeti henüz etkin değil.', 'database_unavailable');
    const url = new URL(request.url), route = url.pathname.replace(/^\/api\/commerce\/?/, '').replace(/\/+$/, ''), parts = route.split('/');
    if (parts[0] === 'webhooks' && parts.length === 2 && ['stripe', 'lemon'].includes(parts[1])) { if (request.method !== 'POST') throw new CommerceError(405, 'Bildirim POST olmalı.', 'method'); return await webhook(request, env, parts[1]); }
    if (!['GET', 'POST'].includes(request.method)) throw new CommerceError(405, 'Bu yöntem desteklenmiyor.', 'method');
    if (request.method === 'POST' && request.headers.get('origin') !== url.origin) throw new CommerceError(403, 'İşlemi sitenin kendi sayfasından gönderin.', 'origin');
    const ip = await hash((env.COMMERCE_RATE_SALT || 'binsite-commerce-v1') + ':' + (request.headers.get('CF-Connecting-IP') || 'local'));
    await limit(env.DB, ip, request.method === 'GET' ? 'read' : 'write', request.method === 'GET' ? 90 : 30);
    const member = await accountFromRequest(request, env.DB);
    if (request.method === 'GET' && route === 'status') {
      const projects = member ? (await env.DB.prepare('SELECT id, title, kind, updated_at AS updatedAt FROM commerce_projects WHERE member_id = ? ORDER BY updated_at DESC LIMIT 100').bind(member.id).all()).results : [];
      return json({ account: await accountStatus(env.DB, member), providers: providers(env), projects, plans: [planInfo(env)], configurationVersion: env.COMMERCE_CONFIG_VERSION || null, providerModes: { stripe: env.STRIPE_SECRET_KEY ? env.STRIPE_SECRET_KEY.startsWith('sk_live_') ? 'live' : 'test' : null, lemon: env.LEMON_API_KEY ? env.LEMON_TEST_MODE === 'true' ? 'test' : 'live' : null }, paymentMode: env.STRIPE_SECRET_KEY?.startsWith('sk_live_') || (env.LEMON_API_KEY && env.LEMON_TEST_MODE !== 'true') ? 'live' : 'test' });
    }
    if (request.method === 'GET' && parts[0] === 'projects') {
      if (!member) throw new CommerceError(401, 'Önce hesap oluşturun veya kurtarma anahtarınızla giriş yapın.', 'authentication');
      if (parts.length === 1) return json({ projects: (await env.DB.prepare('SELECT id, title, kind, updated_at AS updatedAt FROM commerce_projects WHERE member_id = ? ORDER BY updated_at DESC LIMIT 100').bind(member.id).all()).results });
      if (parts.length === 2) { const row = await env.DB.prepare('SELECT id, title, kind, body, updated_at AS updatedAt FROM commerce_projects WHERE id = ? AND member_id = ?').bind(requireId(parts[1]), member.id).first(); if (!row) throw new CommerceError(404, 'Proje bulunamadı.', 'not_found'); return json({ project: { ...row, body: JSON.parse(row.body) } }); }
    }
    if (request.method !== 'POST') throw new CommerceError(404, 'Adres bulunamadı.', 'not_found');
    const body = await bodyJson(request);
    if (route === 'account') {
      if (member) throw new CommerceError(409, 'Bu tarayıcıda zaten bir hesap açık.', 'already_signed_in');
      await limit(env.DB, ip, 'account', 4, 3600);
      const id = crypto.randomUUID(), recoveryKey = 'bin-' + randomToken(), now = Date.now();
      await env.DB.prepare('INSERT INTO commerce_members (id, recovery_hash, created_at) VALUES (?, ?, ?)').bind(id, await hash(recoveryKey), now).run();
      return json({ recoveryKey }, 201, await startSession(env.DB, id));
    }
    if (route === 'recover') {
      await limit(env.DB, ip, 'recover', 10, 3600);
      if (typeof body.recoveryKey !== 'string' || !/^bin-[A-Za-z0-9_-]{43}$/.test(body.recoveryKey.trim())) throw new CommerceError(401, 'Kurtarma anahtarı bulunamadı.', 'recovery_failed');
      const account = await env.DB.prepare('SELECT id FROM commerce_members WHERE recovery_hash = ?').bind(await hash(body.recoveryKey.trim())).first();
      if (!account) throw new CommerceError(401, 'Kurtarma anahtarı bulunamadı.', 'recovery_failed');
      return json({ ok: true }, 200, await startSession(env.DB, account.id, true));
    }
    if (!member) throw new CommerceError(401, 'Önce hesap oluşturun veya kurtarma anahtarınızla giriş yapın.', 'authentication');
    if (route === 'logout') { const match = (request.headers.get('cookie') || '').split(';').map(value => value.trim()).find(value => value.startsWith(COOKIE + '=')); await env.DB.prepare('DELETE FROM commerce_sessions WHERE session_hash = ?').bind(await hash(match.slice(COOKIE.length + 1))).run(); return json({ ok: true }, 200, sessionCookie('', true)); }
    if (route === 'checkout') { await limit(env.DB, member.id, 'checkout', 5, 3600); return json(await checkout(request, env, member, body, url.origin)); }
    if (route === 'portal') { await limit(env.DB, member.id, 'portal', 10, 3600); return json(await portal(env, member, body.provider, url.origin)); }
    if (parts[0] === 'projects') {
      if (parts.length === 3 && parts[2] === 'delete') { const result = await env.DB.prepare('DELETE FROM commerce_projects WHERE id = ? AND member_id = ?').bind(requireId(parts[1]), member.id).run(); if (!result.meta.changes) throw new CommerceError(404, 'Proje bulunamadı.', 'not_found'); return json({ ok: true }); }
      if (parts.length === 1) {
        const fields = projectFields(body), id = crypto.randomUUID(), now = Date.now();
        const result = await env.DB.prepare('INSERT INTO commerce_projects (id, member_id, title, kind, body, created_at, updated_at) SELECT ?, ?, ?, ?, ?, ?, ? WHERE (SELECT COUNT(*) FROM commerce_projects WHERE member_id = ?) < CASE WHEN EXISTS(SELECT 1 FROM commerce_subscriptions WHERE member_id = ? AND premium = 1 AND expires_at > ?) THEN 100 ELSE 3 END').bind(id, member.id, fields.title, fields.kind, fields.value, now, now, member.id, member.id, now).run();
        if (!result.meta.changes) throw new CommerceError(409, 'Proje sınırına ulaştınız. Ücretsiz hesap 3, aktif üyelik 100 proje saklayabilir.', 'project_limit'); return json({ id }, 201);
      }
      if (parts.length === 2) { const fields = projectFields(body), id = requireId(parts[1]); const result = await env.DB.prepare('UPDATE commerce_projects SET title = ?, kind = ?, body = ?, updated_at = ? WHERE id = ? AND member_id = ?').bind(fields.title, fields.kind, fields.value, Date.now(), id, member.id).run(); if (!result.meta.changes) throw new CommerceError(404, 'Proje bulunamadı.', 'not_found'); return json({ id }); }
    }
    throw new CommerceError(404, 'Adres bulunamadı.', 'not_found');
  } catch (error) {
    if (error instanceof CommerceError) return json({ error: error.message, code: error.code }, error.status);
    console.error('Commerce request failed', { name: error?.name || 'Error' });
    return json({ error: 'İşlem tamamlanamadı. Biraz sonra yeniden deneyin.', code: 'internal_error' }, 500);
  }
}
