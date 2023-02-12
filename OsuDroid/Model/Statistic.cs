namespace OsuDroid.Model;

public static class Statistic {
    private static (DateTime LastUpdate, (long Last1h, long Last1Day, long Register) Value) Buffer = (DateTime.MinValue,
        default);

    public static Result<(long Last1h, long Last1Day, long Register), string> ActiveUser() {
        if (Buffer.LastUpdate + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
            return Result<(long Last1h, long Last1Day, long Register), string>.Ok(Buffer.Value);

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var rep = SqlFunc.GetStatisticActiveUser(db);
        if (rep == EResult.Err)
            return Result<(long Last1h, long Last1Day, long Register), string>.Err(rep.Err());

        Buffer = (DateTime.UtcNow, (rep.Ok().ActiveUserLast1h, rep.Ok().ActiveUserLast1Day, rep.Ok().RegisterUser));

        return Result<(long Last1h, long Last1Day, long Register), string>.Ok(Buffer.Value);
    }

    public static Result<List<(string Username, long Id)>, string> GetActivePatreon() {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        return SqlFunc.GetPatronUser(db);
    }
}