CREATE TABLE IF NOT EXISTS bbl_patron
(
    patron_email     text    NOT NULL,
    active_supporter boolean NOT NULL DEFAULT false,
    PRIMARY KEY (patron_email)
);

CREATE SEQUENCE IF NOT EXISTS bbl_user_id_seq AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

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




CREATE TABLE IF NOT EXISTS bbl_token_user
(
    token_id    uuid      NOT NULL,
    user_id     bigint    NOT NULL REFERENCES bbl_user(id) ON DELETE CASCADE,
    create_date timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (token_id)
);

CREATE INDEX IF NOT EXISTS idx_bbl_token_user ON bbl_token_user USING brin (create_date);







ALTER SEQUENCE bbl_user_id_seq OWNED BY bbl_user.id;

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_patron_email ON bbl_user USING btree (patron_email);

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_username ON bbl_user USING btree (lower(username));

CREATE TABLE IF NOT EXISTS bbl_user_stats
(
    uid               bigint NOT NULL REFERENCES bbl_user(id) ON DELETE CASCADE,
    overall_playcount bigint NOT NULL DEFAULT 0,
    overall_score     bigint NOT NULL DEFAULT 0,
    overall_accuracy  bigint NOT NULL DEFAULT 0,
    overall_combo     bigint NOT NULL DEFAULT 0,
    overall_xss       bigint NOT NULL DEFAULT 0,
    overall_xs        bigint NOT NULL DEFAULT 0,
    overall_ss        bigint NOT NULL DEFAULT 0,
    overall_s         bigint NOT NULL DEFAULT 0,
    overall_a         bigint NOT NULL DEFAULT 0,
    overall_b         bigint NOT NULL DEFAULT 0,
    overall_c         bigint NOT NULL DEFAULT 0,
    overall_d         bigint NOT NULL DEFAULT 0,
    overall_hits      bigint NOT NULL DEFAULT 0,
    overall_300       bigint NOT NULL DEFAULT 0,
    overall_100       bigint NOT NULL DEFAULT 0,
    overall_50        bigint NOT NULL DEFAULT 0,
    overall_geki      bigint NOT NULL DEFAULT 0,
    overall_katu      bigint NOT NULL DEFAULT 0,
    overall_miss      bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (uid)
);

CREATE INDEX IF NOT EXISTS idx_bbl_user_stats_score_many ON bbl_user_stats(overall_score) INCLUDE (uid);



CREATE SEQUENCE IF NOT EXISTS public.bbl_score_id_seq
    AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;



CREATE TABLE IF NOT EXISTS bbl_score
(
    id       bigint    NOT NULL DEFAULT nextval('bbl_score_id_seq'),
    uid      bigint    NOT NULL REFERENCES bbl_user(id) ON DELETE CASCADE,
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
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_user_id_id ON bbl_score USING btree (uid desc , id desc) INCLUDE (
    filename,
    hash,
    mode,
    score,
    combo,
    mark,
    geki,
    perfect,
    katu,
    good,
    bad,
    miss,
    date,
    accuracy);

ALTER SEQUENCE bbl_score_id_seq OWNED BY bbl_score.id;



CREATE TABLE IF NOT EXISTS bbl_score_pre_submit
(
    id       bigint    NOT NULL DEFAULT nextval('bbl_score_id_seq'),
    uid      bigint    NOT NULL REFERENCES bbl_user(id) ON DELETE CASCADE,
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
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON bbl_score_pre_submit(uid) INCLUDE (id);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON bbl_score_pre_submit(id) INCLUDE (uid);


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
    user_id        bigint NOT NULL REFERENCES bbl_user(id) ON DELETE CASCADE,
    date           date   NOT NULL DEFAULT current_date,
    global_ranking bigint NOT NULL,
    score          bigint NOT NULL,
    PRIMARY KEY (user_id, date)
) PARTITION BY RANGE (date);



CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2010 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2010-01-01') TO ('2011-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2011 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2011-01-01') TO ('2012-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2012 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2012-01-01') TO ('2013-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2013 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2013-01-01') TO ('2014-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2014 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2014-01-01') TO ('2015-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2015 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2015-01-01') TO ('2016-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2016 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2016-01-01') TO ('2017-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2017 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2017-01-01') TO ('2018-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2018 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2018-01-01') TO ('2019-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2019 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2019-01-01') TO ('2020-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2020 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2020-01-01') TO ('2021-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2021 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2021-01-01') TO ('2022-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2022 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2022-01-01') TO ('2023-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2023 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2024 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2025 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2026 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2027 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2027-01-01') TO ('2028-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2028 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2028-01-01') TO ('2029-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2029 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2029-01-01') TO ('2030-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2030 PARTITION OF bbl_global_ranking_timeline FOR VALUES FROM ('2030-01-01') TO ('2031-01-01');

CREATE TABLE IF NOT EXISTS bbl_avatar_hash
(
    user_id BIGINT REFERENCES bbl_user(id) ON DELETE CASCADE,
    size    int,
    hash    TEXT,
    PRIMARY KEY (user_id, size)
);

-- Only IN Production (Need Space)
-- CREATE INDEX IF NOT EXISTS idx_bbl_global_ranking_timeline_any
--     on bbl_global_ranking_timeline (user_id, date) INCLUDE (score, global_ranking);


CREATE TABLE IF NOT EXISTS log(
    id uuid NOT NULL ,
    date_time timestamp NOT NULL,
    message text NOT NULL,
    status text NOT NULL,
    stack text NOT NULL,
    trigger text NOT NULL,
    PRIMARY KEY (id, date_time)
);



CREATE TABLE IF NOT EXISTS
    group_privilege (
                        id uuid NOT NULL DEFAULT gen_random_uuid(),
                        name text UNIQUE NOT NULL ,
                        description text NOT NULL ,
                        PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS
    bbl_user_group_privilege (
                                 user_id BIGINT          NOT NULL REFERENCES bbl_user(id)        ON DELETE CASCADE,
                                 group_privilege_id uuid NOT NULL REFERENCES group_privilege(id) ON DELETE CASCADE,
                                 PRIMARY KEY (user_id, group_privilege_id),
                                 FOREIGN KEY (user_id) REFERENCES bbl_user (id),
                                 FOREIGN KEY (group_privilege_id) REFERENCES group_privilege (id)
);

CREATE TABLE IF NOT EXISTS
    privilege(
                 id uuid DEFAULT gen_random_uuid() NOT NULL,
                 name text UNIQUE NOT NULL,
                 description text NOT NULL,
                 PRIMARY KEY (id)          
);

CREATE TABLE IF NOT EXISTS
    group_privilege_privilege (
                                  group_privilege_id uuid NOT NULL REFERENCES group_privilege(id) ON DELETE CASCADE,
                                  mode_allow bool NOT NULL,
                                  privilege_id uuid NOT NULL REFERENCES privilege(id) ON DELETE CASCADE,
                                  primary key (group_privilege_id, privilege_id)
);





-- SELECT * 
-- FROM privilege
--     join group_privilege_privilege gpp on privilege.id = gpp.privilege_id
--     join group_privilege gp on gpp.group_privilege_id = gp.id
-- WHERE gp.name = 'admin';


-- INSERT INTO bbl_user_group_privilege (user_id, group_privilege_id) 
-- SELECT 22578, id FROM group_privilege
--     WHERE name = 'admin';
-- 
-- SELECT p.*, gpp.*, gp.* 
-- FROM privilege p
--          join group_privilege_privilege gpp on p.id = gpp.privilege_id
--          join group_privilege gp on gpp.group_privilege_id = gp.id
-- WHERE gp.id IN (SELECT bbl_user_group_privilege.group_privilege_id FROM bbl_user_group_privilege WHERE user_id = 22578);




CREATE TABLE IF NOT EXISTS need_privilege (
    need_privilege_id uuid NOT NULL default gen_random_uuid(),
    name TEXT UNIQUE NOT NULL,
    PRIMARY KEY (need_privilege_id)
);

CREATE TABLE IF NOT EXISTS need_privilege_privilege (
    need_privilege_id uuid NOT NULL REFERENCES need_privilege(need_privilege_id) ON DELETE CASCADE,
    privilege_id uuid NOT NULL REFERENCES privilege(id) ON DELETE CASCADE,
    PRIMARY KEY (need_privilege_id, privilege_id)
);


CREATE TABLE IF NOT EXISTS setting (
    name TEXT not null,
    value TEXT not null,
    PRIMARY KEY (name)
);

CREATE TABLE IF NOT EXISTS router_setting (
    path TEXT NOT NULL,
    need_privilege uuid NOT NULL REFERENCES need_privilege(need_privilege_id) ON DELETE RESTRICT,
    need_cookie boolean NOT NULL,
    need_cookie_handler text,
    PRIMARY KEY (path)
);



CREATE OR REPLACE function user_check_need_privilege_by_name (userId BIGINT, need_privilege_name text)
    RETURNS RECORD
AS $$
DECLARE
    ret RECORD;
BEGIN

    SELECT
        DISTINCT ON (npp.privilege_id)
        us.pri_name,
        npp.privilege_id as need_privilege_id,
        (case when us.user_mode_allow = true THEN true ELSE false END) as user_has_privilege
    FROM need_privilege need
             join need_privilege_privilege npp on need.need_privilege_id = npp.need_privilege_id
             FULL JOIN
         (
             SELECT p.id as user_privilege_id, gpp.mode_allow as user_mode_allow, p.name as pri_name
             FROM privilege p
                      JOIN group_privilege_privilege gpp on p.id = gpp.privilege_id
                      JOIN group_privilege gp on gpp.group_privilege_id = gp.id
                      JOIN
                  (
                      SELECT need.*, npp.*
                      FROM need_privilege need join need_privilege_privilege npp on need.need_privilege_id = npp.need_privilege_id
                      WHERE need.name = need_privilege_name
                  ) need_pri on p.id = need_pri.privilege_id
             WHERE gp.id IN (SELECT bbl_user_group_privilege.group_privilege_id FROM bbl_user_group_privilege WHERE user_id = userId)
         ) us on us.user_privilege_id = npp.privilege_id
    WHERE need.name = need_privilege_name
    ORDER BY npp.privilege_id, user_has_privilege ASC INTO ret;

    RETURN ret;
END $$ LANGUAGE plpgsql;

CREATE OR REPLACE function user_check_need_privilege_by_id (userId BIGINT, need_privilege_key_id uuid)
    RETURNS RECORD
AS $$
DECLARE
    ret RECORD;
BEGIN

    SELECT
        DISTINCT ON (npp.privilege_id)
        us.pri_name,
        npp.privilege_id as need_privilege_id,
        (case when us.user_mode_allow = true THEN true ELSE false END) as user_has_privilege
    FROM need_privilege need
             join need_privilege_privilege npp on need.need_privilege_id = npp.need_privilege_id
             FULL JOIN
         (
             SELECT p.id as user_privilege_id, gpp.mode_allow as user_mode_allow, p.name as pri_name
             FROM privilege p
                      JOIN group_privilege_privilege gpp on p.id = gpp.privilege_id
                      JOIN group_privilege gp on gpp.group_privilege_id = gp.id
                      JOIN
                  (
                      SELECT need.*, npp.*
                      FROM need_privilege need join need_privilege_privilege npp on need.need_privilege_id = npp.need_privilege_id
                      WHERE need.need_privilege_id = need_privilege_key_id
                  ) need_pri on p.id = need_pri.privilege_id
             WHERE gp.id IN (SELECT bbl_user_group_privilege.group_privilege_id FROM bbl_user_group_privilege WHERE user_id = userId)
         ) us on us.user_privilege_id = npp.privilege_id
    WHERE need.need_privilege_id = need_privilege_key_id
    ORDER BY npp.privilege_id, user_has_privilege ASC INTO ret;

    RETURN ret;
END $$ LANGUAGE plpgsql;

CREATE OR REPLACE function router_settings_with_privilege (path TEXT, need_cookie bool, cookie_handler TEXT)
RETURNS TEXT
    AS $$
DECLARE
    id uuid;
    name TEXT;
BEGIN
    id := gen_random_uuid();
    name := concat('route:', path);
    
    INSERT INTO need_privilege (name, need_privilege_id) VALUES (name, id);
    
    INSERT INTO router_setting 
        (path, need_privilege, need_cookie, need_cookie_handler) 
    VALUES 
        (path, id, need_cookie, cookie_handler);

    RETURN name;
END
$$ LANGUAGE plpgsql;


SELECT ;
INSERT INTO privilege
(name, description)
VALUES
    ('admin_pannel_login', 'you can login to admin pannel'),
    ('admin_pannel_change_user_settings', 'you can change other settings from other user'),
    ('admin_pannel_user_delete', 'delete user'),
    ('admin_pannel_user_delete_plays', 'delte userplays'),
    ('admin_pannel_privilege_rwd', 'read, write, delete privilege'),
    ('admin_pannel_group_priviege_rwd', 'read, write, delete group_privilege'),
    ('admin_pannel_user_group_privilege_rwd', 'read, write, delete user_group_privilege'),
    ('widget_database_stats', 'can see stats from database')
ON CONFLICT DO NOTHING
;

INSERT INTO group_privilege
(name, description)
VALUES
    ('admin', 'can do all'),
    ('player', 'none'),
    ('group_widget_database_stats', 'group to can use widget_database_stats')
ON CONFLICT DO NOTHING
;

INSERT INTO group_privilege_privilege
(group_privilege_id, mode_allow, privilege_id)
SELECT gp.id, true, privilege.id FROM privilege
                                          join group_privilege gp on gp.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO group_privilege_privilege
(group_privilege_id, mode_allow, privilege_id)
SELECT gp.id, false, privilege.id FROM privilege
                                          join group_privilege gp on gp.name = 'player'
ON CONFLICT DO NOTHING;












-- widget_database_stats
INSERT INTO need_privilege (name) VALUES ('widget_database_stats');

INSERT INTO 
    need_privilege_privilege 
    (need_privilege_id, privilege_id) 
SELECT need.need_privilege_id, np.id 
FROM need_privilege need
JOIN privilege np on np.name = 'widget_database_stats';

SELECT router_settings_with_privilege('/api/user-info-by-cookie', false, null);
SELECT router_settings_with_privilege('/api/weblogin', false, null);
SELECT router_settings_with_privilege('/api/webloginwithusername', false, null);
SELECT router_settings_with_privilege('/api/webregister', false, null);
SELECT router_settings_with_privilege('/api/weblogintoken', false, null);
SELECT router_settings_with_privilege('/api/webupdateCookie', true, null);
SELECT router_settings_with_privilege('/api/webresetpasswdandsendemail', false, null);
SELECT router_settings_with_privilege('/api/token/newpasswdwithtoken', false, null);
SELECT router_settings_with_privilege('/api/signin/patreon', false, null);
SELECT router_settings_with_privilege('/api/signout/patreon', true, null);
SELECT router_settings_with_privilege('/api/weblogout', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/stats/{id:long}', false, null);
SELECT router_settings_with_privilege('/api/profile/stats/timeline/{id:long}', false, null);
SELECT router_settings_with_privilege('/api/profile/topplays/{id:long}', false, null);
SELECT router_settings_with_privilege('/api/profile/topplays/{id:long}/page/{page:int}', false, null);
SELECT router_settings_with_privilege('/api/profile/recentplays/{id:long}', false, null);
SELECT router_settings_with_privilege('/api/profile/update/email', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/update/passwd', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/update/username', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/update/avatar', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/update/patreonemail', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/accept/patreonemail/token/{token:guid}', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/drop-account/sendMail}', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/drop-account/token/{token:guid}', true, 'BASE');
SELECT router_settings_with_privilege('/api/profile/top-play-by-marks-length/user-id/{userId:long}', false, null);
SELECT router_settings_with_privilege('/api/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}', false, null);
SELECT router_settings_with_privilege('/api/update.php', false, null);
SELECT router_settings_with_privilege('/api2/apk/version/{dirNameNumber:long}.apk', false, null);
SELECT router_settings_with_privilege('/api2/avatar/hash', false, null);
SELECT router_settings_with_privilege('/api2/avatar/hash/{size:long}/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/avatar/{size:long}/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/jar/version/{version}.jar', false, null);
SELECT router_settings_with_privilege('/api2/leaderboard', false, null);
SELECT router_settings_with_privilege('/api2/leaderboard/user', false, null);
SELECT router_settings_with_privilege('/api2/leaderboard/search-user', false, null);
SELECT router_settings_with_privilege('/api2/token-create', false, null);
SELECT router_settings_with_privilege('/api2/token-refresh', true, 'BASE');
SELECT router_settings_with_privilege('/api2/token-remove', true, 'BASE');
SELECT router_settings_with_privilege('/api2/token-user-id', true, 'BASE');
SELECT router_settings_with_privilege('/api2/odr/{replayId}.odr', false, null);
SELECT router_settings_with_privilege('/api2/odr/{replayId:long}.zip', false, null);
SELECT router_settings_with_privilege('/api2/odr/fullname/{replayId:long}/{fullname}.zip', false, null);
SELECT router_settings_with_privilege('/api2/odr/redirect/{replayId:long}.zip', false, null);
SELECT router_settings_with_privilege('/api2/play/by-id', false, null);
SELECT router_settings_with_privilege('/api2/play/recent', false, null);
SELECT router_settings_with_privilege('/api2/rank/map-file', false, null);
SELECT router_settings_with_privilege('/api2/statistic/active-user', false, null);
SELECT router_settings_with_privilege('/api2/statistic/all-patreon', false, null);
SELECT router_settings_with_privilege('/api2/submit/play-start', true, 'BASE');
SELECT router_settings_with_privilege('/api2/submit/play-end', true, 'BASE');
SELECT router_settings_with_privilege('/api2/submit/replay-file', true, 'BASE');
SELECT router_settings_with_privilege('/api2/update/{lang}', false, null);
