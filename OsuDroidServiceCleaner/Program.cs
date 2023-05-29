// See https://aka.ms/new-console-template for more information

using Npgsql;
using OsuDroidLib;
using OsuDroidServiceCleaner;

namespace OsuDroidServiceCleaner;

public static class Program {
    public static async Task Main(string[] args) {
        WriteLine("Create Cleaner Service");
        DbBuilder.NpgsqlConnectionString = CreateNpgsqlConnectionString();
        await ServiceManager<ServiceState>
            .DefaultStetting()
            .SetStateBuilder(Service.StateBuilder)
            .AddFunction(Service.RunClean)
            .ExecuteFunctionAfter(TimeSpan.FromDays(1))
            .SetFirstLoop(false)
            .Run();
    }

    private static string CreateNpgsqlConnectionString() {
        var ip = Env.CrDbIpv4;
        var port = Convert.ToInt32(Env.CrDbPortStr);

        var connStringBuilder = new NpgsqlConnectionStringBuilder {
            Host = ip,
            Port = port,
            Password = Env.CrDbPasswd,
            Username = Env.CrDbUsername,
            Database = Env.CrDbDatabase,
            Pooling = true,
            ReadBufferSize = 1048576,
            WriteBufferSize = 1048576,
            MaxPoolSize = 256,
            MinPoolSize = 20,
            KeepAlive = 10,
            TcpKeepAlive = true,
            Timeout = 1024,
            CommandTimeout = 1024
        };

        return connStringBuilder.ConnectionString;
    }
}