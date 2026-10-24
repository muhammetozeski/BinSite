PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS commerce_workspaces (
  id TEXT PRIMARY KEY,
  owner_id TEXT NOT NULL REFERENCES commerce_members(id),
  kind TEXT NOT NULL CHECK (kind IN ('catalog','review','field')),
  title TEXT NOT NULL CHECK (length(title) BETWEEN 1 AND 100),
  body TEXT NOT NULL CHECK (json_valid(body) AND length(CAST(body AS BLOB)) <= 524288),
  revision INTEGER NOT NULL DEFAULT 1,
  created_at INTEGER NOT NULL,
  updated_at INTEGER NOT NULL,
  archived_at INTEGER,
  last_write_id TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS commerce_workspaces_owner ON commerce_workspaces(owner_id,archived_at,updated_at DESC);
CREATE TABLE IF NOT EXISTS commerce_workspace_members (
  workspace_id TEXT NOT NULL REFERENCES commerce_workspaces(id),
  member_id TEXT NOT NULL REFERENCES commerce_members(id),
  role TEXT NOT NULL CHECK (role IN ('viewer','editor')),
  created_at INTEGER NOT NULL,
  PRIMARY KEY(workspace_id,member_id)
);
CREATE INDEX IF NOT EXISTS commerce_workspace_member_lookup ON commerce_workspace_members(member_id,workspace_id);
CREATE TABLE IF NOT EXISTS commerce_workspace_versions (
  workspace_id TEXT NOT NULL REFERENCES commerce_workspaces(id),
  revision INTEGER NOT NULL,
  title TEXT NOT NULL,
  body TEXT NOT NULL CHECK (json_valid(body)),
  author_id TEXT NOT NULL REFERENCES commerce_members(id),
  created_at INTEGER NOT NULL,
  PRIMARY KEY(workspace_id,revision)
);
CREATE TABLE IF NOT EXISTS commerce_workspace_invites (
  token_hash TEXT PRIMARY KEY,
  workspace_id TEXT NOT NULL REFERENCES commerce_workspaces(id),
  role TEXT NOT NULL CHECK (role IN ('viewer','editor')),
  created_by TEXT NOT NULL REFERENCES commerce_members(id),
  created_at INTEGER NOT NULL,
  expires_at INTEGER NOT NULL,
  claimed_by TEXT REFERENCES commerce_members(id),
  claim_id TEXT
);
CREATE INDEX IF NOT EXISTS commerce_workspace_invite_expiry ON commerce_workspace_invites(expires_at);
