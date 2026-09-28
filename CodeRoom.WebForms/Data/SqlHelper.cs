using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// Small, explicit ADO.NET helpers. They remove repetition (opening connections, declaring
    /// parameters, reading nullable columns) without hiding the underlying SqlCommand,
    /// SqlDataReader, SqlDataAdapter or SqlTransaction usage - repositories still write their own
    /// parameterised SQL and still drive the reader row by row.
    /// </summary>
    public static class SqlHelper
    {
        /// <summary>Values that must be sent to the database as SQL NULL.</summary>
        public static readonly object Null = DBNull.Value;

        /// <summary>Empty strings are stored as NULL, matching the nullable model properties.</summary>
        public static object Value(string value)
        {
            return string.IsNullOrEmpty(value) ? Null : value;
        }

        public static object Value(DateTime? value)
        {
            return value.HasValue ? value.Value : Null;
        }

        public static object Value(int? value)
        {
            return value.HasValue ? value.Value : Null;
        }

        /// <summary>Build a command, optionally attached to an open transaction.</summary>
        public static SqlCommand Prepare(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            CommandType commandType = CommandType.Text)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }

        /// <summary>
        /// Add a typed parameter. The explicit SqlDbType keeps the column type, parameter length and
        /// value together, and no application value is ever concatenated into the statement text.
        /// </summary>
        public static SqlParameter Add(SqlCommand command, string name, SqlDbType type, object value)
        {
            var parameter = command.Parameters.Add(name, type);
            parameter.Value = value ?? Null;
            return parameter;
        }

        public static SqlParameter AddInt(SqlCommand command, string name, int value)
        {
            return Add(command, name, SqlDbType.Int, value);
        }

        public static SqlParameter AddBool(SqlCommand command, string name, bool value)
        {
            return Add(command, name, SqlDbType.Bit, value);
        }

        public static SqlParameter AddDateTime(SqlCommand command, string name, DateTime value)
        {
            return Add(command, name, SqlDbType.DateTime2, value);
        }

        public static SqlParameter AddNVarChar(SqlCommand command, string name, string value, int size)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.NVarChar, size);
            parameter.Value = Value(value);
            return parameter;
        }

        public static SqlParameter AddText(SqlCommand command, string name, string value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.NVarChar, -1);
            parameter.Value = Value(value);
            return parameter;
        }

        public static SqlParameter AddOutput(SqlCommand command, string name, SqlDbType type)
        {
            var parameter = command.Parameters.Add(name, type);
            parameter.Direction = ParameterDirection.Output;
            return parameter;
        }

        // ------------------------------------------------------------- execution --

        public static int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return ExecuteNonQuery(connection, null, commandText, parameters);
            }
        }

        public static int ExecuteNonQuery(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            params SqlParameter[] parameters)
        {
            using (var command = Prepare(connection, transaction, commandText))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return ExecuteScalar(connection, null, commandText, parameters);
            }
        }

        public static object ExecuteScalar(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            params SqlParameter[] parameters)
        {
            using (var command = Prepare(connection, transaction, commandText))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }

        public static int ExecuteIntScalar(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            params SqlParameter[] parameters)
        {
            var result = ExecuteScalar(connection, transaction, commandText, parameters);
            return result == null || result == Null ? 0 : Convert.ToInt32(result);
        }

        public static bool Exists(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return ExecuteIntScalar(connection, null, commandText, parameters) > 0;
            }
        }

        // ---------------------------------------------------------------- reading --

        /// <summary>Read every row through a mapper, in one SqlDataReader pass.</summary>
        public static List<T> ReadList<T>(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            Func<SqlDataReader, T> map,
            params SqlParameter[] parameters)
        {
            var results = new List<T>();

            using (var command = Prepare(connection, transaction, commandText))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(map(reader));
                    }
                }
            }

            return results;
        }

        public static List<T> ReadList<T>(string commandText, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return ReadList(connection, null, commandText, map, parameters);
            }
        }

        /// <summary>Read the first row, or return default(T) when the result set is empty.</summary>
        public static T ReadFirst<T>(
            SqlConnection connection,
            SqlTransaction transaction,
            string commandText,
            Func<SqlDataReader, T> map,
            params SqlParameter[] parameters)
        {
            using (var command = Prepare(connection, transaction, commandText))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? map(reader) : default(T);
                }
            }
        }

        public static T ReadFirst<T>(string commandText, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            using (var connection = DbConnectionFactory.Open())
            {
                return ReadFirst(connection, null, commandText, map, parameters);
            }
        }

        // ------------------------------------------------------------ data tables --

        /// <summary>
        /// Fill a disconnected DataTable. Used for the administrative grids, where Web Forms binds a
        /// DataSource rather than a typed list.
        /// </summary>
        public static DataTable GetTable(string commandText, params SqlParameter[] parameters)
        {
            var table = new DataTable();

            using (var connection = DbConnectionFactory.Open())
            using (var adapter = new SqlDataAdapter(commandText, connection))
            {
                if (parameters != null)
                {
                    adapter.SelectCommand.Parameters.AddRange(parameters);
                }

                adapter.Fill(table);
                return table;
            }
        }

        // ------------------------------------------------------------- transactions --

        /// <summary>
        /// Run a unit of work inside one SqlTransaction. The work commits only when it returns
        /// normally; any exception rolls every statement back.
        /// </summary>
        public static void WithTransaction(Action<SqlConnection, SqlTransaction> work)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    work(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    SafeRollback(transaction);
                    throw;
                }
            }
        }

        public static T WithTransaction<T>(Func<SqlConnection, SqlTransaction, T> work)
        {
            using (var connection = DbConnectionFactory.Open())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var result = work(connection, transaction);
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    SafeRollback(transaction);
                    throw;
                }
            }
        }

        private static void SafeRollback(SqlTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
                // The transaction was already ended by the server (for example a deadlock); nothing to roll back.
            }
        }

        // ---------------------------------------------------------- column readers --

        public static string GetString(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        public static string GetNullableString(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static int GetInt(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        public static int? GetNullableInt(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
        }

        public static long GetLong(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt64(ordinal);
        }

        public static bool GetBool(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }

        public static DateTime GetDateTime(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? DateTime.MinValue : reader.GetDateTime(ordinal);
        }

        public static DateTime? GetNullableDateTime(SqlDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }
    }
}
