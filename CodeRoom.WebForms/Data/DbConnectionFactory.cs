using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CodeRoom.WebForms.Data
{
    /// <summary>
    /// The single place where a SqlConnection is created. Every repository opens its connection
    /// through here so the Web.config connection string is the only database configuration.
    /// </summary>
    public static class DbConnectionFactory
    {
        public const string DefaultConnectionName = "DefaultConnection";

        /// <summary>The configured connection string, read once per call from Web.config.</summary>
        public static string ConnectionString
        {
            get
            {
                var testOverride = Environment.GetEnvironmentVariable("CODEROOM_TEST_CONNECTION_STRING");
                if (!string.IsNullOrWhiteSpace(testOverride))
                {
                    return testOverride;
                }

                var setting = ConfigurationManager.ConnectionStrings[DefaultConnectionName];

                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "ConnectionStrings/" + DefaultConnectionName + " is missing from Web.config.");
                }

                return setting.ConnectionString;
            }
        }

        /// <summary>Open a connection ready for use. The caller owns disposal.</summary>
        public static SqlConnection Open()
        {
            return Open(ConnectionString);
        }

        /// <summary>Open a connection to an explicit database, used by initialization.</summary>
        public static SqlConnection OpenToDatabase(string databaseName)
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = databaseName };
            return Open(builder.ConnectionString);
        }

        /// <summary>
        /// Open a connection to the instance (master) rather than to an application database, so a
        /// missing CodeRoomDb can be detected and created without a failing login.
        /// </summary>
        public static SqlConnection OpenToInstance()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
            return Open(builder.ConnectionString);
        }

        public static string DatabaseName
        {
            get { return new SqlConnectionStringBuilder(ConnectionString).InitialCatalog; }
        }

        private static SqlConnection Open(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
