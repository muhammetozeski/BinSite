PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS commerce_members (
  id TEXT PRIMARY KEY,
  recovery_hash TEXT NOT NULL UNIQUE,
  created_at INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS commerce_sessions (
  session_hash TEXT PRIMARY KEY,
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  created_at INTEGER NOT NULL,
  expires_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS commerce_sessions_member ON commerce_sessions(member_id);
CREATE INDEX IF NOT EXISTS commerce_sessions_expiry ON commerce_sessions(expires_at);

CREATE TABLE IF NOT EXISTS commerce_checkouts (
  id TEXT PRIMARY KEY,
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  provider TEXT NOT NULL CHECK (provider IN ('stripe', 'lemon')),
  plan_id TEXT NOT NULL,
  provider_checkout_id TEXT,
  created_at INTEGER NOT NULL,
  UNIQUE(provider, provider_checkout_id)
);
CREATE INDEX IF NOT EXISTS commerce_checkouts_member ON commerce_checkouts(member_id, created_at DESC);

CREATE TABLE IF NOT EXISTS commerce_customers (
  provider TEXT NOT NULL CHECK (provider IN ('stripe', 'lemon')),
  customer_id TEXT NOT NULL,
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  PRIMARY KEY(provider, customer_id)
);
CREATE INDEX IF NOT EXISTS commerce_customers_member ON commerce_customers(member_id, provider);

CREATE TABLE IF NOT EXISTS commerce_subscriptions (
  provider TEXT NOT NULL CHECK (provider IN ('stripe', 'lemon')),
  subscription_id TEXT NOT NULL,
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  customer_id TEXT NOT NULL,
  plan_id TEXT NOT NULL,
  status TEXT NOT NULL,
  premium INTEGER NOT NULL CHECK (premium IN (0, 1)),
  expires_at INTEGER NOT NULL,
  cancel_at_period_end INTEGER NOT NULL DEFAULT 0 CHECK (cancel_at_period_end IN (0, 1)),
  period_id TEXT NOT NULL DEFAULT '',
  updated_at INTEGER NOT NULL,
  PRIMARY KEY(provider, subscription_id)
);
CREATE INDEX IF NOT EXISTS commerce_subscriptions_member ON commerce_subscriptions(member_id, premium, expires_at);

CREATE TABLE IF NOT EXISTS commerce_revocations (
  provider TEXT NOT NULL,
  subscription_id TEXT NOT NULL,
  period_id TEXT NOT NULL,
  reason TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  PRIMARY KEY(provider, subscription_id, period_id)
);

CREATE TABLE IF NOT EXISTS commerce_events (
  provider TEXT NOT NULL,
  event_id TEXT NOT NULL,
  state TEXT NOT NULL CHECK (state IN ('pending', 'done')),
  lease_token TEXT NOT NULL,
  lease_until INTEGER NOT NULL,
  received_at INTEGER NOT NULL,
  processed_at INTEGER,
  PRIMARY KEY(provider, event_id)
);
CREATE INDEX IF NOT EXISTS commerce_events_received ON commerce_events(received_at);

CREATE TABLE IF NOT EXISTS commerce_locks (
  lock_key TEXT PRIMARY KEY,
  lease_token TEXT NOT NULL,
  expires_at INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS commerce_projects (
  id TEXT PRIMARY KEY,
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  title TEXT NOT NULL CHECK (length(title) BETWEEN 1 AND 100),
  kind TEXT NOT NULL,
  body TEXT NOT NULL CHECK (length(CAST(body AS BLOB)) <= 65536 AND json_valid(body)),
  created_at INTEGER NOT NULL,
  updated_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS commerce_projects_member ON commerce_projects(member_id, updated_at DESC);

CREATE TABLE IF NOT EXISTS commerce_rate_limits (
  rate_key TEXT PRIMARY KEY,
  count INTEGER NOT NULL,
  expires_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS commerce_rate_limits_expiry ON commerce_rate_limits(expires_at);
