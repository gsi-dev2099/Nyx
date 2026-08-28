CREATE TABLE IF NOT EXISTS user_service.user_preferences (
    id_user      BIGINT PRIMARY KEY REFERENCES user_service.users(id_user),
    theme_name   VARCHAR(50) NOT NULL DEFAULT 'theme-default',
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Index for quick lookups
CREATE INDEX IF NOT EXISTS idx_user_preferences_user ON user_service.user_preferences(id_user);
