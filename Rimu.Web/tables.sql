CREATE TABLE IF NOT EXISTS "Log" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Date" DATE NOT NULL,
    "DateTime" timestamp NOT NULL,
    "Message" text NOT NULL,
    "Status" text NOT NULL,
    "Stack" text NOT NULL,
    "Trigger" text NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_log_date ON "Log" USING btree ("Date");
CREATE INDEX IF NOT EXISTS idx_log_datetime ON "Log" USING btree ("DateTime");


CREATE SEQUENCE IF NOT EXISTS bbl_user_id_seq AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS "UserInfo"
(
    "UserId"             bigint    NOT NULL DEFAULT nextval('bbl_user_id_seq'),
    "Username"           text      NOT NULL UNIQUE,
    "Password"           text      NOT NULL DEFAULT '',
    "PasswordGen2"       text      NOT NULL DEFAULT '',
    "Email"              text      NOT NULL UNIQUE,
    "DeviceId"           text      NOT NULL DEFAULT '',
    "RegisterTime"       timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginTime"      timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Region"             text      NOT NULL DEFAULT '',
    "Active"             boolean   NOT NULL DEFAULT true,
    "Banned"             boolean   NOT NULL DEFAULT false,
    "Archived"           boolean   NOT NULL DEFAULT false,
    "RestrictMode"       boolean   NOT NULL DEFAULT false,
    "UsernameLastChange" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LatestIp"           text      NOT NULL DEFAULT '',
    "PatronEmail"        text               DEFAULT null,
    "PatronEmailAccept"  boolean   NOT NULL DEFAULT false,
    PRIMARY KEY ("UserId")
);

ALTER SEQUENCE bbl_user_id_seq OWNED BY "UserInfo"."UserId";

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_patron_email ON "UserInfo" USING btree ("PatronEmail");

CREATE UNIQUE INDEX IF NOT EXISTS idx_bbl_user_username ON "UserInfo" USING btree (lower("Username"));
CREATE INDEX IF NOT EXISTS idx_bbl_user_info_id_and_banned ON "UserInfo" USING btree ("UserId") INCLUDE ("Banned");


CREATE TABLE IF NOT EXISTS "UserSetting" 
(
    "UserId" bigint NOT NULL PRIMARY KEY REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "ShowUserClassifications" bool NOT NULL DEFAULT True
);


CREATE TABLE IF NOT EXISTS "UserClassifications"
(
    "UserId" bigint NOT NULL PRIMARY KEY REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "CoreDeveloper" boolean NOT NULL DEFAULT false,
    "Developer" boolean NOT NULL DEFAULT false,
    "Contributor" boolean NOT NULL DEFAULT false,
    "Supporter" boolean NOT NULL DEFAULT false
);

CREATE 
    VIEW "View_UserInfo_UserClassifications" AS 
    SELECT
        "US"."UserId",
        "US"."CoreDeveloper",
        "US"."Developer",
        "US"."Contributor",
        "US"."Supporter",
        
        "UI"."Username",
        "UI"."Password",
        "UI"."PasswordGen2",
        "UI"."Email",
        "UI"."DeviceId",
        "UI"."RegisterTime",
        "UI"."LastLoginTime",
        "UI"."Region",
        "UI"."Active",
        "UI"."Banned",
        "UI"."RestrictMode",
        "UI"."UsernameLastChange",
        "UI"."LatestIp",
        "UI"."PatronEmail",
        "UI"."PatronEmailAccept"
    FROM "UserClassifications" "US"
    JOIN "UserInfo" "UI" on "UI"."UserId" = "US"."UserId";



CREATE TABLE IF NOT EXISTS "TokenUser"
(
    "TokenId"    text      NOT NULL,
    "UserId"     bigint    NOT NULL REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "CreateDate" timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY ("TokenId")
);

CREATE INDEX IF NOT EXISTS idx_TokenUser ON "TokenUser" USING btree ("CreateDate");
CREATE INDEX IF NOT EXISTS idx_TokenUser_UserId ON "TokenUser" USING btree ("TokenId") INCLUDE ("UserId");


CREATE SEQUENCE IF NOT EXISTS replay_file_id_seq AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS "ReplayFile"
(
    "Id" bigint NOT NULL PRIMARY KEY DEFAULT nextval('replay_file_id_seq'),
    "Odr" bytea NOT NULL
) WITH (toast_tuple_target = 128);

ALTER TABLE "ReplayFile" ALTER COLUMN "Odr" SET STORAGE EXTERNAL;
ALTER SEQUENCE replay_file_id_seq OWNED BY "ReplayFile"."Id";


CREATE TABLE IF NOT EXISTS "UserStats"
(
    "UserId"           bigint NOT NULL REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "OverallPlaycount" bigint NOT NULL DEFAULT 0,
    "OverallScore"     bigint NOT NULL DEFAULT 0,
    "OverallAccuracy"  double precision NOT NULL DEFAULT 0,
    "OverallPp"        double precision NOT NULL DEFAULT 0,
    "OverallCombo"     bigint NOT NULL DEFAULT 0,
    "OverallXss"       bigint NOT NULL DEFAULT 0,
    "OverallXs"        bigint NOT NULL DEFAULT 0,
    "OverallSs"        bigint NOT NULL DEFAULT 0,
    "OverallS"         bigint NOT NULL DEFAULT 0,
    "OverallA"         bigint NOT NULL DEFAULT 0,
    "OverallB"         bigint NOT NULL DEFAULT 0,
    "OverallC"         bigint NOT NULL DEFAULT 0,
    "OverallD"         bigint NOT NULL DEFAULT 0,
    "OverallPerfect"   bigint NOT NULL DEFAULT 0,
    "OverallHits"      bigint NOT NULL DEFAULT 0,
    "Overall300"       bigint NOT NULL DEFAULT 0,
    "Overall100"       bigint NOT NULL DEFAULT 0,
    "Overall50"        bigint NOT NULL DEFAULT 0,
    "OverallGeki"      bigint NOT NULL DEFAULT 0,
    "OverallKatu"      bigint NOT NULL DEFAULT 0,
    "OverallMiss"      bigint NOT NULL DEFAULT 0,
    PRIMARY KEY ("UserId")
);

CREATE INDEX IF NOT EXISTS idx_bbl_user_stats_score_many ON "UserStats" ("OverallScore") INCLUDE ("UserId");


CREATE VIEW "View_UserInfo_UserStats" AS 
SELECT US."UserId", 
       "Username", 
       "Password", 
       "Email", 
       "DeviceId", 
       "RegisterTime", 
       "LastLoginTime", 
       "Region", 
       "Active", 
       "Banned", 
       "RestrictMode", 
       "UsernameLastChange", 
       "LatestIp",
       "PatronEmail",
       "PatronEmailAccept",
       "OverallPlaycount",
       "OverallScore",
       "OverallAccuracy",
       "OverallCombo",
       "OverallXss",
       "OverallXs",
       "OverallSs",
       "OverallS",
       "OverallA",
       "OverallB",
       "OverallC",
       "OverallD",
       "OverallPerfect",
       "OverallHits",
       "Overall300",
       "Overall100",
       "Overall50",
       "OverallGeki",
       "OverallKatu",
       "OverallMiss"
FROM "UserInfo"   
JOIN "UserStats" US on "UserInfo"."UserId" = US."UserId";


CREATE SEQUENCE IF NOT EXISTS public.public_score_id_seq
    AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;

CREATE TABLE IF NOT EXISTS "Play" 
(
    "Id"        bigint NOT NULL PRIMARY KEY DEFAULT nextval('public.public_score_id_seq'),
    "UserId"    bigint NOT NULL REFERENCES "UserInfo" ("UserId"),
    "Filename"  TEXT   NOT NULL,
    "FileHash"  TEXT   NOT NULL
);

alter table "Play"
    add constraint Play_Play__fk
        foreign key ("UserId") references "UserInfo" ("UserId");

alter table "Play"
    add constraint un_Play_UserId_Filename_FileHash
        unique ("UserId", "Filename", "FileHash");

CREATE INDEX IF NOT EXISTS idx_Play_UserId ON "Play" USING btree ("UserId");
CREATE INDEX IF NOT EXISTS idx_Play_uid_filename ON "Play" USING btree ("UserId", "Filename");
CREATE INDEX IF NOT EXISTS idx_Play_filename ON "Play" USING btree ("Filename");
CREATE INDEX IF NOT EXISTS idx_Play_hash_filename ON "Play" USING btree ("Filename", "FileHash");
CREATE INDEX IF NOT EXISTS idx_Play_user_userid_playscoreid ON "Play" USING btree ("UserId" desc, "Id" desc);

CREATE TABLE IF NOT EXISTS "PlayStats"
(
    "Id"           bigint           NOT NULL PRIMARY KEY,
    "ReplayFileId" bigint                    DEFAULT null,
    "Mode"         text[]           NOT NULL DEFAULT '{}',
    "Score"        bigint           NOT NULL DEFAULT 0,
    "Combo"        bigint           NOT NULL DEFAULT 0,
    "Mark"         text             NOT NULL DEFAULT '',
    "Geki"         bigint           NOT NULL DEFAULT 0,
    "Perfect"      bigint           NOT NULL DEFAULT 0,
    "Katu"         bigint           NOT NULL DEFAULT 0,
    "Good"         bigint           NOT NULL DEFAULT 0,
    "Bad"          bigint           NOT NULL DEFAULT 0,
    "Miss"         bigint           NOT NULL DEFAULT 0,
    "Date"         timestamp        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Accuracy"     double precision NOT NULL DEFAULT 0,
    "Pp"           double precision NOT NULL DEFAULT -1,    
    CONSTRAINT fk_Play_PlayStats 
        FOREIGN KEY("Id")
            REFERENCES "Play"("Id"),
    CONSTRAINT "fk_PlayStats_ReplayFile"
        FOREIGN KEY("ReplayFileId")
            REFERENCES "ReplayFile"("Id")
);

CREATE VIEW "View_Play_PlayStats" AS 
    SELECT PS.*, p."UserId", p."FileHash", p."Filename"
    FROM "Play" p
    JOIN "PlayStats" PS on p."Id" = PS."Id";

CREATE SEQUENCE IF NOT EXISTS public.play_score_history_id
    AS bigint START WITH 1 INCREMENT BY 1 CACHE 1;


CREATE TABLE IF NOT EXISTS "PlayStatsHistory"
(
    "Id"           bigint           NOT NULL PRIMARY KEY,
    "ReplayFileId" bigint                    DEFAULT null,
    "PlayId"       bigint           NOT NULL,
    "Score"        double precision NOT NULL DEFAULT 0,
    "Pp"           double precision NOT NULL DEFAULT 0,
    "Date"         timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "fk_Play_PlayStats"
        FOREIGN KEY("PlayId")
            REFERENCES "Play"("Id"),
    CONSTRAINT "fk_PlayStatsHistory_ReplayFile"
        FOREIGN KEY("ReplayFileId")
            REFERENCES "ReplayFile"("Id")
);

CREATE VIEW "View_Play_PlayStatsHistory" AS
SELECT PSH.*, p."UserId", p."FileHash", p."Filename"
FROM "Play" p
         JOIN "PlayStatsHistory" PSH on p."Id" = PSH."PlayId";

CREATE VIEW "View_Play_PlayStats_UserInfo" AS
SELECT PSH.*, p."FileHash", p."Filename", UI.* 
FROM "Play" p
         JOIN "PlayStats" PSH on p."Id" = PSH."Id"
         JOIN "UserInfo" UI on p."UserId" = UI."UserId"
;

CREATE TABLE IF NOT EXISTS
    "ResetPasswordKey"
(
    "Token"      TEXT      NOT NULL,
    "UserId"     BIGINT    NOT NULL REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "CreateTime" TIMESTAMP NOT NULL,
    PRIMARY KEY ("UserId", "Token")
);

CREATE TABLE IF NOT EXISTS
    "TokenWithGroup"
(
    "Group" TEXT NOT NULL,
    "Token" TEXT NOT NULL,
    "CreateTime" TIMESTAMP NOT NULL,
    "Data" TEXT NOT NULL,
    
    PRIMARY KEY ("Group", "Token")
);

CREATE TABLE IF NOT EXISTS "GlobalRankingTimeline"
(
    "UserId"        bigint        NOT NULL REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "Date"          date          NOT NULL DEFAULT current_date,
    "GlobalRanking" bigint        NOT NULL,
    "Pp"         double precision NOT NULL,
    PRIMARY KEY ("UserId", "Date")
) PARTITION BY RANGE ("Date");


CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2021 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2021-01-01') TO ('2022-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2022 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2022-01-01') TO ('2023-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2023 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2023-01-01') TO ('2024-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2024 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2024-01-01') TO ('2025-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2025 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2026 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2027 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2027-01-01') TO ('2028-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2028 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2028-01-01') TO ('2029-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2029 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2029-01-01') TO ('2030-01-01');
CREATE TABLE IF NOT EXISTS bbl_global_ranking_timeline_y2030 PARTITION OF "GlobalRankingTimeline" 
    FOR VALUES FROM ('2030-01-01') TO ('2031-01-01');

CREATE TABLE IF NOT EXISTS "UserAvatar"
(
    "UserId"    BIGINT REFERENCES "UserInfo" ("UserId") ON DELETE CASCADE,
    "Hash"      TEXT  Not NULL,
    "TypeExt"   TEXT  Not NULL,
    "PixelSize" int   Not NULL,
    "Animation" bool  Not NULL,
    "Bytes"     bytea Not NULL,
    "Original"  bool  Not NULL,
    PRIMARY KEY ("UserId", "Hash")
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_user_avatar_userid_pixelsize on "UserAvatar" ("UserId", "PixelSize");
CREATE UNIQUE INDEX IF NOT EXISTS idx_user_avatar_hash on "UserAvatar" ("Hash");


CREATE VIEW "View_UserAvatarNoBytes" AS 
    (
        SELECT "UserId", "Hash", "TypeExt", "PixelSize", "Animation", "Original"
        FROM "UserAvatar"
    );



CREATE TABLE IF NOT EXISTS "WebLoginMathResult"
(
    "WebLoginMathResultId" uuid,
    "CreateTime"           TIMESTAMP,
    "MathResult"           INT,
    PRIMARY KEY ("WebLoginMathResultId")
);

CREATE INDEX IF NOT EXISTS idx_web_login_math_result on "WebLoginMathResult" ("CreateTime");


-- Only IN Production (Need Space)
CREATE INDEX IF NOT EXISTS idx_bbl_global_ranking_timeline_any
    on "GlobalRankingTimeline" ("UserId", "Date") INCLUDE ("Pp", "GlobalRanking");


CREATE TABLE IF NOT EXISTS "Setting"
(
    "MainKey" TEXT not null,
    "SubKey"  TEXT not null,
    "Value"   TEXT not null,
    PRIMARY KEY ("MainKey", "SubKey")
);

CREATE TABLE IF NOT EXISTS "SettingHot"
(
    "MainKey" TEXT not null,
    "SubKey"  TEXT not null,
    "Value"   TEXT not null,
    PRIMARY KEY ("MainKey", "SubKey")
);

CREATE TABLE IF NOT EXISTS
    "OldMarkNewMark"
(
    "Old" TEXT PRIMARY KEY,
    "New" TEXT UNIQUE
);

INSERT INTO "OldMarkNewMark"
    ("Old", "New")
VALUES ('XH', 'XSS'),
       ('X', 'SS'),
       ('SH', 'XS'),
       ('S', 'S'),
       ('A', 'A'),
       ('B', 'B'),
       ('C', 'C'),
       ('D', 'D')
;



CREATE OR REPLACE FUNCTION setting_update(NewMainKey TEXT, NewSubKey TEXT, NewValue TEXT)
    RETURNS void
AS
$$
BEGIN
    INSERT INTO "Setting" ("MainKey", "SubKey", "Value")
    VALUES (NewMainKey, NewSubKey, NewValue)
    ON CONFLICT ("MainKey", "SubKey") DO UPDATE
        SET "MainKey" = NewMainKey,
            "SubKey"  = NewSubKey,
            "Value"   = NewVALUE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION setting_hot_update(NewMainKey TEXT, NewSubKey TEXT, NewValue TEXT)
    RETURNS void
AS
$$
BEGIN
    INSERT INTO "SettingHot" ("MainKey", "SubKey", "Value")
    VALUES (NewMainKey, NewSubKey, NewValue)
    ON CONFLICT ("MainKey", "SubKey") DO UPDATE
        SET "MainKey" = NewMainKey,
            "SubKey"  = NewSubKey,
            "Value"   = NewVALUE;
END;
$$ LANGUAGE plpgsql;


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
SELECT setting_update('LoginToken', 'ValidTimeInMin', '');
SELECT setting_update('LoginToken', 'TokenSize', '');
SELECT setting_update('SecurityOld', 'Keyword', '');
SELECT setting_update('SecurityOld', 'AppSign', '');
SELECT setting_update('SecurityOld', 'AppSignDaily', '');
SELECT setting_update('SecurityOld', 'SignKey', '');
SELECT setting_update('Pp', 'URL', '');
SELECT setting_update('TokenUser', 'TTL', '5184000');
SELECT setting_update('TokenDropAccount', 'TTL', '900');
SELECT setting_update('TokenResetPassword', 'TTL', '900');
SELECT setting_update('TokenSignup', 'TTL', '900');
SELECT setting_update('Security', 'HashValidation', 'true');

SELECT setting_hot_update('ChangeLogs', 'Path', '');
SELECT setting_hot_update('ChangeLogs', 'UpdateUrl', '');
SELECT setting_hot_update('ChangeLogs', 'Version', '');