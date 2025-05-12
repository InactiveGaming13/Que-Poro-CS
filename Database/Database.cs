using Npgsql;
using NpgsqlTypes;

namespace QuePoro.Database;

public class Database
{
    public static async Task DatabaseThings()
    {
        string? databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
        string? databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
        string? databaseUser = Environment.GetEnvironmentVariable("DATABASE_USER");
        string? databasePass = Environment.GetEnvironmentVariable("DATABASE_PASS");
        string? databaseDb = Environment.GetEnvironmentVariable("DATABASE_DB");

        var connectionString =
            $"Host={databaseHost};Port={databasePort};Username={databaseUser};Password={databasePass};Database={databaseDb}";
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

// Insert some data
        await using (var cmd = dataSource.CreateCommand(
                         "INSERT INTO media (created_by, alias, category, url) VALUES ($1, $2, $3, $4)"))
        {
            cmd.Parameters.AddWithValue(123456789000);
            cmd.Parameters.AddWithValue("Hello world");
            cmd.Parameters.AddWithValue("HW1");
            cmd.Parameters.AddWithValue("https://google.com");
            await cmd.ExecuteNonQueryAsync();
        }

// Retrieve all rows
        await using (var cmd = dataSource.CreateCommand("SELECT created_by, alias, category, url FROM media"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }
    }
}