CREATE TABLE IF NOT EXISTS collab_spaces(id TEXT PRIMARY KEY,kind TEXT NOT NULL,title TEXT NOT NULL,owner TEXT NOT NULL,state TEXT NOT NULL,revision INTEGER NOT NULL DEFAULT 0,created_at INTEGER NOT NULL,hidden INTEGER NOT NULL DEFAULT 0);
CREATE INDEX IF NOT EXISTS collab_space_directory ON collab_spaces(kind,hidden,created_at DESC);
CREATE TABLE IF NOT EXISTS collab_rates(rate_key TEXT PRIMARY KEY,count INTEGER NOT NULL,expires_at INTEGER NOT NULL);
CREATE INDEX IF NOT EXISTS collab_rate_expiry ON collab_rates(expires_at);
CREATE TABLE IF NOT EXISTS collab_reports(kind TEXT NOT NULL,target_id TEXT NOT NULL,network TEXT NOT NULL,reason TEXT NOT NULL,created_at INTEGER NOT NULL,PRIMARY KEY(kind,target_id,network));
