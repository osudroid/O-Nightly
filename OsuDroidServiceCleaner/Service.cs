using NPoco;

namespace OsuDroidServiceCleaner;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Result<ServiceState, string> RunClean(ServiceState state) {
        WriteLine("Start Clean");
        using var db = DbBuilder.BuildPostSqlAndOpen();
        try {
            return db.Execute(new Sql(@"
DELETE FROM bbl_score_pre_submit 
       WHERE date < @0 
", DateTime.UtcNow - TimeSpan.FromDays(1))).Map(x => state);
        }
        finally {
            WriteLine("Finish Clean");
        }
    }
}