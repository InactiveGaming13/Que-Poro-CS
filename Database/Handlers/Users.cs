using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Users
{
    public static async Task<bool> AddUser(ulong userId, string username, string? globalName, bool admin = false,
        bool repliedTo = true, bool tracked = true, bool banned = false)
    {
        if ((long)userId == Convert.ToInt64(Environment.GetEnvironmentVariable("BOT_OWNER_ID")))
            admin = true;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = 
            "INSERT INTO users (id, username, global_name, admin, replied_to, tracked, banned) " +
            "VALUES (@id, @username, @globalName, @admin, @repliedTo, @tracked, @banned)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("username", NpgsqlDbType.Text) { Value = username });
        command.Parameters.Add(globalName is null
        ? new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = DBNull.Value }
        : new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = globalName });
        command.Parameters.Add(new NpgsqlParameter("admin", NpgsqlDbType.Boolean) { Value = admin });
        command.Parameters.Add(new NpgsqlParameter("repliedTo", NpgsqlDbType.Boolean) { Value = repliedTo });
        command.Parameters.Add(new NpgsqlParameter("tracked", NpgsqlDbType.Boolean) { Value = tracked });
        command.Parameters.Add(new NpgsqlParameter("banned", NpgsqlDbType.Boolean) { Value = banned });

        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public static async Task<bool> RemoveUser(ulong userId)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "DELETE FROM users WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = userId });
        
        try
        {
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public static async Task<UserRow> GetUser(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = $"SELECT * FROM users WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong userId = (ulong)reader.GetInt64(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            string username = reader.GetString(reader.GetOrdinal("username"));
            string globalName = reader.GetString(reader.GetOrdinal("global_name"));
            bool admin = reader.GetBoolean(reader.GetOrdinal("admin"));
            bool repliedTo = reader.GetBoolean(reader.GetOrdinal("replied_to"));
            bool reactedTo = reader.GetBoolean(reader.GetOrdinal("reacted_to"));
            bool tracked = reader.GetBoolean(reader.GetOrdinal("tracked"));
            bool banned = reader.GetBoolean(reader.GetOrdinal("banned"));
                
            return new UserRow
            {
                Id = userId,
                CreatedAt = createdAt,
                Username = username,
                GlobalName = globalName,
                Admin = admin,
                RepliedTo = repliedTo,
                ReactedTo = reactedTo,
                Tracked = tracked,
                Banned = banned
            };
        }

        throw new KeyNotFoundException($"No User was found with id: {id}");
    }
    
    public static async Task<bool> UserExists(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM users WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        return command.ExecuteScalar() is not null;
    }
}