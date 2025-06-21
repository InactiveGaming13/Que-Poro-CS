using Npgsql;

namespace QuePoro.Database;

public static class Database
{
    public static NpgsqlDataSource GetDataSource()
    {
        string? databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
        string? databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
        string? databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
        string? databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");
        string? databaseDb = Environment.GetEnvironmentVariable("DATABASE_DB");

        string connectionString =
            $"Host={databaseHost};Port={databasePort};Username={databaseUser};Password={databasePass};Database={databaseDb}";
        return NpgsqlDataSource.Create(connectionString);
    }
}