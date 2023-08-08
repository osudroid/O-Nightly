using OsuDroidLib.Extension;

namespace OsuDroidServiceCleaner;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Result<ServiceState, string> RunClean(ServiceState state) {
        WriteLine("Start Clean");
        using var dbTask = DbBuilder.BuildNpgsqlConnection();
        dbTask.Wait();
        var db = dbTask.Result;

        try {
            var sql = @"DELETE FROM PlayScorePreSubmit WHERE date < @Time";
            var task = db.SafeQueryAsync(sql, new { Time = DateTime.UtcNow - TimeSpan.FromDays(1) });
            task.Wait();
            return task.Result.Map(x => state);
        }
        finally {
            WriteLine("Finish Clean");
        }
    }
}