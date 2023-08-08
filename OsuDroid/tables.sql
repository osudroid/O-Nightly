CREATE TABLE IF NOT EXISTS Patron
(
    PatronEmail     text    NOT NULL,
    ActiveSupporter boolean NOT NULL DEFAULT false,
    PRIMARY KEY (PatronEmail)
);

CREATE SEQUENCE IF NOT EXISTS bbl_user_id_seq AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS UserInfo
(
    UserId             bigint    NOT NULL DEFAULT nextval('bbl_user_id_seq'),
    Username           text      NOT NULL UNIQUE,
    Password           text      NOT NULL DEFAULT '',
    Email              text      NOT NULL UNIQUE,
    DeviceId           text      NOT NULL DEFAULT '',
    RegisterTime       timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLoginTime      timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Region             text      NOT NULL DEFAULT '',
    Active             boolean   NOT NULL DEFAULT true,
    Banned             boolean   NOT NULL DEFAULT false,
    RestrictMode       boolean   NOT NULL DEFAULT false,
    UsernameLastChange timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LatestIp           text      NOT NULL DEFAULT '',
    PatronEmail        text               DEFAULT null,
    PatronEmailAccept  boolean   NOT NULL DEFAULT false,
    FOREIGN KEY (PatronEmail) REFERENCES Patron (PatronEmail),
    PRIMARY KEY (UserId)
);

ALTER SEQUENCE bbl_user_id_seq OWNED BY UserInfo.UserId;

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_patron_email ON UserInfo USING btree (PatronEmail);

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_username ON UserInfo USING btree (lower(username));



CREATE TABLE IF NOT EXISTS TokenUser
(
    TokenId    uuid      NOT NULL,
    UserId     bigint    NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    CreateDate timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (TokenId)
);

CREATE INDEX IF NOT EXISTS idx_TokenUser ON TokenUser USING btree (CreateDate);
CREATE INDEX IF NOT EXISTS idx_TokenUser_UserId ON TokenUser USING btree (TokenId) INCLUDE (UserId);



CREATE TABLE IF NOT EXISTS UserStats
(
    UserId           bigint NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    OverallPlaycount bigint NOT NULL DEFAULT 0,
    OverallScore     bigint NOT NULL DEFAULT 0,
    OverallAccuracy  bigint NOT NULL DEFAULT 0,
    OverallCombo     bigint NOT NULL DEFAULT 0,
    OverallXss       bigint NOT NULL DEFAULT 0,
    OverallXs        bigint NOT NULL DEFAULT 0,
    OverallSs        bigint NOT NULL DEFAULT 0,
    OverallS         bigint NOT NULL DEFAULT 0,
    OverallA         bigint NOT NULL DEFAULT 0,
    OverallB         bigint NOT NULL DEFAULT 0,
    OverallC         bigint NOT NULL DEFAULT 0,
    OverallD         bigint NOT NULL DEFAULT 0,
    OverallPerfect   bigint NOT NULL DEFAULT 0,
    OverallHits      bigint NOT NULL DEFAULT 0,
    Overall300       bigint NOT NULL DEFAULT 0,
    Overall100       bigint NOT NULL DEFAULT 0,
    Overall50        bigint NOT NULL DEFAULT 0,
    OverallGeki      bigint NOT NULL DEFAULT 0,
    OverallKatu      bigint NOT NULL DEFAULT 0,
    OverallMiss      bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (UserId)
);

CREATE INDEX IF NOT EXISTS idx_bbl_user_stats_score_many ON UserStats (OverallScore) INCLUDE (UserId);



CREATE SEQUENCE IF NOT EXISTS public.public_score_id_seq
    AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS PlayScore
(
    PlayScoreId bigint    NOT NULL DEFAULT nextval('public_score_id_seq'),
    UserId      bigint    NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    Filename    text      NOT NULL,
    Hash        text      NOT NULL,
    Mode        text[]    NOT NULL DEFAULT '{}',
    Score       bigint    NOT NULL DEFAULT 0,
    Combo       bigint    NOT NULL DEFAULT 0,
    Mark        text      NOT NULL DEFAULT '',
    Geki        bigint    NOT NULL DEFAULT 0,
    Perfect     bigint    NOT NULL DEFAULT 0,
    Katu        bigint    NOT NULL DEFAULT 0,
    Good        bigint    NOT NULL DEFAULT 0,
    Bad         bigint    NOT NULL DEFAULT 0,
    Miss        bigint    NOT NULL DEFAULT 0,
    Date        timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Accuracy    bigint    NOT NULL DEFAULT 0,
    PRIMARY KEY (PlayScoreId)
);



CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_uid ON PlayScore USING btree (UserId);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_uid_filename ON PlayScore USING btree (UserId, Filename);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_filename ON PlayScore USING btree (Filename);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_hash_filename ON PlayScore USING btree (Filename, Hash);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_user_id_id ON PlayScore USING btree (UserId desc, PlayScoreId desc) INCLUDE (
    Filename,
    Hash,
    Mode,
    Score,
    Combo,
    Mark,
    Geki,
    Perfect,
    Katu,
    Good,
    Bad,
    Miss,
    Date,
    Accuracy);

ALTER SEQUENCE public_score_id_seq OWNED BY PlayScore.PlayScoreId;



CREATE TABLE IF NOT EXISTS PlayScorePreSubmit
(
    ScorePreSubmitId bigint    NOT NULL DEFAULT nextval('public_score_id_seq'),
    UserID           bigint    NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    Filename         text      NOT NULL,
    Hash             text      NOT NULL,
    Mode             text[]    NOT NULL DEFAULT '{}',
    Score            bigint    NOT NULL DEFAULT 0,
    Combo            bigint    NOT NULL DEFAULT 0,
    Mark             text      NOT NULL DEFAULT '',
    Geki             bigint    NOT NULL DEFAULT 0,
    Perfect          bigint    NOT NULL DEFAULT 0,
    Katu             bigint    NOT NULL DEFAULT 0,
    Good             bigint    NOT NULL DEFAULT 0,
    Bad              bigint    NOT NULL DEFAULT 0,
    Miss             bigint    NOT NULL DEFAULT 0,
    Date             timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Accuracy         bigint    NOT NULL DEFAULT 0,
    PRIMARY KEY (ScorePreSubmitId)
);

ALTER SEQUENCE public_score_id_seq OWNED BY PlayScorePreSubmit.ScorePreSubmitId;
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON PlayScorePreSubmit (UserId) INCLUDE (ScorePreSubmitId);
CREATE INDEX IF NOT EXISTS idx_bbl_bbl_score_pre_submit_uid ON PlayScorePreSubmit (ScorePreSubmitId) INCLUDE (UserId);


CREATE TABLE IF NOT EXISTS
    ResetPasswordKey(
                        Token      TEXT      NOT NULL,
                        UserId     BIGINT    NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
                        CreateTime TIMESTAMP NOT NULL,
                        PRIMARY KEY (UserId, token)
);



CREATE TABLE IF NOT EXISTS
    PatreonEmailToken (
                          UserId BIGINT        NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
                          Token UUID           NOT NULL PRIMARY KEY,
                          CreateTime TIMESTAMP NOT NULL ,
                          Email TEXT           NOT NULL UNIQUE
);
CREATE INDEX IF NOT EXISTS idx_patreon_email_token_token_create_time ON PatreonEmailToken(Token, CreateTime);



CREATE TABLE IF NOT EXISTS
    PatreonDeleteAccEmailToken (
                          UserId BIGINT        NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
                          Token UUID           NOT NULL PRIMARY KEY,
                          CreateTime TIMESTAMP NOT NULL ,
                          Email TEXT           NOT NULL UNIQUE
);
CREATE INDEX IF NOT EXISTS idx_patreon_delete_acc_email_token_token_create_time ON PatreonDeleteAccEmailToken(Token, CreateTime);



CREATE TABLE IF NOT EXISTS
    PlayScoreBanned
(
    ScoreBannedId bigint PRIMARY KEY NOT NULL,
    UserId        bigint             NOT NULL,
    Filename      text               NOT NULL,
    Hash          text               NOT NULL,
    Mode          text[]             NOT NULL DEFAULT '{}',
    Score         bigint             NOT NULL DEFAULT 0,
    Combo         bigint             NOT NULL DEFAULT 0,
    Mark          text               NOT NULL DEFAULT '',
    Geki          bigint             NOT NULL DEFAULT 0,
    Perfect       bigint             NOT NULL DEFAULT 0,
    Katu          bigint             NOT NULL DEFAULT 0,
    Good          bigint             NOT NULL DEFAULT 0,
    Bad           bigint             NOT NULL DEFAULT 0,
    Miss          bigint             NOT NULL DEFAULT 0,
    Date          timestamp          NOT NULL,
    accuracy      bigint             NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS GlobalRankingTimeline
(
    UserId        bigint NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    Date          date   NOT NULL DEFAULT current_date,
    GlobalRanking bigint NOT NULL,
    Score         bigint NOT NULL,
    PRIMARY KEY (UserId, Date)
) PARTITION BY RANGE (date);



CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2010 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2010-01-01') TO ('2011-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2011 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2011-01-01') TO ('2012-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2012 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2012-01-01') TO ('2013-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2013 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2013-01-01') TO ('2014-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2014 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2014-01-01') TO ('2015-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2015 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2015-01-01') TO ('2016-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2016 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2016-01-01') TO ('2017-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2017 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2017-01-01') TO ('2018-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2018 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2018-01-01') TO ('2019-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2019 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2019-01-01') TO ('2020-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2020 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2020-01-01') TO ('2021-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2021 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2021-01-01') TO ('2022-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2022 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2022-01-01') TO ('2023-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2023 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2024 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2025 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2026 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2027 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2027-01-01') TO ('2028-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2028 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2028-01-01') TO ('2029-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2029 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2029-01-01') TO ('2030-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2030 PARTITION OF GlobalRankingTimeline FOR VALUES FROM ('2030-01-01') TO ('2031-01-01');

CREATE TABLE IF NOT EXISTS UserAvatar
(
    UserId    BIGINT REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    Hash      TEXT  Not NULL,
    TypeExt   TEXT  Not NULL,
    PixelSize int   Not NULL,
    Animation bool  Not NULL,
    Bytes     bytea Not NULL,
    Original  bool  Not NULL,
    PRIMARY KEY (UserId, Hash)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_user_avatar_userid_pixelsize on UserAvatar (UserId, PixelSize);
CREATE UNIQUE INDEX IF NOT EXISTS idx_user_avatar_hash on UserAvatar (hash);


CREATE TABLE IF NOT EXISTS WebLoginMathResult (
    WebLoginMathResultId uuid,
    CreateTime TIMESTAMP,
    MathResult INT,
    PRIMARY KEY (WebLoginMathResultId)
);

CREATE INDEX IF NOT EXISTS idx_web_login_math_result on WebLoginToken (CreateTime);



-- Only IN Production (Need Space)
CREATE INDEX IF NOT EXISTS idx_bbl_global_ranking_timeline_any
    on GlobalRankingTimeline (UserId, Date) INCLUDE (score, GlobalRanking);


CREATE TABLE IF NOT EXISTS Log
(
    Id       uuid      NOT NULL,
    Number   int       NOT NULL,
    DateTime timestamp NOT NULL,
    Message  text      NOT NULL,
    Status   text      NOT NULL,
    Stack    text      NOT NULL,
    Trigger  text      NOT NULL,
    PRIMARY KEY (id, Number)
);



CREATE TABLE IF NOT EXISTS
    GroupPrivilege
(
    GroupPrivilegeId uuid        NOT NULL DEFAULT gen_random_uuid(),
    Name             text UNIQUE NOT NULL,
    Description      text        NOT NULL,
    PRIMARY KEY (GroupPrivilegeId)
);

CREATE TABLE IF NOT EXISTS
    UserGroupPrivilege
(
    UserId           BIGINT NOT NULL REFERENCES UserInfo (UserId) ON DELETE CASCADE,
    GroupPrivilegeId uuid   NOT NULL REFERENCES GroupPrivilege (GroupPrivilegeId) ON DELETE CASCADE,
    PRIMARY KEY (UserId, GroupPrivilegeId),
    FOREIGN KEY (UserId) REFERENCES UserInfo (UserId),
    FOREIGN KEY (GroupPrivilegeId) REFERENCES GroupPrivilege (GroupPrivilegeId)
);

CREATE TABLE IF NOT EXISTS
    Privilege
(
    PrivilegeId uuid DEFAULT gen_random_uuid() NOT NULL,
    Name        text UNIQUE                    NOT NULL,
    Description text                           NOT NULL,
    PRIMARY KEY (PrivilegeId)
);

CREATE TABLE IF NOT EXISTS
    GroupPrivilege_Privilege
(
    GroupPrivilegeId uuid NOT NULL REFERENCES GroupPrivilege (GroupPrivilegeId) ON DELETE CASCADE,
    ModeAllow        bool NOT NULL,
    PrivilegeId      uuid NOT NULL REFERENCES Privilege (PrivilegeId) ON DELETE CASCADE,
    primary key (GroupPrivilegeId, PrivilegeId)
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


CREATE TABLE IF NOT EXISTS NeedPrivilege
(
    NeedPrivilegeId uuid        NOT NULL default gen_random_uuid(),
    Name            TEXT UNIQUE NOT NULL,
    PRIMARY KEY (NeedPrivilegeId)
);

CREATE TABLE IF NOT EXISTS NeedPrivilege_Privilege
(
    NeedPrivilegeId uuid NOT NULL REFERENCES NeedPrivilege (NeedPrivilegeId) ON DELETE CASCADE,
    PrivilegeId     uuid NOT NULL REFERENCES Privilege (PrivilegeId) ON DELETE CASCADE,
    PRIMARY KEY (NeedPrivilegeId, PrivilegeId)
);


CREATE TABLE IF NOT EXISTS Setting
(
    MainKey TEXT not null,
    SubKey  TEXT not null,
    Value   TEXT not null,
    PRIMARY KEY (MainKey, SubKey)
);

CREATE TABLE IF NOT EXISTS RouterSetting
(
    Path              TEXT    NOT NULL,
    NeedPrivilege     uuid    NOT NULL REFERENCES NeedPrivilege (NeedPrivilegeId) ON DELETE RESTRICT,
    NeedCookie        boolean NOT NULL,
    NeedCookieHandler text,
    PRIMARY KEY (Path)
);



CREATE OR REPLACE function user_check_need_privilege_by_name(CheckUserId BIGINT, NeedPrivilegeName text)
    RETURNS RECORD
AS
$$
DECLARE
    ret RECORD;
BEGIN
    SELECT DISTINCT ON (npp.PrivilegeId) us.PriName,
                                         npp.PrivilegeId                                              as NeedPrivilegeId,
                                         (case when us.UserModeAllow = true THEN true ELSE false END) as UserHasPrivilege
    FROM NeedPrivilege need
             join NeedPrivilege_Privilege npp on need.NeedPrivilegeId = npp.NeedPrivilegeId
             FULL JOIN
         (SELECT p.PrivilegeId as user_privilege_id, gpp.ModeAllow as UserModeAllow, p.Name as PriName
          FROM Privilege p
                   JOIN GroupPrivilege_Privilege gpp on p.PrivilegeId = gpp.PrivilegeId
                   JOIN GroupPrivilege gp on gpp.GroupPrivilegeId = gp.GroupPrivilegeId
                   JOIN
               (SELECT need.*, npp.*
                FROM NeedPrivilege need
                         join NeedPrivilege_Privilege npp on need.NeedPrivilegeId = npp.NeedPrivilegeId
                WHERE need.Name = NeedPrivilegeName) need_pri on p.PrivilegeId = need_pri.PrivilegeId
          WHERE gp.GroupPrivilegeId IN (SELECT UserGroupPrivilege.GroupPrivilegeId
                                        FROM UserGroupPrivilege
                                        WHERE UserGroupPrivilege.UserId = CheckUserId)) us
         on us.user_privilege_id = npp.PrivilegeId
    WHERE need.name = NeedPrivilegeName
    ORDER BY npp.PrivilegeId, UserHasPrivilege ASC
    INTO ret;

    RETURN ret;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE function user_check_need_privilege_by_id(CheckUserId BIGINT, NeedPrivilegeKeyId uuid)
    RETURNS RECORD
AS
$$
DECLARE
    ret RECORD;
BEGIN

    SELECT DISTINCT ON (npp.PrivilegeId) us.PriName,
                                         npp.PrivilegeId                                              as NeedPrivilegeId,
                                         (case when us.UserModeAllow = true THEN true ELSE false END) as UserHasPrivilege
    FROM NeedPrivilege need
             join NeedPrivilege_Privilege npp on need.NeedPrivilegeId = npp.NeedPrivilegeId
             FULL JOIN
         (SELECT p.PrivilegeId as UserPrivilegeId, gpp.ModeAllow as UserModeAllow, p.Name as PriName
          FROM privilege p
                   JOIN GroupPrivilege_Privilege gpp on p.PrivilegeId = gpp.PrivilegeId
                   JOIN GroupPrivilege gp on gpp.GroupPrivilegeId = gp.GroupPrivilegeId
                   JOIN
               (SELECT need.*, npp.*
                FROM NeedPrivilege need
                         join NeedPrivilege_Privilege npp on need.NeedPrivilegeId = npp.NeedPrivilegeId
                WHERE need.NeedPrivilegeId = NeedPrivilegeKeyId) NeedPri on p.PrivilegeId = NeedPri.PrivilegeId
          WHERE gp.GroupPrivilegeId IN
                (SELECT UserGroupPrivilege.GroupPrivilegeId FROM UserGroupPrivilege WHERE UserId = CheckUserId)) us
         on us.UserPrivilegeId = npp.PrivilegeId
    WHERE need.NeedPrivilegeId = NeedPrivilegeKeyId
    ORDER BY npp.PrivilegeId, UserHasPrivilege ASC
    INTO ret;

    RETURN ret;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE function router_settings_with_privilege(Path TEXT, NeedCookie bool, CookieHandler TEXT)
    RETURNS TEXT
AS
$$
DECLARE
    Id   uuid;
    Name TEXT;
BEGIN
    Id := gen_random_uuid();
    Name := concat('route:', path);

    INSERT INTO NeedPrivilege (name, NeedPrivilegeId) VALUES (name, id);

    INSERT INTO RouterSetting
        (path, NeedPrivilege, NeedCookie, NeedCookieHandler)
    VALUES (path, Id, NeedCookie, CookieHandler);

    RETURN Name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE function beatmap_top(file TEXT, fileHash TEXT, limitCount INT)
    RETURNS RECORD
AS
$$
DECLARE
    ret RECORD;
BEGIN
    SELECT score.*, UserInfo.Username, md5(UserInfo.Email) as EmailHash
    FROM PlayScore,
         UserInfo
    WHERE PlayScore.Filename = file
      AND Hash = fileHash
      AND PlayScore.UserId = UserInfo.UserId
    ORDER BY PlayScore.Score DESC, PlayScore.Accuracy DESC, PlayScore.Date ASC
    LIMIT limitCount;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION setting_update(NewMainKey TEXT, NewSubKey TEXT, NewValue TEXT)
    RETURNS void
AS
$$
BEGIN
    INSERT INTO Setting (MainKey, SubKey, Value)
    VALUES (NewMainKey, NewSubKey, NewValue)
    ON CONFLICT (MainKey, SubKey) DO UPDATE
        SET MainKey = NewMainKey,
            SubKey  = NewSubKey,
            Value   = NewVALUE;
END;
$$ LANGUAGE plpgsql;



SELECT;
INSERT INTO privilege
    (name, description)
VALUES ('admin_pannel_login', 'you can login to admin pannel'),
       ('admin_pannel_change_user_settings', 'you can change other settings from other user'),
       ('admin_pannel_user_delete', 'delete user'),
       ('admin_pannel_user_delete_plays', 'delte userplays'),
       ('admin_pannel_privilege_rwd', 'read, write, delete privilege'),
       ('admin_pannel_group_priviege_rwd', 'read, write, delete group_privilege'),
       ('admin_pannel_user_group_privilege_rwd', 'read, write, delete user_group_privilege'),
       ('widget_database_stats', 'can see stats from database')
ON CONFLICT DO NOTHING
;

INSERT INTO GroupPrivilege
    (name, description)
VALUES ('admin', 'can do all'),
       ('player', 'none'),
       ('group_widget_database_stats', 'group to can use widget_database_stats')
ON CONFLICT DO NOTHING
;

INSERT INTO GroupPrivilege_Privilege
    (GroupPrivilegeId, ModeAllow, PrivilegeId)
SELECT gp.GroupPrivilegeId, true, privilege.PrivilegeId
FROM privilege
         join GroupPrivilege gp on gp.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO GroupPrivilege_Privilege
    (GroupPrivilegeId, ModeAllow, PrivilegeId)
SELECT gp.GroupPrivilegeId, false, privilege.PrivilegeId
FROM Privilege
         join GroupPrivilege gp on gp.name = 'player'
ON CONFLICT DO NOTHING;


-- widget_database_stats
INSERT INTO NeedPrivilege (name)
VALUES ('widget_database_stats');

INSERT INTO NeedPrivilege_Privilege
    (NeedPrivilegeId, PrivilegeId)
SELECT need.NeedPrivilegeId, np.PrivilegeId
FROM NeedPrivilege need
         JOIN privilege np on np.name = 'widget_database_stats';


SELECT router_settings_with_privilege('/api/user-info-by-cookie', false, null);
SELECT router_settings_with_privilege('/api/weblogin', false, null);
SELECT router_settings_with_privilege('/api/webloginwithusername', false, null);
SELECT router_settings_with_privilege('/api/webregister', false, null);
SELECT router_settings_with_privilege('/api/weblogintoken', false, null);
SELECT router_settings_with_privilege('/api/webupdateCookie', true, 'BASE');
SELECT router_settings_with_privilege('/api/webresetpasswdandsendemail', false, null);
SELECT router_settings_with_privilege('/api/token/newpasswdwithtoken', false, null);
SELECT router_settings_with_privilege('/api/signin/patreon', false, null);
SELECT router_settings_with_privilege('/api/signout/patreon', true, 'BASE');
SELECT router_settings_with_privilege('/api/weblogout', true, 'BASE');
SELECT router_settings_with_privilege('/api/update', false, null);
SELECT router_settings_with_privilege('/api2/profile/stats/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/profile/stats/timeline/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/profile/topplays/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/profile/topplays/{id:long}/page/{page:int}', false, null);
SELECT router_settings_with_privilege('/api2/profile/recentplays/{id:long}', false, null);
SELECT router_settings_with_privilege('/api2/profile/update/email', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/update/passwd', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/update/username', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/update/avatar', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/update/patreonemail', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/accept/patreonemail/token/{token:guid}', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/drop-account/sendMail}', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/drop-account/token/{token:guid}', true, 'BASE');
SELECT router_settings_with_privilege('/api2/profile/top-play-by-marks-length/user-id/{userId:long}', false, null);
SELECT router_settings_with_privilege('/api2/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}',false, null);
SELECT router_settings_with_privilege('/api2/apk/version/{dirNameNumber:long}.apk', false, null);
SELECT router_settings_with_privilege('/api2/avatar/hash', false, null);
SELECT router_settings_with_privilege('/api2/avatar/hash/{hash:alpha}', false, null);
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


SELECT setting_update('Domain', 'Name', '');
SELECT setting_update('APK', 'SignKey', '');
SELECT setting_update('GeoIp', 'LicenseKey', '');
SELECT setting_update('GeoIp', 'UserId', '');
SELECT setting_update('RequestHash', 'Keyword', '');
SELECT setting_update('Password', 'Seed', '');
SELECT setting_update('Password', 'MinLength', '');
SELECT setting_update('Password', 'BCryptSalt', '');
SELECT setting_update('Email', 'NoReplay', '');
SELECT setting_update('Email', 'NoReplayPassword', '');
SELECT setting_update('Email', 'NoReplaySmtpAddress', '');
SELECT setting_update('Email', 'NoReplayUsername', '');
SELECT setting_update('Patreon', 'ClientId', '');
SELECT setting_update('Patreon', 'ClientSecret', '');
SELECT setting_update('Patreon', 'AccessToken', '');
SELECT setting_update('Patreon', 'RefreshToken', '');
SELECT setting_update('Patreon', 'CampaignId', '');
SELECT setting_update('Log', 'DbName', '');
SELECT setting_update('Log', 'SaveInDb', '');
SELECT setting_update('Log', 'Ok', '');
SELECT setting_update('Log', 'Debug', '');
SELECT setting_update('Log', 'Error', '');
SELECT setting_update('Log', 'RequestJsonPrint', '');
SELECT setting_update('UserAvatar', 'SizeLow', '');
SELECT setting_update('UserAvatar', 'SizeHigh', '');