using System.Data.SqlTypes;
using NPoco;

namespace OsuDroidLib.Database.Entities;
#nullable enable
[TableName("bbl_score")]
[ExplicitColumns]
[PrimaryKey(new[] { "id" }, AutoIncrement = true)]
#pragma warning disable CS0660, CS0661
public class BblScore {
    public enum EBblScore {
        Geki,
        Perfect,
        Katu,
        Good,
        Bad,
        Miss,
        Accuracy,
        Hits,
        N300,
        N100,
        N50
    }

    public enum EMark {
        XSS,
        SS,
        XS,
        S,
        A,
        B,
        C,
        D
    }

    private string? _mark;
    [Column("id")] public long Id { get; set; }
    [Column("uid")] public long Uid { get; set; }

    /// <summary> varchar(255) </summary>
    [Column("filename")]
    public string? Filename { get; set; }

    /// <summary> varchar(36) </summary>
    [Column("hash")]
    public string? Hash { get; set; }

    /// <summary> varchar(8) </summary>
    [Column("mode")]
    public string? Mode { get; set; }

    [Column("score")] public long Score { get; set; }
    [Column("combo")] public long Combo { get; set; }

    [Column("mark")]
    public string? Mark {
        get => _mark;
        set {
            var mark = OldMarkOrMarkToMark(value ?? "");
            _mark = mark ?? throw new NullReferenceException(nameof(mark) + ": Mark Not Or Key Is False");
        }
    }

    [Column("geki")] public long Geki { get; set; }
    [Column("perfect")] public long Perfect { get; set; }
    [Column("katu")] public long Katu { get; set; }
    [Column("good")] public long Good { get; set; }
    [Column("bad")] public long Bad { get; set; }
    [Column("miss")] public long Miss { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("accuracy")] public long Accuracy { get; set; }

    public long GetValue(EBblScore eBblScore) {
        return eBblScore switch {
            EBblScore.Geki => Geki,
            EBblScore.Perfect => Perfect,
            EBblScore.Katu => Katu,
            EBblScore.Good => Good,
            EBblScore.Bad => Bad,
            EBblScore.Miss => Miss,
            EBblScore.Accuracy => Accuracy,
            EBblScore.Hits => Perfect + Perfect + Good + Bad + Geki + Katu,
            EBblScore.N300 => Perfect,
            EBblScore.N100 => Good,
            EBblScore.N50 => Bad,
            _ => throw new ArgumentOutOfRangeException(nameof(eBblScore), eBblScore, null)
        };
    }

    public static List<BblScoreWithUsername>? GetBeatmapTop(SavePoco db, string filename, string fileHash) {
        var sqlStr = $@"
SELECT bbl_score.*, bbl_user.username, md5(bbl_user.email) as email_hash
FROM bbl_score, bbl_user
WHERE filename = '{new SqlString(filename).ToString()}'
  AND hash = '{new SqlString(fileHash).ToString()}'
    AND bbl_score.uid = bbl_user.id
        ORDER BY bbl_score.score DESC, bbl_score.accuracy DESC, bbl_score.date ASC
    LIMIT 50;
";
        var response = db.Fetch<BblScoreWithUsername>(sqlStr);
        if (response == EResult.Err)
            return null;
        foreach (var bblScoreWithUsername in response.Ok()) {
            bblScoreWithUsername.Username ??= string.Empty;
            bblScoreWithUsername.EmailHash ??= string.Empty;
            bblScoreWithUsername.Filename ??= string.Empty;
            bblScoreWithUsername.Hash ??= string.Empty;
            bblScoreWithUsername.Mode ??= string.Empty;
        }

        return response.Ok();
        // var res = new List<BblScoreWithUsername>(32);
        // var command = db.Connection!.CreateCommand();
        // command.CommandText = sqlStr;
        // var reader = command.ExecuteReader();
        //
        // while (reader.Read()) {
        //     var item = new BblScoreWithUsername();
        //     item.Id = (long) reader["id"];
        //     item.Uid = (long) reader["uid"];
        //     item.Filename = (string) reader["filename"];
        //     item.Hash = (string) reader["hash"];
        //     item.Mode = reader["mode"] is System.DBNull? string.Empty: (string) reader["mode"];
        //     item.Score = (long) reader["score"];
        //     item.Combo = (long) reader["combo"];
        //     item.Mark = reader["mark"] is System.DBNull? string.Empty: (string) reader["mark"];
        //     item.Geki = (long) reader["geki"];
        //     item.Perfect = (long) reader["perfect"];
        //     item.Katu = (long) reader["katu"];
        //     item.Good = (long) reader["good"];
        //     item.Bad = (long) reader["bad"];
        //     item.Miss = (long) reader["miss"];
        //     item.Date = (DateTime) reader["date"];
        //     item.Accuracy = (long) reader["accuracy"];
        //     item.Username = (string) reader["username"];
        //     item.EmailHash = (string) reader["email_hash"];
        //     res.Add(item);
        // }
        //
        // return res;
    }

    public static Result<Option<BblScore>, string> GetUserTopScore(SavePoco db, long userID, string filename, string fileHash) {
        return db.FirstOrDefault<BblScore>(
            "SELECT * FROM bbl_score WHERE uid = @0 AND filename = @1 AND hash = @2",
            userID, filename, fileHash).Map(x => Option<BblScore>.NullSplit(x));
    }

    public static Result<Option<BblScore>, string> GetById(SavePoco db, long playID) {
        return db.FirstOrDefault<BblScore>($"SELECT * FROM bbl_score WHERE id = {playID} LIMIT 1")
            .Map(x => Option<BblScore>.NullSplit(x));
    }


    public static Result<long, string> GetUserMapRank(SavePoco db, long playID) {
        return db.First<long>(@$"
SELECT count(*) + 1 
FROM bbl_score 
    totalscore, (SELECT filename, hash, score FROM bbl_score WHERE id = {playID}) refer 
WHERE totalscore.filename = refer.filename 
  AND totalscore.hash = refer.hash 
  AND totalscore.score > refer.score");
    }

    public static BblScore operator -(BblScore newVal, BblScore oldVal) {
        return new BblScore {
            Id = newVal.Id,
            Uid = newVal.Uid,
            Filename = newVal.Filename,
            Hash = newVal.Hash,
            Mode = newVal.Mode,
            Score = newVal.Score - oldVal.Score,
            Combo = newVal.Combo - oldVal.Combo,
            Mark = newVal.Mark,
            Geki = newVal.Geki - oldVal.Geki,
            Perfect = newVal.Perfect - oldVal.Perfect,
            Katu = newVal.Katu - oldVal.Katu,
            Good = newVal.Good - oldVal.Good,
            Bad = newVal.Bad - oldVal.Bad,
            Miss = newVal.Miss - oldVal.Miss,
            Date = newVal.Date,
            Accuracy = newVal.Accuracy - oldVal.Accuracy
        };
    }

    public int EqAsInt(EMark mark) {
        return Eq(mark) ? 1 : 0;
    }

    public bool Eq(EMark mark) {
        Enum.TryParse(Mark, out EMark myStatus);
        return mark == myStatus;
    }

    public static bool operator ==(BblScore score, EMark mark) {
        return !score.Eq(mark);
    }

    public static bool operator !=(BblScore score, EMark mark) {
        return !score.Eq(mark);
    }


    public static string? OldMarkOrMarkToMark(string mark) {
        mark = mark.ToUpper();
        return mark switch {
            // OLD TO NEW
            "XH" => "XSS",
            "SH" => "XS",
            "X" => "SS",
            // NEW
            "XSS" => mark,
            "SS" => mark,
            "XS" => mark,
            "S" => mark,
            "A" => mark,
            "B" => mark,
            "C" => mark,
            "D" => mark,
            _ => null
        };
    }

    public class BblScoreWithUsername {
        [Column("id")] public long Id { get; set; }
        [Column("uid")] public long Uid { get; set; }
        [Column("username")] public string? Username { get; set; }
        [Column("email_hash")] public string? EmailHash { get; set; }
        [Column("filename")] public string? Filename { get; set; }
        [Column("hash")] public string? Hash { get; set; }
        [Column("mode")] public string? Mode { get; set; }
        [Column("score")] public long Score { get; set; }
        [Column("combo")] public long Combo { get; set; }
        [Column("mark")] public string? Mark { get; set; }
        [Column("geki")] public long Geki { get; set; }
        [Column("perfect")] public long Perfect { get; set; }
        [Column("katu")] public long Katu { get; set; }
        [Column("good")] public long Good { get; set; }
        [Column("bad")] public long Bad { get; set; }
        [Column("miss")] public long Miss { get; set; }
        [Column("date")] public DateTime Date { get; set; }
        [Column("accuracy")] public long Accuracy { get; set; }
    }
#pragma warning restore CS0660, CS0661
}