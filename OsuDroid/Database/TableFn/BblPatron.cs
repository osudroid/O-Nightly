namespace OsuDroid.Database.TableFn;

public static class BblPatron {
    public static ResultErr<string> DeletePatreonByPatreonEmail(SavePoco db, string patreonEmail) 
        => db.Execute(@$"Delete FROM bbl_patron WHERE patron_email = @0", patreonEmail);
}