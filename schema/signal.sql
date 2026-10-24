PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS signal_challenges (
 id TEXT PRIMARY KEY,owner_id TEXT NOT NULL REFERENCES commerce_members(id),definition TEXT NOT NULL,
 created_at INTEGER NOT NULL,withdrawn_at INTEGER,last_write_id TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS signal_challenge_owner ON signal_challenges(owner_id,withdrawn_at);
CREATE TABLE IF NOT EXISTS signal_attempts (
 id TEXT PRIMARY KEY,guest_hash TEXT NOT NULL,snapshot TEXT NOT NULL,candidates TEXT NOT NULL,
 query_limit INTEGER NOT NULL,used_queries INTEGER NOT NULL DEFAULT 0,revision INTEGER NOT NULL DEFAULT 1,
 created_at INTEGER NOT NULL,expires_at INTEGER NOT NULL,finished_at INTEGER,result TEXT,last_write_id TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS signal_attempt_guest ON signal_attempts(guest_hash,expires_at);
CREATE INDEX IF NOT EXISTS signal_attempt_expiry ON signal_attempts(expires_at);
CREATE TABLE IF NOT EXISTS signal_actions (
 attempt_id TEXT NOT NULL REFERENCES signal_attempts(id) ON DELETE CASCADE,request_id TEXT NOT NULL,
 kind TEXT NOT NULL,payload_hash TEXT NOT NULL,response TEXT NOT NULL,sequence INTEGER NOT NULL,created_at INTEGER NOT NULL,
 PRIMARY KEY(attempt_id,request_id)
);
CREATE TABLE IF NOT EXISTS signal_requests (
 owner_key TEXT NOT NULL,action TEXT NOT NULL,request_id TEXT NOT NULL,payload_hash TEXT NOT NULL,
 response TEXT NOT NULL,status INTEGER NOT NULL,created_at INTEGER NOT NULL,
 PRIMARY KEY(owner_key,action,request_id)
);
CREATE INDEX IF NOT EXISTS signal_request_expiry ON signal_requests(created_at);
CREATE TABLE IF NOT EXISTS signal_reports (
 challenge_id TEXT NOT NULL,member_id TEXT NOT NULL REFERENCES commerce_members(id),reason TEXT NOT NULL,
 note TEXT NOT NULL,created_at INTEGER NOT NULL,resolved_at INTEGER,PRIMARY KEY(challenge_id,member_id)
);
CREATE TABLE IF NOT EXISTS signal_moderation (challenge_id TEXT PRIMARY KEY,hidden_at INTEGER NOT NULL);
