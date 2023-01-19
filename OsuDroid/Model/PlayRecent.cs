using NPoco;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class PlayRecent {
    /// <summary>
    /// </summary>
    /// <param name="filterPlays"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="startAt"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<IReadOnlyList<BblScoreWithUsername>> FilterByAsync(
        string filterPlays,
        string orderBy,
        int limit,
        int startAt) {
        static string OrderBy(string orderBy) {
            return orderBy switch {
                "Time_ASC" => "date ASC",
                "Time_DESC" => "date DESC",
                "Score_ASC" => "score ASC",
                "Score_DESC" => "score DESC",
                "Combo_ASC" => "combo ASC",
                "Combo_DESC" => "combo DESC",
                "50_ASC" => "bad ASC",
                "50_DESC" => "bad DESC",
                "100_ASC" => "good ASC",
                "100_DESC" => "good DESC",
                "300_ASC" => "perfect ASC",
                "300_DESC" => "perfect DESC",
                "Miss_ASC" => "miss ASC",
                "Miss_DESC" => "miss DESC",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        static string WhereSql(string filterPlays) {
            return filterPlays switch {
                "Any" => string.Empty,
                "XSS_Plays" => "WHERE mark = 'XSS'",
                "SS_Plays" => "WHERE mark = 'SS'",
                "XS_Plays" => "WHERE mark = 'XS'",
                "S_Plays" => "WHERE mark = 'S'",
                "A_Plays" => "WHERE mark = 'A'",
                "B_Plays" => "WHERE mark = 'B'",
                "C_Plays" => "WHERE mark = 'C'",
                "D_Plays" => "WHERE mark = 'D'",
                "Accuracy_100" => "WHERE accuracy = 100000",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        var sql = new Sql($@"
SELECT bbl_score.id, uid, filename, hash, mode, score, combo, mark, geki, perfect, katu, good, bad, miss, date, accuracy, bus.username as username From bbl_score
JOIN bbl_user bus on bbl_score.uid = bus.id
{WhereSql(filterPlays)}
ORDER BY {OrderBy(orderBy)} 
LIMIT {limit}
OFFSET {startAt}
");

        using var db = DbBuilder.BuildPostSqlAndOpenNormalPoco();
        db.CommandTimeout = 3;
        return await db.FetchAsync<BblScoreWithUsername>(sql) ?? new List<BblScoreWithUsername>();
    }

    public sealed class BblScoreWithUsername : BblScore {
        [Column("username")] public string? Username { get; set; }
    }
}