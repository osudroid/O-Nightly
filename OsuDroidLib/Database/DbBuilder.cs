using LamLogger;
using Npgsql;

namespace OsuDroidLib.Database;

public static class DbBuilder {
    public static string? NpgsqlConnectionString;
    
    public static async Task<NpgsqlConnection> BuildNpgsqlConnection() {
        var con = new NpgsqlConnection(NpgsqlConnectionString);
        await con.OpenAsync();
        return con;
    }
}