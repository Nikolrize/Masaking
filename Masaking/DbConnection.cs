using DotNetEnv;
using Npgsql;
using System;
using System.IO;

public static class DbConnection
{
    private static bool envLoaded = false;

    private static void LoadEnvironment()
    {
        if (envLoaded)
            return;

        string envPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ".env"
        );

        Env.Load(envPath);
        envLoaded = true;
    }

    public static NpgsqlConnection GetConnection()
    {
        LoadEnvironment();

        string connectionString =
            Environment.GetEnvironmentVariable(
                "SUPABASE_CONNECTION_STRING"
            );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "SUPABASE_CONNECTION_STRING is missing from .env."
            );
        }

        return new NpgsqlConnection(connectionString);
    }
}
