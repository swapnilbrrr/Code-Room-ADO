using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// Prepares the SQL Server database for the application without Entity Framework: it detects whether
    /// the configured database exists, creates it when it does not, runs the schema script batch by batch
    /// and then seeds the baseline content. Nothing here drops an existing database - an installed
    /// CodeRoomDb is brought up to the current schema by the guarded statements in the script instead.
    /// </summary>
    public static class DatabaseInitializer
    {
        private static readonly object InitializeLock = new object();

        private static bool initialized;

        private static readonly Regex BatchSeparator =
            new Regex(@"^[ \t]*GO[ \t]*(--.*)?\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>Virtual path of the SQL Server schema script shipped with the project.</summary>
        public const string SchemaScript = "~/Database/CodeRoom.sql";

        /// <summary>Create the database and schema if missing, then run the seed action once.</summary>
        public static InitializationResult Initialize(Action seed)
        {
            return Initialize(SchemaScript, seed);
        }

        /// <summary>
        /// The start-up entry point: bring the schema up to date and write the baseline content,
        /// reporting how many seed rows the run actually created.
        /// </summary>
        public static InitializationResult InitializeWithSeedData()
        {
            DbSeeder.SeedReport seedReport = null;

            var result = Initialize(() => seedReport = DbSeeder.Seed());
            result.SeedRowsCreated = seedReport == null ? 0 : seedReport.TotalRowsCreated;
            return result;
        }

        public static InitializationResult Initialize(string schemaScriptVirtualPath, Action seed)        {
            lock (InitializeLock)
            {
                if (initialized)
                {
                    return InitializationResult.AlreadyInitialized;
                }

                var databaseName = DbConnectionFactory.DatabaseName;
                var result = new InitializationResult
                {
                    DatabaseName = databaseName,
                    DatabaseExistedBefore = DatabaseExists(databaseName)
                };

                if (!result.DatabaseExistedBefore)
                {
                    CreateDatabase(databaseName);
                    result.DatabaseCreated = true;
                }

                var scriptPath = MapPath(schemaScriptVirtualPath);

                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException("The SQL Server schema script was not found.", scriptPath);
                }

                result.BatchesExecuted = ExecuteScript(File.ReadAllText(scriptPath), databaseName);

                if (seed != null)
                {
                    seed();
                }

                initialized = true;
                return result;
            }
        }

        /// <summary>Reset the once-per-app-domain guard; used by the data layer tests.</summary>
        public static void ResetForTests()
        {
            lock (InitializeLock)
            {
                initialized = false;
            }
        }

        /// <summary>True when the configured database is present on the instance.</summary>
        public static bool DatabaseExists(string databaseName)
        {
            using (var connection = DbConnectionFactory.OpenToInstance())
            using (var command = SqlHelper.Prepare(connection, null,
                       "SELECT COUNT(1) FROM sys.databases WHERE name = @Name;"))
            {
                SqlHelper.AddNVarChar(command, "@Name", databaseName, 128);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        /// <summary>
        /// Create the application database on the configured instance. The name comes from Web.config, is
        /// checked against a strict pattern and then bracketed, because identifiers cannot be parameterised.
        /// </summary>
        public static void CreateDatabase(string databaseName)
        {
            if (!Regex.IsMatch(databaseName, @"^[A-Za-z0-9_]{1,128}$"))
            {
                throw new ArgumentException("The configured database name is not a safe SQL Server identifier.", nameof(databaseName));
            }

            using (var connection = DbConnectionFactory.OpenToInstance())
            using (var command = SqlHelper.Prepare(connection, null, "CREATE DATABASE [" + databaseName + "];"))
            {
                command.CommandTimeout = 120;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Run the schema script against the application database, one GO separated batch at a time, and
        /// return how many batches were executed.
        /// </summary>
        public static int ExecuteScript(string script, string databaseName)
        {
            var batches = new List<string>();
            var parts = BatchSeparator.Split("\n" + script + "\n");

            foreach (var part in parts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    batches.Add(part);
                }
            }

            using (var connection = DbConnectionFactory.OpenToDatabase(databaseName))
            {
                for (var i = 0; i < batches.Count; i++)
                {
                    using (var command = SqlHelper.Prepare(connection, null, batches[i]))
                    {
                        command.CommandTimeout = 120;
                        command.ExecuteNonQuery();
                    }
                }
            }

            return batches.Count;
        }

        /// <summary>Table names currently present in the application database.</summary>
        public static List<string> GetTables(string databaseName)
        {
            const string sql = "SELECT name FROM sys.tables ORDER BY name;";

            using (var connection = DbConnectionFactory.OpenToDatabase(databaseName))
            {
                return SqlHelper.ReadList(connection, null, sql, reader => SqlHelper.GetString(reader, "name"));
            }
        }

        /// <summary>Resolve a "~" path outside of an HTTP request, for start-up initialization.</summary>
        public static string MapPath(string virtualPath)
        {
            string rooted = null;

            try
            {
                rooted = HostingEnvironment.MapPath(virtualPath);
            }
            catch (System.Web.HttpException)
            {
                // No ASP.NET runtime host, e.g. a console tool running the same initializer.
            }

            if (!string.IsNullOrEmpty(rooted))
            {
                return rooted;
            }

            var relative = virtualPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
        }
    }

    /// <summary>What initialization actually did, so the result can be reported and logged.</summary>
    public class InitializationResult
    {
        public static readonly InitializationResult AlreadyInitialized = new InitializationResult
        {
            DatabaseExistedBefore = true,
            DatabaseCreated = false,
            BatchesExecuted = 0,
            AlreadyRan = true
        };

        public string DatabaseName { get; set; }
        public bool DatabaseExistedBefore { get; set; }
        public bool DatabaseCreated { get; set; }
        public int BatchesExecuted { get; set; }
        public int SeedRowsCreated { get; set; }
        public bool AlreadyRan { get; set; }
    }
}
