using System.Data;
using System.Data.Common;
using Dapper;
using Dapper.Contrib.Extensions;

#pragma warning disable CS8625

namespace OsuDroidLib.Extension; 

public static class DapperExtensions {
    
    public static async Task<Result<int, string>> SafeInsertAsync<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null,
        int? commandTimeout = null, ISqlAdapter sqlAdapter = null) where T : class {

        try {
            return Result<int, string>.Ok(await connection.InsertAsync(entityToInsert, transaction, commandTimeout, sqlAdapter));
        }
        catch (Exception e) {
            return Result<int, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<IEnumerable<dynamic>, string>> SafeQueryAsync(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

        try {
            return Result<IEnumerable<dynamic>, string>.Ok(await cnn.QueryAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<IEnumerable<dynamic>, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<IEnumerable<dynamic>, string>> SafeQueryAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return Result<IEnumerable<dynamic>, string>.Ok(await cnn.QueryAsync(command));
        }
        catch (Exception e) {
            return Result<IEnumerable<dynamic>, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<dynamic, string>> SafeQueryFirstAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return Result<dynamic, string>.Ok(await cnn.QueryFirstAsync(command));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<dynamic, string>> SafeQueryFirstOrDefaultAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return Result<dynamic, string>.Ok(await cnn.QueryFirstOrDefaultAsync(command));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<dynamic, string>> SafeQuerySingleAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return Result<dynamic, string>.Ok(await cnn.QuerySingleAsync(command));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="command">The command used to query on this connection.</param>
    /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
    public static async Task<Result<dynamic, string>> SafeQuerySingleOrDefaultAsync(this IDbConnection cnn, CommandDefinition command) {
        try {
            return Result<dynamic, string>.Ok(await cnn.QuerySingleOrDefaultAsync(command));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of results to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    /// <returns>
    /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
    /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
    /// </returns>
    public static async Task<Result<IEnumerable<T>, string>> SafeQueryAsync<T>(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

        try {
            return Result<IEnumerable<T>, string>.Ok(await cnn.QueryAsync<T>(
                sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<IEnumerable<T>, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<T, string>> SafeQueryFirstAsync<T>(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

        try {
            return Result<T, string>.Ok(
                await cnn.QueryFirstAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<T, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<Option<T>, string>> SafeQueryFirstOrDefaultAsync<T>(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) where T: class {
        
        try {
            return Result<T, string>.Ok(Option<T>.With(await cnn.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType)));
        }
        catch (Exception e) {
            return Result<Option<T>, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type of result to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<T, string>> SafeQuerySingleAsync<T>(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
        
        try {
            return Result<T, string>.Ok(await cnn.QuerySingleAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<T, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<T, string>> SafeQuerySingleOrDefaultAsync<T>(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
        
        try {
            return Result<T, string>.Ok(await cnn.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<T, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<dynamic, string>> SafeQueryFirstAsync(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
        
        try {
            return Result<dynamic, string>.Ok(await cnn.QueryFirstAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<dynamic, string>> SafeQueryFirstOrDefaultAsync(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
        
        try {
            return Result<dynamic, string>.Ok(await cnn.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<dynamic, string>> SafeQuerySingleAsync(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
        
        try {
            return Result<dynamic, string>.Ok(await cnn.QuerySingleAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }


    /// <summary>
    /// Execute a single-row query asynchronously using Task.
    /// </summary>
    /// <param name="cnn">The connection to query on.</param>
    /// <param name="sql">The SQL to execute for the query.</param>
    /// <param name="param">The parameters to pass, if any.</param>
    /// <param name="transaction">The transaction to use, if any.</param>
    /// <param name="commandTimeout">The command timeout (in seconds).</param>
    /// <param name="commandType">The type of command to execute.</param>
    public static async Task<Result<dynamic, string>> SafeQuerySingleOrDefaultAsync(this IDbConnection cnn, string sql, object param = null,
        IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

        try {
            return Result<dynamic, string>.Ok(await cnn.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }
        catch (Exception e) {
            return Result<dynamic, string>.Err(e.ToString());
        }
    }

        /// <summary>
        /// Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static async Task<Result<IEnumerable<object>, string>> SafeQueryAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return Result<IEnumerable<object>, string>.Ok(await cnn.QueryAsync(
                    type,sql, param, transaction, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<IEnumerable<object>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static async Task<Result<object, string>> SafeQueryFirstAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return Result<object, string>.Ok(await cnn.QueryFirstAsync(type, sql, param,transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }
        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static async Task<Result<object, string>> SafeQueryFirstOrDefaultAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try {
                return Result<object, string>.Ok(await cnn.QueryFirstOrDefaultAsync(type, sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }
        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static async Task<Result<object, string>> SafeQuerySingleAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try {
                return Result<object, string>.Ok(await cnn.QuerySingleAsync(type, sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }
        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static async Task<Result<object, string>> SafeQuerySingleOrDefaultAsync(this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            try {
                return Result<object, string>.Ok(await cnn.QuerySingleOrDefaultAsync(type, sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        /// <returns>
        /// A sequence of data of <typeparamref name="T"/>; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static async Task<Result<IEnumerable<T>, string>> SafeQueryAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<IEnumerable<T>, string>.Ok(await cnn.QueryAsync<T>(command));
            }
            catch (Exception e) {
                return Result<IEnumerable<T>, string>.Err(e.ToString());
            }
        }


        /// <summary>
        /// Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<IEnumerable<object>, string>> SafeQueryAsync(this IDbConnection cnn, Type type, CommandDefinition command) {
            try {
                return Result<IEnumerable<object>, string>.Ok(await cnn.QueryAsync(type, command));
            }
            catch (Exception e) {
                return Result<IEnumerable<object>, string>.Err(e.ToString());
            }
        }


        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<object, string>> SafeQueryFirstAsync(this IDbConnection cnn, Type type, CommandDefinition command) {
            try {
                return Result<object, string>.Ok(await cnn.QueryFirstAsync(type, command));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<T, string>> SafeQueryFirstAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<T, string>.Ok(await cnn.QueryFirstAsync<T>(command));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<object, string>> SafeQueryFirstOrDefaultAsync(this IDbConnection cnn, Type type,
            CommandDefinition command) {

            try {
                return Result<object, string>.Ok(await cnn.QueryFirstOrDefaultAsync(type, command));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }


        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<T, string>> SafeQueryFirstOrDefaultAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<T, string>.Ok(await cnn.QueryFirstOrDefaultAsync<T>(command));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<object, string>> SafeQuerySingleAsync(this IDbConnection cnn, Type type,
            CommandDefinition command) {

            try {
                return Result<object, string>.Ok(await cnn.QuerySingleAsync(type, command));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
            
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<T, string>> SafeQuerySingleAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<T, string>.Ok(await cnn.QuerySingleAsync<T>(command));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<object, string>> SafeQuerySingleOrDefaultAsync(this IDbConnection cnn, Type type, CommandDefinition command) {
            try {
                return Result<object, string>.Ok(await cnn.QuerySingleOrDefaultAsync(type, command));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        public static async Task<Result<T, string>> SafeQuerySingleOrDefaultAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<T, string>.Ok(await cnn.QuerySingleOrDefaultAsync<T>(command));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<Result<int, string>> SafeExecuteAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

            try {
                return Result<int, string>.Ok(await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<int, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute on this connection.</param>
        /// <returns>The number of rows affected.</returns>
        public static async Task<Result<int, string>> SafeExecuteAsync(this IDbConnection cnn, CommandDefinition command)
        {
            try {
                return Result<int, string>.Ok(await cnn.ExecuteAsync(command));
            }
            catch (Exception e) {
                return Result<int, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 2 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null,
            bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {

            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 2 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TReturn>(this IDbConnection cnn,
            CommandDefinition command, Func<TFirst, TSecond, TReturn> map, string splitOn = "Id") {

            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(command, map, splitOn));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 3 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn,
            string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null,
            CommandType? commandType = null) {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 3 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TReturn>(this IDbConnection cnn,
            CommandDefinition command, Func<TFirst, TSecond, TThird, TReturn> map, string splitOn = "Id") {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    command, map, splitOn
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 4 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>>
            SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this IDbConnection cnn, string sql,
                Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
                IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
                int? commandTimeout = null, CommandType? commandType = null) {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 4 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            this IDbConnection cnn, CommandDefinition command, Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            string splitOn = "Id") {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(command, map, splitOn));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 5 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null) {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction,  buffered, splitOn, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 5 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            this IDbConnection cnn, CommandDefinition command,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, string splitOn = "Id") {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    command, map, splitOn
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 6 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
            int? commandTimeout = null, CommandType? commandType = null) {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn,
                    commandTimeout, commandType
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 6 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            this IDbConnection cnn, CommandDefinition command,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, string splitOn = "Id") {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    command, map, splitOn
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 7 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>>
            SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn,
                string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
                object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id",
                int? commandTimeout = null, CommandType? commandType = null) {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, map, param, transaction, buffered, splitOn,
                    commandTimeout, commandType
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Perform an asynchronous multi-mapping query with 7 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
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
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>>
            SafeQueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn,
                CommandDefinition command,
                Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, string splitOn = "Id") {
            
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    command, map, splitOn
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

       
        /// <summary>
        /// Perform an asynchronous multi-mapping query with an arbitrary number of input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="types">Array of types in the recordset.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static async Task<Result<IEnumerable<TReturn>, string>> SafeQueryAsync<TReturn>(this IDbConnection cnn, string sql, Type[] types, Func<object[], TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            try {
                return Result<IEnumerable<TReturn>, string>.Ok(await cnn.QueryAsync(
                    sql, types, map, param, transaction, buffered, splitOn, commandTimeout, commandType
                ));
            }
            catch (Exception e) {
                return Result<IEnumerable<TReturn>, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static async Task<Result<SqlMapper.GridReader, string>> SafeQueryMultipleAsync(this IDbConnection cnn, string sql,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null) {

            try {
                return Result<SqlMapper.GridReader, string>.Ok(await cnn.QueryMultipleAsync(
                    sql, param, transaction, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<SqlMapper.GridReader, string>.Err(e.ToString());
            }
        }
            

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command to execute for this query.</param>
        public static async Task<Result<SqlMapper.GridReader, string>> SafeQueryMultipleAsync(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<SqlMapper.GridReader, string>.Ok(await cnn.QueryMultipleAsync(command));
            }
            catch (Exception e) {
                return Result<SqlMapper.GridReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
        /// or <see cref="T:DataSet"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// DataTable table = new DataTable("MyTable");
        /// using (var reader = ExecuteReader(cnn, sql, param))
        /// {
        ///     table.Load(reader);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static async Task<Result<IDataReader, string>> SafeExecuteReaderAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {

            try {
                return Result<IDataReader, string>.Ok(await cnn.ExecuteReaderAsync(
                    sql, param, transaction, commandTimeout, commandType
                    ));
            }
            catch (Exception e) {
                return Result<IDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static async Task<Result<DbDataReader, string>> SafeExecuteReaderAsync(this DbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            
            try {
                var reader = await cnn.ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType);
                return Result<DbDataReader, string>.Ok((DbDataReader)reader);
            }
            catch (Exception e) {
                return Result<DbDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
        /// or <see cref="T:DataSet"/>.
        /// </remarks>
        public static async Task<Result<IDataReader, string>> SafeExecuteReaderAsync(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<IDataReader, string>.Ok(await cnn.ExecuteReaderAsync(command));
            }
            catch (Exception e) {
                return Result<IDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        public static async Task<Result<DbDataReader, string>> SafeExecuteReaderAsync(this DbConnection cnn, CommandDefinition command) {
            try {
                var reader = await cnn.ExecuteReaderAsync(command);
                return Result<DbDataReader, string>.Ok((DbDataReader)reader);
            }
            catch (Exception e) {
                return Result<DbDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return an <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> flags for this reader.</param>
        /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
        /// or <see cref="T:DataSet"/>.
        /// </remarks>
        public static async Task<Result<IDataReader, string>> SafeExecuteReaderAsync(this IDbConnection cnn, CommandDefinition command,
            CommandBehavior commandBehavior) {
            try {
                return Result<IDataReader, string>.Ok(await cnn.ExecuteReaderAsync(command, commandBehavior));
            }
            catch (Exception e) {
                return Result<IDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL and return a <see cref="DbDataReader"/>.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="commandBehavior">The <see cref="CommandBehavior"/> flags for this reader.</param>
        public static async Task<Result<IDataReader, string>> SafeExecuteReaderAsync(this DbConnection cnn, CommandDefinition command,
            CommandBehavior commandBehavior) {

            try {
                return Result<IDataReader, string>.Ok(await cnn.ExecuteReaderAsync(command, commandBehavior));
            }
            catch (Exception e) {
                return Result<IDataReader, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <see cref="object"/>.</returns>
        public static async Task<Result<object, string>> SafeExecuteScalarAsync(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            try {
                return Result<object, string>.Ok(await cnn.ExecuteScalarAsync(
                    sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }
            

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sql">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T"/>.</returns>
        public static async Task<Result<T, string>> SafeExecuteScalarAsync<T>(this IDbConnection cnn, string sql, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            try {
                return Result<T, string>.Ok(await cnn.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }
            

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>The first cell selected as <see cref="object"/>.</returns>
        public static async Task<Result<object, string>> SafeExecuteScalarAsync(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<object, string>.Ok(await cnn.ExecuteScalarAsync(command));
            }
            catch (Exception e) {
                return Result<object, string>.Err(e.ToString());
            }
        }

        /// <summary>
        /// Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>The first cell selected as <typeparamref name="T"/>.</returns>
        public static async Task<Result<T, string>> SafeExecuteScalarAsync<T>(this IDbConnection cnn, CommandDefinition command) {
            try {
                return Result<T, string>.Ok(await cnn.ExecuteScalarAsync<T>(command));
            }
            catch (Exception e) {
                return Result<T, string>.Err(e.ToString());
            }
        }
}