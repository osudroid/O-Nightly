namespace OsuDroid.Database.TableFn;

public static class BblPatron {
    public static Response DeletePatreonByPatreonEmail(SavePoco db, string patreonEmail) {
        var res = db.Execute(@$"Delete FROM bbl_patron WHERE patron_email = @0", patreonEmail);
        if (res == EResponse.Err)
            return Response.Err();
        return Response.Ok();
    }
}