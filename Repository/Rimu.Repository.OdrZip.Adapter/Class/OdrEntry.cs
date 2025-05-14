using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.OdrZip.Adapter.Class;

public class OdrEntry {
    public int Version { get; set; }
    public OdrReplay? Replay { get; set; }

    
    
    public static OdrEntry Factory(IPlay_PlayStatsReadonly play_PlayStats, string username) {
        return new OdrEntry {
            Version = 1,
            Replay = new OdrReplay {
                Filename = play_PlayStats.Filename,
                Playername = username,
                Replayfile = $"{play_PlayStats.Id}.odr",
                Mod = Mode.ModeArrayToModeAsSingleString(play_PlayStats.Mode).Or("|"),
                Score = play_PlayStats.Score,
                Combo = play_PlayStats.Combo,
                Mark = play_PlayStats.Mark,
                H300k = play_PlayStats.Geki,
                H300 = play_PlayStats.Perfect,
                H100k = play_PlayStats.Katu,
                H100 = play_PlayStats.Good,
                H50 = play_PlayStats.Bad,
                Misses = play_PlayStats.Miss,
                Accuracy = play_PlayStats.Accuracy,
                Time = play_PlayStats.Date.Ticks,
                Perfect = play_PlayStats.Perfect
            }
        };
    }
}

/*
 * {
  // always 1
  "version": 1,
  "replaydata": {
    // filename
    "filename": "416153 Remo Prototype[CV- Hanamori Yumiri] - Sendan Life\\/Remo Prototype[CV Hanamori Yumiri] - Sendan Life (Lami) [Nostalgia].osu",
    // username
    "playername": "Rian8337",
    // <id>.odr
    "replayfile": "3855327.odr",
    // mode
    "mod": "hrs",
    // score
    "score": 22158586,
    // combo
    "combo": 1224,
    // mark
    "mark": "A",
    // geki
    "h300k": 180,
    // perfect
    "h300": 875,
    // katu
    "h100k": 3,
    // good
    "h100": 12,
    // bad
    "h50": 0,
    // miss
    "misses": 4,
    // accuracy / 10000
    "accuracy": 0.9865319865319865,
    // date in milliseconds epoch time
    "time": 1608889707000,
    // since no data in db, just set to 0
    "perfect": 0
  }
}
*/