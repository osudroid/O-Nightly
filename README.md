[![CodeFactor](https://www.codefactor.io/repository/github/osudroid/o-nightly/badge)](https://www.codefactor.io/repository/github/osudroid/o-nightly)

Rimu Backend
=============================
- [Goals](#goals)
- [Requirements](#requirements)
- [None Docker Deploy](#none-docker-deploy)
- [Docker Deploy](#docker-deploy)
- [CSharp dependencies](#csharp-dependencies)
- [Environment / Settings](#environment)
- [Bot](#securityuserjson)
- [Paths](#path)
----------------------------------

# Goals

### Finish Goals
#### (1.0.0)
- Automatic Migration from old to new Database
- Stable PHP API
- Stable New API
### Future Goals
#### (1.0.1)
- Add Deploy Help in Readme
#### (1.1.0)
- Manuel SQL Migration Up Down over Args

# None Docker Deploy



# Docker Deploy


#### ENV VAR
- AVATAR_PATH
  - INFO 
  - Der Ordner Avatar baucht ein default.jpg file als Default Profile Image
- REPLAY_PATH
  - INFO 
- SECURITY_USER_JSON
  - INFO 
- DOMAIN
  - INFO 
- appSign
  - INFO 
- signKey
  - INFO 
- OSUDROID_SECURITY_DLL
  - INFO 
- JWT_HASH
  - INFO 
- JWT_SECRET
  - INFO 
- LICENSE_KEY_GEO_IP
  - INFO 
- USER_ID_GEO_IP
  - INFO 
- DB_IPV4
  - INFO 
- DB_PORT
  - INFO 
- DB_USERNAME
  - INFO 
- DB_PASSWD
  - INFO 
- DATABASE
  - INFO 
- KEYWORD
  - INFO 
- PASSWD_SEED
  - INFO 
- EMAIL_NO_REPLAY
  - INFO 
- EMAIL_NO_REPLAY_PASSWD
  - INFO 
- EMAIL_NO_REPLAY_SMTP_ADDRESS
  - INFO 
- EMAIL_NO_REPLAY_USERNAME
  - INFO 
- PATREON_CLIENT_ID
  - INFO 
- PATREON_CLIENT_SECRET
  - INFO 
- PATREON_ACCESS_TOKEN
  - INFO 
- PATREON_REFRESH_TOKEN
  - INFO 
- PATREON_CAMPAIGN_ID
  - INFO 
- AVATAR_PATH
  - INFO
- REPLAY_PATH
  - INFO
- SECURITY_USER_JSON
  - INFO
- REPLAY_ZIP_PATH
  - INFO 
- UPDATE_PATH
  - INFO
- JAR_PATH
  - INFO

### SECURITY_USER_JSON
```json
{
  "users": [
    {
      "key": "KEY",
      "name": "NAME",
      "root": true,
      "sudo": ["sudo"]
    }
  ]
}
```
# Path
### Avatar Dir
```
|-<AVATAR_PATH>
  | default.jpg
  | ... 
```

### Apk and changelog Dir
```
|-<UPDATE_PATH>
  |-<versionid>
    | android.apk
    |-changelog
      | en
      | de
      | ...
```

### Jar Dir
```
|-<JAR_PATH>
  | <versionid>.jar
```

### Odr Dir
```
|-<REPLAY_PATH>
  | <replayid>.odr
```

### OdrZip Dir
```
|-<REPLAY_ZIP_PATH>
  | <replayid>.zip
```
