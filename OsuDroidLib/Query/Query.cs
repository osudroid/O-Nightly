using Dapper;
using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class Query {
    public static async Task<Result<IEnumerable<PlayScoreWithUsername>,string>> PlayRecentFilterByAsync(
        NpgsqlConnection db,
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

        var sql = $@"
SELECT PlayScore.*, bus.username as Username 
From PlayScore
JOIN UserInfo bus on PlayScore.UserId = bus.UserId
{WhereSql(filterPlays)}
ORDER BY {OrderBy(orderBy)} 
LIMIT {limit}
OFFSET {startAt}
";

        return await db.SafeQueryAsync<PlayScoreWithUsername>(sql);
    }
}