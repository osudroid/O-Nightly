using NPoco;
using Patreon.NET;

namespace OsuDroidLib.Database.Entities;

[TableName("bbl_patron")]
[ExplicitColumns]
[PrimaryKey(new[] { "patron_email" }, AutoIncrement = false)]
public class BblPatron {
    [Column(Name = "patron_email")] public string? PatronEmail { get; set; }
    [Column(Name = "active_supporter")] public bool ActiveSupporter { get; set; }

    public static void SyncPatronMembersByEmails(SavePoco db, IReadOnlyList<string> activeSupportEmails) {
        object[] sqlArr = activeSupportEmails.ToArray();
        var sql = new Sql(@$"
UPDATE bbl_patron
SET active_supporter = CASE 
    WHEN patron_email 
             IN ({string.Join(", ", Enumerable.Range(0, activeSupportEmails.Count).Select(x => "@" + x + " "))}) 
        THEN true 
    ELSE false
    END 
", sqlArr);

        var ss = new Sql();

        var sql2 = new Sql(@$"
INSERT INTO bbl_patron (patron_email, active_supporter) 
VALUES {string.Join(", ", Enumerable.Range(0, activeSupportEmails.Count).Select(x => "(" + "@" + x + ", true" + ")"))}
ON CONFLICT DO NOTHING", sqlArr);

        if (Response.ThrowByDebug(db.Execute(sql)) == EResponse.Err)
            return;

        if (Response.ThrowByDebug(db.Execute(sql2)) == EResponse.Err)
            return;
    }

    public static void SyncPatronMember(SavePoco db, Member member) {
        var sql = new Sql(@$"
INSERT INTO bbl_patron (patron_email, active_supporter) 
VALUES (@0, {member.Attributes.PatreonStatus == "active_patron"})
ON CONFLICT DO UPDATE SET active_supporter = {member.Attributes.PatreonStatus == "active_patron"}",
            member.Attributes.Email);

        if (Response.ThrowByDebug(db.Execute(sql)) == EResponse.Err)
            return;
    }
}