PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS procedure_playbooks (
 id TEXT PRIMARY KEY,
 owner_id TEXT NOT NULL REFERENCES commerce_members(id),
 author_alias TEXT NOT NULL,
 title TEXT NOT NULL,
 summary TEXT NOT NULL,
 category TEXT NOT NULL,
 tags TEXT NOT NULL,
 current_revision INTEGER NOT NULL DEFAULT 1,
 parent_id TEXT,
 parent_revision INTEGER,
 created_at INTEGER NOT NULL,
 updated_at INTEGER NOT NULL,
 withdrawn_at INTEGER,
 hidden_at INTEGER,
 last_write_id TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS procedure_public ON procedure_playbooks(withdrawn_at,hidden_at,updated_at DESC);
CREATE INDEX IF NOT EXISTS procedure_owner ON procedure_playbooks(owner_id,updated_at DESC);
CREATE TABLE IF NOT EXISTS procedure_versions (
 playbook_id TEXT NOT NULL REFERENCES procedure_playbooks(id),
 revision INTEGER NOT NULL,
 document TEXT NOT NULL,
 author_alias TEXT NOT NULL,
 published_at INTEGER NOT NULL,
 PRIMARY KEY(playbook_id,revision)
);
CREATE TABLE IF NOT EXISTS procedure_outcomes (
 playbook_id TEXT NOT NULL,
 revision INTEGER NOT NULL,
 member_id TEXT NOT NULL REFERENCES commerce_members(id),
 kind TEXT NOT NULL CHECK(kind IN ('completed','partial','blocked')),
 note TEXT NOT NULL,
 updated_at INTEGER NOT NULL,
 PRIMARY KEY(playbook_id,revision,member_id),
 FOREIGN KEY(playbook_id,revision) REFERENCES procedure_versions(playbook_id,revision)
);
CREATE TABLE IF NOT EXISTS procedure_reports (
 playbook_id TEXT NOT NULL REFERENCES procedure_playbooks(id),
 member_id TEXT NOT NULL REFERENCES commerce_members(id),
 reason TEXT NOT NULL CHECK(reason IN ('spam','unsafe','rights','privacy')),
 note TEXT NOT NULL,
 created_at INTEGER NOT NULL,
 resolved_at INTEGER,
 PRIMARY KEY(playbook_id,member_id)
);
CREATE INDEX IF NOT EXISTS procedure_reports_open ON procedure_reports(resolved_at,created_at DESC);
