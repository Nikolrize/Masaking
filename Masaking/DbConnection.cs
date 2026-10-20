
using Npgsql;
using System.Configuration;

public static class DbConnection
{
    public static NpgsqlConnection GetConnection()
    {
        string connectionString =
            ConfigurationManager
                .ConnectionStrings["SupabaseConnection"]
                .ConnectionString;

        return new NpgsqlConnection(connectionString);
    }
}

