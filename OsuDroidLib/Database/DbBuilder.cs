using Npgsql;
using NPoco;

namespace OsuDroidLib.Database;

public static class DbBuilder {
    public static string? NpgsqlConnectionString;

    private static DatabaseFactory? DatabaseFactory;

    private static void DatabaseFactoryBuild() {
        DatabaseFactory = DatabaseFactory.Config(config => {
            config.UsingDatabase(() => {
                var db = new NPoco.Database(new NpgsqlConnection(NpgsqlConnectionString), DatabaseType.PostgreSQL);
                db.EnableAutoSelect = false;
                db.KeepConnectionAlive = false;
                db.Connection!.Open();
                return db;
            });
        });
    }

    public static SavePoco BuildPostSqlAndOpen() {
        return new(BuildPostSqlAndOpenNormalPoco());
    }

    public static NPoco.Database BuildPostSqlAndOpenNormalPoco() {
        if (DatabaseFactory is null) DatabaseFactoryBuild();

        var database = DatabaseFactory!.GetDatabase();
        if (database.Connection is null)
            throw new NullReferenceException(nameof(database.Connection));
        return database;
    }

    public static NpgsqlConnection BuildNpgsqlConnection() {
        var con = new NpgsqlConnection(NpgsqlConnectionString);
        con.Open();
        return con;
    }
}