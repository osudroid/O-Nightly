using Npgsql;

namespace OsuDroidLib.Database;

public static class DbBuilder {
    public static string? NpgsqlConnectionString;

    public static async Task<NpgsqlConnection> BuildNpgsqlConnection() {
        NpgsqlConnectionString ??= CreateNpgsqlConnectionString();

        var con = new NpgsqlConnection(NpgsqlConnectionString);
        await con.OpenAsync();
        return con;
    }

    private static string CreateNpgsqlConnectionString() {
        var ip = Setting.CrDbIpv4;
        var port = Convert.ToInt32(Setting.CrDbPortStr);

        var connStringBuilder = new NpgsqlConnectionStringBuilder();
        connStringBuilder.Host = ip;
        connStringBuilder.Port = port;
        connStringBuilder.Password = Setting.CrDbPasswd;
        connStringBuilder.Username = Setting.CrDbUsername;
        connStringBuilder.Database = Setting.CrDbDatabase;
        connStringBuilder.Pooling = true;
        connStringBuilder.TcpKeepAlive = true;
        connStringBuilder.Multiplexing = true;
        connStringBuilder.SocketSendBufferSize = 4_048_576;
        connStringBuilder.ReadBufferSize = 1048576;
        connStringBuilder.WriteBufferSize = 1048576;
        connStringBuilder.MaxPoolSize = 1024;
        connStringBuilder.MinPoolSize = 32;

        return connStringBuilder.ConnectionString;
    }
}