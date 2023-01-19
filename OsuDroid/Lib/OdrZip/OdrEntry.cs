using OsuDroidLib.Database.Entities;

namespace OsuDroid.Lib.OdrZip;

public class OdrEntry {
    public int Version { get; set; }
    public OdrReplay? Replay { get; set; }

    public static OdrEntry Factory(BblScore bblScore, string username) {
        return new OdrEntry {
            Version = 1,
            Replay = new OdrReplay {
                Filename = bblScore.Filename,
                Playername = username,
                Replayfile = $"{bblScore.Id}.odr",
                Mod = bblScore.Mode,
                Score = bblScore.Score,
                Combo = bblScore.Combo,
                Mark = bblScore.Mark,
                H300k = bblScore.Geki,
                H300 = bblScore.Perfect,
                H100k = bblScore.Katu,
                H100 = bblScore.Good,
                H50 = bblScore.Bad,
                Misses = bblScore.Miss,
                Accuracy = bblScore.Accuracy,
                Time = bblScore.Date.Ticks,
                Perfect = 0
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