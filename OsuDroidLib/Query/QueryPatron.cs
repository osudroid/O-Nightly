using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;

namespace OsuDroidLib.Query; 

public static class QueryPatron {
    public static async Task<ResultErr<string>> SyncPatronMembersByEmails(NpgsqlConnection db, IReadOnlyList<string> activeSupportEmails) {
        var sql0 = @"
UPDATE Patron
SET ActiveSupporter = CASE 
    WHEN PatronEmail 
             IN @PatronEmail 
        THEN true 
        ELSE false
        END 
";

        var sql1 = @"
INSERT INTO Patron (PatronEmail, ActiveSupporter) 
VALUES (@PatronEmail, @ActiveSupporter)
";
        
        return await ((ResultErr<string>)
            await db.SafeQueryAsync(sql0, 
                new { PatronEmail = activeSupportEmails.ToArray() })).AndAsync(async () => {
            return await db.SafeQueryAsync(sql1, activeSupportEmails.Select(x =>
                new Database.Entities.Patron { PatronEmail = x, ActiveSupporter = true }));
        });
    }

    public static async Task<ResultErr<string>> SyncPatronMember(NpgsqlConnection db, Patreon.NET.Member mem) {
        var sql = @$"
INSERT INTO Patron (PatronEmail, ActiveSupporter) 
VALUES (@PatronEmail, @ActiveSupporter)
ON CONFLICT DO UPDATE SET ActiveSupporter = @ActiveSupporter";
        
        return await db.SafeQueryAsync(sql, new {
            PatronEmail = mem.Attributes!.Email, 
            ActiveSupporter = mem.Attributes.PatreonStatus == "active_patron"
        });
    }
    
    public static async Task<Result<IEnumerable<(string Username, long UserId)>, string>> GetPatronUser(NpgsqlConnection db) {
        var sql = @"
SELECT bu.Username, bu.UserId
FROM Patron 
JOIN UserInfo bu on Patron.PatronEmail = bu.PatronEmail
WHERE ActiveSupporter = true 
  and bu.active = true 
";
        return (await db.SafeQueryAsync<UserInfo>(sql))
            .Map(se => se.Select(f => (f.Username??"", f.UserId)));
    }

    public static async Task<Result<Option<Patron>, string>> GetByPatronEmailAsync(NpgsqlConnection db, string patronEmail) {
        var sql = $@"
SELECT * 
FROM Patron
WHERE PatronEmail = @PatronEmail
";
        return await db.SafeQueryFirstOrDefaultAsync<Patron>(sql, new { PatronEmail = patronEmail });
    }
}