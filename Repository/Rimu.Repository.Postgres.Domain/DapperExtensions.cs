using System.Data;
using System.Data.Common;
using Dapper;
using Dapper.Contrib.Extensions;
using LamLibAllOver;

namespace Rimu.Repository.Postgres.Domain;

public static class DapperExtensions {
    public static async Task<SResult<int>> SafeInsertAsync<T>(
        this IDbConnection connection,
        T entityToInsert,
        IDbTransaction transaction = null!,
        int? commandTimeout = null,
        ISqlAdapter sqlAdapter = null!) where T : class {
        try {
            return SResult<int>.Ok(await connection.InsertAsync(entityToInsert, transaction, commandTimeout,
                    sqlAdapter
                )
            );
        }
        catch (Exception e) {
            return SResult<int>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<IEnumerable<dynamic>>> SafeQueryAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<dynamic>>.Ok(await cnn.QueryAsync(sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<dynamic>>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<IEnumerable<dynamic>>> SafeQueryAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<IEnumerable<dynamic>>.Ok(await cnn.QueryAsync(command));
        }
        catch (Exception e) {
            return SResult<IEnumerable<dynamic>>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<dynamic>> SafeQueryFirstAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<dynamic>.Ok(await cnn.QueryFirstAsync(command));
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<dynamic>> SafeQueryFirstOrDefaultAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<dynamic>.Ok(await cnn.QueryFirstOrDefaultAsync(command));
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<dynamic>> SafeQuerySingleAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<dynamic>.Ok(await cnn.QuerySingleAsync(command));
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<SResult<dynamic>> SafeQuerySingleOrDefaultAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<dynamic>.Ok(await cnn.QuerySingleOrDefaultAsync(command));
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of SResults to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    ///     A sequence of data of <typeparamref name="T" />; if a basic type (int, string, etc) is queried then the data from
    ///     the first column in assumed, otherwise an instance is
    ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<SResult<IEnumerable<T>>> SafeQueryAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<T>>.Ok(await cnn.QueryAsync<T>(
                    sql, param, transaction, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<T>>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of SResult to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<T>> SafeQueryFirstAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<T>.Ok(
                await cnn.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType)
            );
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of SResult to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<Option<T>>> SafeQueryFirstOrDefaultAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null)
        where T : class {
        try {
            return SResult<Option<T>>.Ok(Option<T>.NullSplit(
                    await cnn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType)
                )
            );
        }
        catch (Exception e) {
            return SResult<Option<T>>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of SResult to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<T>> SafeQuerySingleAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<T>.Ok(await cnn.QuerySingleAsync<T>(sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<T>> SafeQuerySingleOrDefaultAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<T>.Ok(
                await cnn.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType)?? throw new NullReferenceException()
            );
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<dynamic>> SafeQueryFirstAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<dynamic>.Ok(await cnn.QueryFirstAsync(sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<dynamic>> SafeQueryFirstOrDefaultAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<dynamic>.Ok(
                await cnn.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType)
            );
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<dynamic>> SafeQuerySingleAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<dynamic>.Ok(await cnn.QuerySingleAsync(sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<SResult<dynamic>> SafeQuerySingleOrDefaultAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<dynamic>.Ok(
                await cnn.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType)
            );
        }
        catch (Exception e) {
            return SResult<dynamic>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
    public static async Task<SResult<IEnumerable<object>>> SafeQueryAsync(
        this IDbConnection cnn,
        Type type,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return SResult<IEnumerable<object>>.Ok(await cnn.QueryAsync(
                    type, sql, param, transaction, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<object>>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
    public static async Task<SResult<object>> SafeQueryFirstAsync(
        this IDbConnection cnn,
        Type type,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return SResult<object>.Ok(await cnn.QueryFirstAsync(type, sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
    public static async Task<SResult<object>> SafeQueryFirstOrDefaultAsync(
        this IDbConnection cnn,
        Type type,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<object>.Ok(
                await cnn.QueryFirstOrDefaultAsync(type, sql, param, transaction, commandTimeout, commandType)?? throw new NullReferenceException()
            );
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
    public static async Task<SResult<object>> SafeQuerySingleAsync(
        this IDbConnection cnn,
        Type type,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<object>.Ok(await cnn.QuerySingleAsync(type, sql, param, transaction, commandTimeout,
                    commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
    public static async Task<SResult<object>> SafeQuerySingleOrDefaultAsync(
        this IDbConnection cnn,
        Type type,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<object>.Ok(
                await cnn.QuerySingleOrDefaultAsync(type, sql, param, transaction, commandTimeout, commandType)?? throw new NullReferenceException()
            );
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <returns>
    ///     A sequence of data of <typeparamref name="T" />; if a basic type (int, string, etc) is queried then the data from
    ///     the first column in assumed, otherwise an instance is
    ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<SResult<IEnumerable<T>>> SafeQueryAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<IEnumerable<T>>.Ok(await cnn.QueryAsync<T>(command));
        }
        catch (Exception e) {
            return SResult<IEnumerable<T>>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<IEnumerable<object>>> SafeQueryAsync(
        this IDbConnection cnn,
        Type type,
        CommandDefinition command) {
        try {
            return SResult<IEnumerable<object>>.Ok(await cnn.QueryAsync(type, command));
        }
        catch (Exception e) {
            return SResult<IEnumerable<object>>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<object>> SafeQueryFirstAsync(
        this IDbConnection cnn,
        Type type,
        CommandDefinition command) {
        try {
            return SResult<object>.Ok(await cnn.QueryFirstAsync(type, command));
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<T>> SafeQueryFirstAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<T>.Ok(await cnn.QueryFirstAsync<T>(command));
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<object>> SafeQueryFirstOrDefaultAsync(
        this IDbConnection cnn,
        Type type,
        CommandDefinition command) {
        try {
            return SResult<object>.Ok(await cnn.QueryFirstOrDefaultAsync(type, command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<T>> SafeQueryFirstOrDefaultAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<T>.Ok(await cnn.QueryFirstOrDefaultAsync<T>(command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<object>> SafeQuerySingleAsync(
        this IDbConnection cnn,
        Type type,
        CommandDefinition command) {
        try {
            return SResult<object>.Ok(await cnn.QuerySingleAsync(type, command));
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<T>> SafeQuerySingleAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<T>.Ok(await cnn.QuerySingleAsync<T>(command));
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="type">The type to return.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<object>> SafeQuerySingleOrDefaultAsync(
        this IDbConnection cnn,
        Type type,
        CommandDefinition command) {
        try {
            return SResult<object>.Ok(await cnn.QuerySingleOrDefaultAsync(type, command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    public static async Task<SResult<T>> SafeQuerySingleOrDefaultAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<T>.Ok(await cnn.QuerySingleOrDefaultAsync<T>(command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a command asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<SResult<int>> SafeExecuteAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<int>.Ok(await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return SResult<int>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a command asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute on this connection.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<SResult<int>> SafeExecuteAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return SResult<int>.Ok(await cnn.ExecuteAsync(command));
        }
        catch (Exception e) {
            return SResult<int>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 2 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TReturn>(
        this IDbConnection cnn,
        string sql,
        Func<TFirst, TSecond, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 2 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TReturn>(
        this IDbConnection cnn,
        CommandDefinition command,
        Func<TFirst, TSecond, TReturn> map,
        string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(await cnn.QueryAsync(command, map, splitOn));
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 3 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TReturn>(
        this IDbConnection cnn,
        string sql,
        Func<TFirst, TSecond, TThird, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 3 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TReturn>(
        this IDbConnection cnn,
        CommandDefinition command,
        Func<TFirst, TSecond, TThird, TReturn> map,
        string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(await cnn.QueryAsync(
                    command, map, splitOn
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 4 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>>
        SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            this IDbConnection cnn,
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            object? param = null,
            IDbTransaction? transaction = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 4 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth,
        TReturn>(
        this IDbConnection cnn,
        CommandDefinition command,
        Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
        string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(command, map, splitOn)
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 5 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth,
        TFifth, TReturn>(
        this IDbConnection cnn,
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 5 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth,
        TFifth, TReturn>(
        this IDbConnection cnn,
        CommandDefinition command,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
        string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    command, map, splitOn
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 6 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth,
        TFifth, TSixth, TReturn>(
        this IDbConnection cnn,
        string sql,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn,
                    commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 6 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth,
        TFifth, TSixth, TReturn>(
        this IDbConnection cnn,
        CommandDefinition command,
        Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
        string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    command, map, splitOn
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 7 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
    /// <typeparam name="TSeventh">The seventh type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>>
        SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            this IDbConnection cnn,
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object? param = null,
            IDbTransaction? transaction = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn,
                    commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Perform an asynchronous multi-mapping query with 7 input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
    /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
    /// <typeparam name="TThird">The third type in the recordset.</typeparam>
    /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
    /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
    /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
    /// <typeparam name="TSeventh">The seventh type in the recordset.</typeparam>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>>
        SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            this IDbConnection cnn,
            CommandDefinition command,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            string splitOn = "Id") {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(
                await cnn.QueryAsync(
                    command, map, splitOn
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }


    /// <summary>
    ///     Perform an asynchronous multi-mapping query with an arbitrary number of input types.
    ///     This returns a single type, combined from the raw types via <paramref name="map" />.
    /// </summary>
    /// <typeparam name="TReturn">The combined type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="types">Array of types in the recordset.</param>
    /// <param name="map">The function to map row types to the return type.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="buffered">Whether to buffer the SResults in memory.</param>
    /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An enumerable of <typeparamref name="TReturn" />.</returns>
    public static async Task<SResult<IEnumerable<TReturn>>> SafeQueryAsync<TReturn>(
        this IDbConnection cnn,
        string sql,
        Type[] types,
        Func<object[], TReturn> map,
        object? param = null,
        IDbTransaction? transaction = null,
        bool buffered = true,
        string splitOn = "Id",
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IEnumerable<TReturn>>.Ok(await cnn.QueryAsync(
                    sql, types, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IEnumerable<TReturn>>.Err(e);
        }
    }

    /// <summary>
    ///     Execute a command that returns multiple SResult sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for this query.</param>
    /// <param name="param">The parameters to use for this query.</param>
    /// <param name="transaction">The transaction to use for this query.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    public static async Task<SResult<SqlMapper.GridReader>> SafeQueryMultipleAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<SqlMapper.GridReader>.Ok(await cnn.QueryMultipleAsync(
                    sql, param, transaction, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<SqlMapper.GridReader>.Err(e);
        }
    }


    /// <summary>
    ///     Execute a command that returns multiple SResult sets, and access each in turn.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command to execute for this query.</param>
    public static async Task<SResult<SqlMapper.GridReader>> SafeQueryMultipleAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<SqlMapper.GridReader>.Ok(await cnn.QueryMultipleAsync(command));
        }
        catch (Exception e) {
            return SResult<SqlMapper.GridReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="transaction">The transaction to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>An <see cref="IDataReader" /> that can be used to iterate over the SResults of the SQL query.</returns>
    /// <remarks>
    ///     This is typically used when the SResults of a query are not processed by Dapper, for example, used to fill a
    ///     <see cref="DataTable" />
    ///     or <see cref="T:DataSet" />.
    /// </remarks>
    /// <example>
    ///     <code>
    /// <![CDATA[
    /// DataTable table = new DataTable("MyTable");
    /// using (var reader = ExecuteReader(cnn, sql, param))
    /// {
    ///     table.Load(reader);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public static async Task<SResult<IDataReader>> SafeExecuteReaderAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<IDataReader>.Ok(
                await cnn.ExecuteReaderAsync(
                    sql, param, transaction, commandTimeout, commandType
                )
            );
        }
        catch (Exception e) {
            return SResult<IDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return a <see cref="DbDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="transaction">The transaction to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    public static async Task<SResult<DbDataReader>> SafeExecuteReaderAsync(
        this DbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            var reader = await cnn.ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType);
            return SResult<DbDataReader>.Ok(reader);
        }
        catch (Exception e) {
            return SResult<DbDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>An <see cref="IDataReader" /> that can be used to iterate over the SResults of the SQL query.</returns>
    /// <remarks>
    ///     This is typically used when the SResults of a query are not processed by Dapper, for example, used to fill a
    ///     <see cref="DataTable" />
    ///     or <see cref="T:DataSet" />.
    /// </remarks>
    public static async Task<SResult<IDataReader>> SafeExecuteReaderAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<IDataReader>.Ok(await cnn.ExecuteReaderAsync(command));
        }
        catch (Exception e) {
            return SResult<IDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return a <see cref="DbDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    public static async Task<SResult<DbDataReader>> SafeExecuteReaderAsync(
        this DbConnection cnn,
        CommandDefinition command) {
        try {
            var reader = await cnn.ExecuteReaderAsync(command);
            return SResult<DbDataReader>.Ok(reader);
        }
        catch (Exception e) {
            return SResult<DbDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="commandBehavior">The <see cref="CommandBehavior" /> flags for this reader.</param>
    /// <returns>An <see cref="IDataReader" /> that can be used to iterate over the SResults of the SQL query.</returns>
    /// <remarks>
    ///     This is typically used when the SResults of a query are not processed by Dapper, for example, used to fill a
    ///     <see cref="DataTable" />
    ///     or <see cref="T:DataSet" />.
    /// </remarks>
    public static async Task<SResult<IDataReader>> SafeExecuteReaderAsync(
        this IDbConnection cnn,
        CommandDefinition command,
        CommandBehavior commandBehavior) {
        try {
            return SResult<IDataReader>.Ok(await cnn.ExecuteReaderAsync(command, commandBehavior));
        }
        catch (Exception e) {
            return SResult<IDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL and return a <see cref="DbDataReader" />.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="commandBehavior">The <see cref="CommandBehavior" /> flags for this reader.</param>
    public static async Task<SResult<IDataReader>> SafeExecuteReaderAsync(
        this DbConnection cnn,
        CommandDefinition command,
        CommandBehavior commandBehavior) {
        try {
            return SResult<IDataReader>.Ok(await cnn.ExecuteReaderAsync(command, commandBehavior));
        }
        catch (Exception e) {
            return SResult<IDataReader>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="transaction">The transaction to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <see cref="object" />.</returns>
    public static async Task<SResult<object>> SafeExecuteScalarAsync(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<object>.Ok(await cnn.ExecuteScalarAsync(
                    sql, param, transaction, commandTimeout, commandType
                )?? throw new NullReferenceException()
            );
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }


    /// <summary>
    ///     Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="sql">The SQL to execute.</param>
    /// <param name="param">The parameters to use for this command.</param>
    /// <param name="transaction">The transaction to use for this command.</param>
    /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
    /// <param name="commandType">Is it a stored proc or a batch?</param>
    /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
    public static async Task<SResult<T>> SafeExecuteScalarAsync<T>(
        this IDbConnection cnn,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null) {
        try {
            return SResult<T>.Ok(await cnn.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout,
                    commandType
                )?? throw new NullReferenceException()
            );
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }


    /// <summary>
    ///     Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>The first cell selected as <see cref="object" />.</returns>
    public static async Task<SResult<object>> SafeExecuteScalarAsync(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<object>.Ok(await cnn.ExecuteScalarAsync(command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<object>.Err(e);
        }
    }

    /// <summary>
    ///     Execute parameterized SQL that selects a single value.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to execute on.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>The first cell selected as <typeparamref name="T" />.</returns>
    public static async Task<SResult<T>> SafeExecuteScalarAsync<T>(
        this IDbConnection cnn,
        CommandDefinition command) {
        try {
            return SResult<T>.Ok(await cnn.ExecuteScalarAsync<T>(command)?? throw new NullReferenceException());
        }
        catch (Exception e) {
            return SResult<T>.Err(e);
        }
    }
}