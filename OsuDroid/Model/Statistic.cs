namespace OsuDroid.Model;

public static class Statistic {
    private static (DateTime LastUpdate, (long Last1h, long Last1Day, long Register) Value) Buffer = (DateTime.MinValue,
        default);

    public static Response<(long Last1h, long Last1Day, long Register)> ActiveUser() {
        if (Buffer.LastUpdate + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
            return Response<(long Last1h, long Last1Day, long Register)>.Ok(Buffer.Value);

        using var db = DbBuilder.BuildPostSqlAndOpen();
        var rep = SqlFunc.GetStatisticActiveUser(db);
        if (rep == EResponse.Err)
            return Response<(long Last1h, long Last1Day, long Register)>.Err;

        Buffer = (DateTime.UtcNow, (rep.Ok().ActiveUserLast1h, rep.Ok().ActiveUserLast1Day, rep.Ok().RegisterUser));

        return Response<(long Last1h, long Last1Day, long Register)>.Ok(Buffer.Value);
    }

    public static Response<List<(string Username, long Id)>> GetActivePatreon() {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        return SqlFunc.GetPatronUser(db);
    }
}