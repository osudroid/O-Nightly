using System.Data;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Hosting;
using NLog.Extensions.Logging;
using Rimu.Repository.Environment.Adapter;

namespace Rimu.Repository.Postgres.Domain;

/// <summary>
/// Provides methods to build and manage Npgsql database connections.
/// </summary>
internal static class NpgsqlBuilder {
    private static readonly System.Threading.SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);
    private static NpgsqlDataSourceBuilder? _npgsqlDataSourceBuilder;
    private static NpgsqlDataSource? _npgsqlDataSource;

    /// <summary>
    /// Initializes the Npgsql data source and its configuration.
    /// </summary>
    private static void Init() {
        DefaultTypeMap.MatchNamesWithUnderscores = false;
        
        IServiceProvider serviceProvider = Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider;
        var envJson = serviceProvider.GetEnvJson();
        
        
        var connStringBuilder = new NpgsqlConnectionStringBuilder();
        connStringBuilder.Host = envJson.DB_IPV4;
        connStringBuilder.Port = (int)envJson.DB_PORT;
        connStringBuilder.Password = envJson.DB_PASSWD;
        connStringBuilder.Username = envJson.DB_USERNAME;
        connStringBuilder.Database = envJson.DATABASE;
        connStringBuilder.Pooling = true;
        connStringBuilder.ReadBufferSize = 1048576;
        connStringBuilder.WriteBufferSize = 1048576;
        connStringBuilder.MaxPoolSize = 512;
        connStringBuilder.MinPoolSize = 256;
        connStringBuilder.KeepAlive = 10;
        connStringBuilder.TcpKeepAlive = true;
        
        _npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connStringBuilder.ToString());
        // _npgsqlDataSourceBuilder.EnableParameterLogging(true);
        // _npgsqlDataSourceBuilder.UseLoggerFactory(new NLogLoggerFactory());

        _npgsqlDataSource = _npgsqlDataSourceBuilder.Build();
    }
    
    /// <summary>
    /// Asynchronously builds and retrieves a new Npgsql database connection.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An open <see cref="NpgsqlConnection"/> instance.</returns>
    public static async Task<NpgsqlConnection> BuildAsync(CancellationToken cancellationToken = default) {
        try {
            await SemaphoreSlim.WaitAsync(cancellationToken);
            if (_npgsqlDataSourceBuilder is null) {
                Init();
            }
        }
        finally {
            SemaphoreSlim.Release();
        }

        var conn = await _npgsqlDataSource!.OpenConnectionAsync(cancellationToken);
        if (conn.State != ConnectionState.Open) {
            await conn.OpenAsync(cancellationToken);    
        }
        
        return conn;
    }
}