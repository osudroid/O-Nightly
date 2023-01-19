// using Npgsql;
// using NPoco;
//
// namespace OsuDroid.Database;
//
// public static class DbBuilder {
//     private static readonly string? NpgsqlConnectionString = CreateNpgsqlConnectionString();
//     
//     private static DatabaseFactory? DatabaseFactory = null;
//
//     private static void DatabaseFactoryBuild() {
//         DatabaseFactory = DatabaseFactory.Config(config => {
//             config.UsingDatabase(() => {
//                 var db = new NPoco.Database(new NpgsqlConnection(NpgsqlConnectionString), DatabaseType.PostgreSQL);
//                 db.EnableAutoSelect = false;
//                 db.KeepConnectionAlive = false;
//                 db.Connection!.Open();
//                 return db;
//             });
//         });
//     }  
//     
//
//
//     
//     public static LamSavePoco.SavePoco BuildPostSqlAndOpen() => new LamSavePoco.SavePoco(BuildPostSqlAndOpenNormalPoco());
//
//     public static NPoco.Database BuildPostSqlAndOpenNormalPoco() {
//         if (DatabaseFactory is null) {
//             DatabaseFactoryBuild();
//         }
//
//         NPoco.Database? database = DatabaseFactory!.GetDatabase();
//         if (database.Connection is null)
//             throw new NullReferenceException(nameof(database.Connection));
//         return database;
//     }
//     
//     public static NpgsqlConnection BuildNpgsqlConnection() {
//         var con = new NpgsqlConnection(NpgsqlConnectionString);
//         con.Open();
//         return con;
//     }
// }

