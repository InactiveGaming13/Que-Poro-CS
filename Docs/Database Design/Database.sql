DROP TABLE IF EXISTS config;
DROP TABLE IF EXISTS banned_phrase_links;
DROP TABLE IF EXISTS banned_phrases;
DROP TABLE IF EXISTS media;
DROP TABLE IF EXISTS role_reactions;
DROP TABLE IF EXISTS message_reactions;
DROP TABLE IF EXISTS temp_vcs;
DROP TABLE IF EXISTS user_stats;
DROP TABLE IF EXISTS responses;
DROP TABLE IF EXISTS channels CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS guilds CASCADE;

CREATE
    EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS guilds
(
    id                           NUMERIC PRIMARY KEY NOT NULL,
    created_at                   TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    name                         TEXT                NOT NULL,
    temp_vc_channel              NUMERIC,
    temp_vc_enabled              BOOLEAN             NOT NULL DEFAULT TRUE,
    temp_vc_default_member_limit INTEGER             NOT NULL DEFAULT 5,
    temp_vc_default_bitrate      INTEGER             NOT NULL DEFAULT 64,
    roblox_alert_channel         NUMERIC,
    roblox_alert_enabled         BOOLEAN             NOT NULL DEFAULT TRUE,
    roblox_alert_interval        INTEGER             NOT NULL DEFAULT 60
);

CREATE TABLE IF NOT EXISTS channels
(
    id         NUMERIC PRIMARY KEY NOT NULL,
    created_at TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    name       TEXT                NOT NULL,
    guild_id   NUMERIC             NOT NULL
        CONSTRAINT channels_guild_id_fk
            REFERENCES guilds,
    topic      TEXT
);

CREATE TABLE IF NOT EXISTS users
(
    id          NUMERIC PRIMARY KEY NOT NULL,
    created_at  TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    username    TEXT                NOT NULL,
    global_name TEXT,
    admin       BOOLEAN             NOT NULL DEFAULT FALSE,
    replied_to  BOOLEAN             NOT NULL DEFAULT TRUE,
    reacted_to  BOOLEAN             NOT NULL DEFAULT TRUE,
    banned      BOOLEAN             NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS user_stats
(
    id              NUMERIC   NOT NULL
        CONSTRAINT user_stats_user_id_fk
            REFERENCES users,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_modified   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    guild_id        NUMERIC   NOT NULL
        CONSTRAINT user_stats_guild_id_fk
            REFERENCES guilds,
    channel_id      NUMERIC   NOT NULL
        CONSTRAINT user_stats_channel_id_fk
            REFERENCES channels,
    tracked         BOOLEAN   NOT NULL DEFAULT TRUE,
    sent            INTEGER   NOT NULL DEFAULT 0,
    temp_vc_created INTEGER   NOT NULL DEFAULT 0,
    mod_actions     INTEGER   NOT NULL DEFAULT 0,
    strikes         INTEGER   NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS temp_vcs
(
    id         NUMERIC PRIMARY KEY NOT NULL,
    created_at TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by NUMERIC             NOT NULL
        CONSTRAINT temp_vcs_created_by_fk
            REFERENCES users,
    guild_id   NUMERIC             NOT NULL
        CONSTRAINT temp_vcs_guild_id_fk
            REFERENCES guilds,
    master     NUMERIC             NOT NULL
        CONSTRAINT temp_vcs_master_fk
            REFERENCES users,
    name       TEXT                NOT NULL,
    bitrate    INTEGER             NOT NULL,
    user_limit INTEGER             NOT NULL,
    user_count INTEGER             NOT NULL,
    user_queue TEXT
);

CREATE TABLE IF NOT EXISTS config
(
    created_at                   TIMESTAMP NOT NULL,
    last_modified                TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status_type                  INTEGER   NOT NULL DEFAULT 0,
    status_message               TEXT      NOT NULL DEFAULT '',
    log_channel                  NUMERIC   NOT NULL DEFAULT 0,
    temp_vc_enabled              BOOLEAN   NOT NULL DEFAULT TRUE,
    temp_vc_default_member_limit INTEGER   NOT NULL DEFAULT 5,
    temp_vc_default_bitrate      INTEGER   NOT NULL DEFAULT 64,
    roblox_alerts_enabled        BOOLEAN   NOT NULL DEFAULT TRUE,
    replies_enabled              BOOLEAN   NOT NULL DEFAULT TRUE,
    testers_enabled              BOOLEAN   NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS media
(
    id         uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by NUMERIC          NOT NULL
        CONSTRAINT media_created_by_fk
            REFERENCES users,
    alias      TEXT             NOT NULL UNIQUE,
    category   TEXT,
    url        TEXT             NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS banned_phrases
(
    id         uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by NUMERIC          NOT NULL,
    severity   INTEGER          NOT NULL,
    phrase     TEXT             NOT NULL UNIQUE,
    reason     TEXT,
    enabled    BOOLEAN          NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS banned_phrase_links
(
    id               uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at       TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    banned_phrase_id uuid             NOT NULL
        CONSTRAINT banned_phrase_links_banned_phrase_id_fk
            REFERENCES banned_phrases,
    channel_id       NUMERIC
        CONSTRAINT banned_phrase_links_channel_id_fk
            REFERENCES channels,
    guild_id         NUMERIC
        CONSTRAINT banned_phrase_links_guild_id_fk
            REFERENCES guilds
);

CREATE TABLE IF NOT EXISTS message_reactions
(
    id            uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at    TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    NUMERIC          NOT NULL
        CONSTRAINT message_reactions_created_by_id_fk
            REFERENCES users,
    emoji_code    TEXT             NOT NULL,
    reacts_to     NUMERIC
        CONSTRAINT message_reactions_reacts_to_fk
            REFERENCES users,
    trigger       TEXT,
    exact_trigger BOOLEAN          NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS role_reactions
(
    id            uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at    TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    NUMERIC          NOT NULL
        CONSTRAINT role_reactions_created_by_id_fk
            REFERENCES users,
    guild_id      NUMERIC          NOT NULL
        CONSTRAINT role_reactions_guild_id_fk
            REFERENCES guilds,
    channel_id    NUMERIC          NOT NULL
        CONSTRAINT role_reactions_channel_id_fk
            REFERENCES channels,
    message_link  TEXT             NOT NULL,
    role_id       NUMERIC          NOT NULL,
    reaction_code TEXT             NOT NULL
);

CREATE TABLE IF NOT EXISTS responses
(
    id             uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at     TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by     NUMERIC          NOT NULL
        CONSTRAINT responses_created_by_fk
            REFERENCES users,
    user_id        NUMERIC
        CONSTRAINT responses_user_id_fk
            REFERENCES users,
    channel_id     NUMERIC
        CONSTRAINT responses_channel_id_fk
            REFERENCES channels,
    trigger        TEXT             NOT NULL,
    response       TEXT,
    media_alias    TEXT,
    media_category TEXT,
    exact          bool             NOT NULL DEFAULT FALSE,
    enabled        bool             NOT NULL DEFAULT TRUE
);