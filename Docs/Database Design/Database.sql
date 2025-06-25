DROP TABLE IF EXISTS config;
DROP TABLE IF EXISTS banned_phrase_channels;
DROP TABLE IF EXISTS banned_phrases;
DROP TABLE IF EXISTS media;
DROP TABLE IF EXISTS reactions;
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
    tracked                      BOOLEAN             NOT NULL DEFAULT TRUE,
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
    id          NUMERIC PRIMARY KEY NOT NULL,
    created_at  TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    name        TEXT                NOT NULL,
    tracked     BOOLEAN             NOT NULL DEFAULT TRUE,
    guild_id    NUMERIC             NOT NULL
        CONSTRAINT guild_id_fk
            REFERENCES guilds,
    description TEXT,
    messages    INTEGER             NOT NULL DEFAULT 0
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
    tracked     BOOLEAN             NOT NULL DEFAULT TRUE,
    banned      BOOLEAN             NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS user_stats
(
    id              NUMERIC   NOT NULL
        CONSTRAINT user_stats_user_id_fk
            REFERENCES users,
    created_at      TIMESTAMP NOT NULL,
    last_modified   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    guild_id        NUMERIC   NOT NULL
        CONSTRAINT user_stats_guild_id_fk
            REFERENCES guilds,
    channel_id      NUMERIC   NOT NULL
        CONSTRAINT user_stats_channel_id_fk
            REFERENCES channels,
    sent            INTEGER   NOT NULL DEFAULT 0,
    deleted         INTEGER   NOT NULL DEFAULT 0,
    edited          INTEGER   NOT NULL DEFAULT 0,
    temp_vc_created INTEGER   NOT NULL DEFAULT 0,
    mod_actions     INTEGER   NOT NULL DEFAULT 0,
    strikes         INTEGER   NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS temp_vcs
(
    id         NUMERIC PRIMARY KEY NOT NULL,
    created_at TIMESTAMP           NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by NUMERIC             NOT NULL,
    guild_id   NUMERIC             NOT NULL
        CONSTRAINT guild_id_tv_fk
            REFERENCES guilds,
    master     NUMERIC             NOT NULL
        CONSTRAINT user_id_tv_fk
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
    testers_enabled              BOOLEAN   NOT NULL DEFAULT FALSE,
    shutdown_channel             NUMERIC   NOT NULL DEFAULT 0,
    shutdown_message             NUMERIC   NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS media
(
    id         uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by NUMERIC          NOT NULL
        CONSTRAINT created_id_fk
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
    enabled    BOOLEAN          NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS banned_phrase_channels
(
    id               uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at       TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    banned_phrase_id uuid             NOT NULL
        CONSTRAINT banned_phrase_id_fk
            REFERENCES banned_phrases,
    channel_id       NUMERIC
        CONSTRAINT channel_id_fk
            REFERENCES channels
);

CREATE TABLE IF NOT EXISTS reactions
(
    id            uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at    TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    NUMERIC          NOT NULL
        CONSTRAINT created_by_id_fk
            REFERENCES users,
    emoji_code    TEXT             NOT NULL,
    reacts_to     NUMERIC
        CONSTRAINT reacts_id_fk
            REFERENCES users,
    trigger       TEXT,
    exact_trigger BOOLEAN          NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS responses
(
    id             uuid PRIMARY KEY NOT NULL DEFAULT uuid_generate_v4(),
    created_at     TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by     NUMERIC          NOT NULL
        CONSTRAINT created_by_response_fk
            REFERENCES users,
    user_id        NUMERIC
        CONSTRAINT user_id_response_fk
            REFERENCES users,
    channel_id     NUMERIC
        CONSTRAINT channel_id_response_fk
            REFERENCES channels,
    trigger        TEXT             NOT NULL,
    response       TEXT,
    media_alias    TEXT,
    media_category TEXT,
    exact          bool             NOT NULL DEFAULT FALSE,
    enabled        bool             NOT NULL DEFAULT TRUE
);