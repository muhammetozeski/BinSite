PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS stories (
  id TEXT PRIMARY KEY,
  title TEXT NOT NULL,
  creator_hash TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  hidden INTEGER NOT NULL DEFAULT 0 CHECK (hidden IN (0, 1))
);
CREATE TABLE IF NOT EXISTS story_nodes (
  id TEXT PRIMARY KEY,
  story_id TEXT NOT NULL REFERENCES stories(id),
  parent_id TEXT REFERENCES story_nodes(id),
  body TEXT NOT NULL,
  depth INTEGER NOT NULL CHECK (depth BETWEEN 0 AND 24),
  creator_hash TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  hidden INTEGER NOT NULL DEFAULT 0 CHECK (hidden IN (0, 1))
);
CREATE INDEX IF NOT EXISTS story_nodes_story ON story_nodes(story_id, created_at);
CREATE INDEX IF NOT EXISTS stories_recent ON stories(hidden, created_at DESC);

CREATE TABLE IF NOT EXISTS polls (
  id TEXT PRIMARY KEY,
  question TEXT NOT NULL,
  option_a TEXT NOT NULL,
  option_b TEXT NOT NULL,
  creator_hash TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  hidden INTEGER NOT NULL DEFAULT 0 CHECK (hidden IN (0, 1))
);
CREATE TABLE IF NOT EXISTS poll_votes (
  poll_id TEXT NOT NULL REFERENCES polls(id),
  visitor_hash TEXT NOT NULL,
  choice INTEGER NOT NULL CHECK (choice IN (0, 1)),
  created_at INTEGER NOT NULL,
  PRIMARY KEY (poll_id, visitor_hash)
);
CREATE INDEX IF NOT EXISTS polls_recent ON polls(hidden, created_at DESC);

CREATE TABLE IF NOT EXISTS capsules (
  id TEXT PRIMARY KEY,
  title TEXT NOT NULL,
  body TEXT NOT NULL,
  unlock_at INTEGER NOT NULL,
  creator_hash TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  hidden INTEGER NOT NULL DEFAULT 0 CHECK (hidden IN (0, 1))
);

CREATE TABLE IF NOT EXISTS social_reports (
  kind TEXT NOT NULL CHECK (kind IN ('story', 'node', 'poll', 'capsule')),
  target_id TEXT NOT NULL,
  ip_hash TEXT NOT NULL,
  reason TEXT NOT NULL,
  created_at INTEGER NOT NULL,
  PRIMARY KEY (kind, target_id, ip_hash)
);
CREATE TABLE IF NOT EXISTS social_rate_limits (
  rate_key TEXT PRIMARY KEY,
  count INTEGER NOT NULL DEFAULT 1,
  expires_at INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS social_rate_expiry ON social_rate_limits(expires_at);
