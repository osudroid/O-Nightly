using NPoco;

namespace OsuDroidLib.Database.Entities;
#nullable enable
[TableName("bbl_score_pre_submit")]
[ExplicitColumns]
[PrimaryKey(new[] { "id" }, AutoIncrement = true)]
public class BblScorePreSubmit {
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
    [Column("mark")] public string? Mark { get; set; }
    [Column("geki")] public long Geki { get; set; }
    [Column("katu")] public long Katu { get; set; }
    [Column("good")] public long Good { get; set; }
    [Column("bad")] public long Bad { get; set; }
    [Column("miss")] public long Miss { get; set; }
    [Column("date")] public DateTime Date { get; set; }
    [Column("accuracy")] public long Accuracy { get; set; }

    /// <summary> INSERT NEW ROW RETURN WITH Id </summary>
    /// <param name="db"></param>
    /// <param name="userId"></param>
    /// <param name="filename"></param>
    /// <param name="fileHash"></param>
    /// <returns></returns>
    public static BblScorePreSubmit? PreAddScore(SavePoco db, long userId, string filename, string fileHash) {
        var insert = new BblScorePreSubmit {
            Uid = userId,
            Filename = filename,
            Hash = fileHash,
            Date = DateTime.UtcNow,
            Mode = string.Empty,
            Mark = string.Empty
        };
        var response = db.Insert(insert);
        if (response == EResponse.Err)
            return null;

        insert.Id = (long)response.Ok();
        return insert;
    }

    public static BblScorePreSubmit? GetById(SavePoco db, long id) {
        return db.SingleOrDefaultById<BblScorePreSubmit>(id) switch {
            { Status: EResponse.Ok } res => res.Ok(),
            _ => null
        };
    }
}