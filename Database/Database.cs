using Npgsql;

namespace QuePoro.Database;

public class Database
{
    public static NpgsqlDataSource DataSource;
    public static async Task ConnectToDb()
    {
        string connString =
            $"Host={Environment.GetEnvironmentVariable("DATABASE_HOST")};" +
            $"Username={Environment.GetEnvironmentVariable("DATABASE_USER")};" +
            $"Password={Environment.GetEnvironmentVariable("DATABASE_PASS")};" +
            $"Database={Environment.GetEnvironmentVariable("DATABASE_DB")}";
        
        NpgsqlConnection connection;
        while (true)
            try
            {
                connection = new NpgsqlConnection(connString);
                break;
            }
            catch (PostgresException exception)
            {
                if (exception.SqlState != "53300") throw;
                Console.WriteLine(
                    "Connection limit hit. Waiting 500ms before trying again.");
                await Task.Delay(500);
            }

        connection.Open();
    }

    public static async Task AddData()
    {
        // Insert some data
        await using var cmd = DataSource.CreateCommand("INSERT INTO users (username) VALUES ('User 1')");
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task GetData()
    {
        // Retrieve all rows
        await using (var cmd = DataSource.CreateCommand("SELECT * FROM users"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }
    }
}