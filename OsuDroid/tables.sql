CREATE SEQUENCE IF NOT EXISTS bbl_user_id_seq AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS bbl_token_user
(
    token_id    uuid      NOT NULL,
    user_id     bigint    NOT NULL,
    create_date timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (token_id)
);

CREATE INDEX IF NOT EXISTS idx_bbl_token_user ON bbl_token_user USING brin (create_date);

CREATE TABLE IF NOT EXISTS bbl_patron
(
    patron_email     text    NOT NULL,
    active_supporter boolean NOT NULL DEFAULT false,
    PRIMARY KEY (patron_email)
);



CREATE TABLE IF NOT EXISTS bbl_user
(
    id                   bigint    NOT NULL DEFAULT nextval('bbl_user_id_seq'),
    username             text      NOT NULL UNIQUE,
    password             text      NOT NULL DEFAULT '',
    email                text      NOT NULL UNIQUE,
    deviceid             text      NOT NULL DEFAULT '',
    regist_time          timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_time      timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    region               text      NOT NULL DEFAULT '',
    active               boolean   NOT NULL DEFAULT true,
    banned               boolean   NOT NULL DEFAULT false,
    restrict_mode        boolean   NOT NULL DEFAULT false,
    username_last_change timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    latest_ip            text      NOT NULL DEFAULT '',
    patron_email         text               DEFAULT null,
    patron_email_accept  boolean   NOT NULL DEFAULT false,
    FOREIGN KEY (patron_email) REFERENCES bbl_patron (patron_email),
    PRIMARY KEY (id)
);

ALTER SEQUENCE bbl_user_id_seq OWNED BY bbl_user.id;

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_patron_email ON bbl_user USING btree (patron_email);

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_username ON bbl_user USING btree (lower(username));

CREATE TABLE IF NOT EXISTS bbl_user_stats
(
    uid              bigint NOT NULL,
    playcount        bigint NOT NULL DEFAULT 0,
    overall_score    bigint NOT NULL DEFAULT 0,
    overall_accuracy bigint NOT NULL DEFAULT 0,
    overall_combo    bigint NOT NULL DEFAULT 0,
    overall_xs       bigint NOT NULL DEFAULT 0,
    overall_ss       bigint NOT NULL DEFAULT 0,
    overall_s        bigint NOT NULL DEFAULT 0,
    overall_a        bigint NOT NULL DEFAULT 0,
    overall_b        bigint NOT NULL DEFAULT 0,
    overall_c        bigint NOT NULL DEFAULT 0,
    overall_d        bigint NOT NULL DEFAULT 0,
    overall_hits     bigint NOT NULL DEFAULT 0,
    overall_300      bigint NOT NULL DEFAULT 0,
    overall_100      bigint NOT NULL DEFAULT 0,
    overall_50       bigint NOT NULL DEFAULT 0,
    overall_geki     bigint NOT NULL DEFAULT 0,
    overall_katu     bigint NOT NULL DEFAULT 0,
    overall_miss     bigint NOT NULL DEFAULT 0,
    overall_xss      bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (uid),
    FOREIGN KEY (uid) REFERENCES bbl_user (id)
);

CREATE INDEX IF NOT EXISTS idx_bbl_user_stats_uid ON bbl_user_stats USING brin (uid);



CREATE SEQUENCE IF NOT EXISTS public.bbl_score_id_seq
    AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;



CREATE TABLE IF NOT EXISTS bbl_score
(
    id       bigint    NOT NULL DEFAULT nextval('bbl_score_id_seq'),
    uid      bigint    NOT NULL,
    filename text      NOT NULL,
    hash     text      NOT NULL,
    mode     text      NOT NULL DEFAULT '',
    score    bigint    NOT NULL DEFAULT 0,
    combo    bigint    NOT NULL DEFAULT 0,
    mark     text      NOT NULL DEFAULT '',
    geki     bigint    NOT NULL DEFAULT 0,
    perfect  bigint    NOT NULL DEFAULT 0,
    katu     bigint    NOT NULL DEFAULT 0,
    good     bigint    NOT NULL DEFAULT 0,
    bad      bigint    NOT NULL DEFAULT 0,
    miss     bigint    NOT NULL DEFAULT 0,
    date     timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    accuracy bigint    NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_uid ON bbl_score USING brin (uid);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_uid_filename ON bbl_score USING brin (uid, filename);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_filename ON bbl_score USING brin (filename);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_hash_filename ON bbl_score USING brin (filename, hash);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_user_id_id ON bbl_score USING btree (uid desc , id desc);
ALTER SEQUENCE bbl_score_id_seq OWNED BY bbl_score.id;



CREATE TABLE IF NOT EXISTS bbl_score_pre_submit
(
    id       bigint    NOT NULL DEFAULT nextval('bbl_score_id_seq'),
    uid      bigint    NOT NULL,
    filename text      NOT NULL,
    hash     text      NOT NULL,
    mode     text      NOT NULL DEFAULT '',
    score    bigint    NOT NULL DEFAULT 0,
    combo    bigint    NOT NULL DEFAULT 0,
    mark     text      NOT NULL DEFAULT '',
    geki     bigint    NOT NULL DEFAULT 0,
    perfect  bigint    NOT NULL DEFAULT 0,
    katu     bigint    NOT NULL DEFAULT 0,
    good     bigint    NOT NULL DEFAULT 0,
    bad      bigint    NOT NULL DEFAULT 0,
    miss     bigint    NOT NULL DEFAULT 0,
    date     timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    accuracy bigint    NOT NULL DEFAULT 0,
    PRIMARY KEY (id)
);

ALTER SEQUENCE bbl_score_id_seq OWNED BY bbl_score_pre_submit.id;
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON bbl_score_pre_submit USING brin (uid);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON bbl_score_pre_submit USING brin (uid, id);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_date ON bbl_score_pre_submit USING brin (date);


CREATE TABLE IF NOT EXISTS
    bbl_score_banned
(
    id       bigint PRIMARY KEY NOT NULL,
    uid      bigint             NOT NULL,
    filename text               NOT NULL,
    hash     text               NOT NULL,
    mode     text               NOT NULL DEFAULT '',
    score    bigint             NOT NULL DEFAULT 0,
    combo    bigint             NOT NULL DEFAULT 0,
    mark     text               NOT NULL DEFAULT '',
    geki     bigint             NOT NULL DEFAULT 0,
    perfect  bigint             NOT NULL DEFAULT 0,
    katu     bigint             NOT NULL DEFAULT 0,
    good     bigint             NOT NULL DEFAULT 0,
    bad      bigint             NOT NULL DEFAULT 0,
    miss     bigint             NOT NULL DEFAULT 0,
    date     timestamp          NOT NULL,
    accuracy bigint             NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline
(
    user_id        bigint NOT NULL,
    date           date   NOT NULL DEFAULT current_date,
    global_ranking bigint NOT NULL,
    score          bigint NOT NULL,
    PRIMARY KEY (user_id, date)
);

CREATE INDEX IF NOT EXISTS idx_global_ranking_timeline ON bbl_global_ranking_timeline USING brin (user_id, date);
CREATE INDEX idx_date_btree on bbl_global_ranking_timeline (date desc);


CREATE TABLE IF NOT EXISTS bbl_avatar_hash
(
    user_id BIGINT,
    size    int,
    hash    TEXT,
    PRIMARY KEY (user_id, size)
);