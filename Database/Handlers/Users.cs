using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Users
{
    /// <summary>
    /// Adds a User to the database.
    /// </summary>
    /// <param name="userId">The ID of the User.</param>
    /// <param name="username">The username of the User.</param>
    /// <param name="globalName">The global name of the User.</param>
    /// <param name="admin">Whether to make the User an administrator.</param>
    /// <param name="repliedTo">Whether to reply to the User.</param>
    /// <param name="banned">Whether to ban to the User.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddUser(ulong userId, string username, string? globalName, bool admin = false,
        bool repliedTo = true, bool banned = false)
    {
        if ((long)userId == Convert.ToInt64(Environment.GetEnvironmentVariable("BOT_OWNER_ID")))
            admin = true;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = 
            "INSERT INTO users (id, created_at, username, global_name, admin, replied_to, banned) " +
            "VALUES (@id, CURRENT_TIMESTAMP, @username, @globalName, @admin, @repliedTo, @banned)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)userId });
        command.Parameters.Add(new NpgsqlParameter("username", NpgsqlDbType.Text) { Value = username });
        command.Parameters.Add(globalName is null
        ? new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = DBNull.Value }
        : new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = globalName });
        command.Parameters.Add(new NpgsqlParameter("admin", NpgsqlDbType.Boolean) { Value = admin });
        command.Parameters.Add(new NpgsqlParameter("repliedTo", NpgsqlDbType.Boolean) { Value = repliedTo });
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

    /// <summary>
    /// Removes a User from the database.
    /// </summary>
    /// <param name="userId">The ID of the User.</param>
    /// <returns>Whether the operation succeeds.</returns>
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
    
    /// <summary>
    /// Modifies a User in the database.
    /// </summary>
    /// <param name="id">The ID of the User.</param>
    /// <param name="globalName">The global name of the User.</param>
    /// <param name="username">The username of the User.</param>
    /// <param name="admin">Whether to set the User as an administrator.</param>
    /// <param name="repliedTo">Whether to reply to the User.</param>
    /// <param name="reactedTo">Whether to react to the User.</param>
    /// <param name="tracked">Whether to track the User.</param>
    /// <param name="banned">Whether to ban the User.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyUser(ulong id, string? globalName, string? username = null,
        bool? admin = null, bool? repliedTo = null, bool? reactedTo = null, bool? tracked = null, bool? banned = null)
    {
        if (username is null && admin is null && repliedTo is null && reactedTo is null && tracked is null &&
            banned is null) return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE users SET";

        if (username != null)
        {
            query += " username=@username,";
            command.Parameters.Add(new NpgsqlParameter("username", NpgsqlDbType.Text) { Value = username });
        }

        query += " global_name=@globalName,";
        command.Parameters.Add(globalName is null
        ? new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = DBNull.Value }
        : new NpgsqlParameter("globalName", NpgsqlDbType.Text) { Value = globalName }); 

        if (admin is not null)
        {
            query += " admin=@admin,";
            command.Parameters.Add(new NpgsqlParameter("admin", NpgsqlDbType.Boolean) { Value = admin });
        }

        if (repliedTo is not null)
        {
            query += " replied_to=@repliedTo";
            command.Parameters.Add(new NpgsqlParameter("repliedTo", NpgsqlDbType.Boolean) { Value = repliedTo });
        }

        if (reactedTo != null)
        {
            query += " reacted_to=@reactedTo,";
            command.Parameters.Add(new NpgsqlParameter("reactedTo", NpgsqlDbType.Boolean) { Value = reactedTo });
        }

        if (tracked != null)
        {
            query += " tracked=@tracked,";
            command.Parameters.Add(new NpgsqlParameter("tracked", NpgsqlDbType.Boolean) { Value = tracked });
        }

        if (banned is not null)
        {
            query += " banend=@banned";
            command.Parameters.Add(new NpgsqlParameter("banned", NpgsqlDbType.Boolean) { Value = banned });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Gets a User from the database.
    /// </summary>
    /// <param name="id">The ID of the User.</param>
    /// <returns>The User.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the User doesn't exist.</exception>
    public static async Task<UserRow> GetUser(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM users WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string? globalName = null;
            try
            {
                globalName = reader.GetString(reader.GetOrdinal("global_name"));
            }
            catch (Exception)
            {
                // ignore
            }
                
            return new UserRow
            {
                Id = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                Username = reader.GetString(reader.GetOrdinal("username")),
                GlobalName = globalName,
                Admin = reader.GetBoolean(reader.GetOrdinal("admin")),
                RepliedTo = reader.GetBoolean(reader.GetOrdinal("replied_to")),
                ReactedTo = reader.GetBoolean(reader.GetOrdinal("reacted_to")),
                Banned = reader.GetBoolean(reader.GetOrdinal("banned"))
            };
        }

        throw new KeyNotFoundException($"No User exists with ID: {id}");
    }
    
    /// <summary>
    /// Checks if a User exists in the database.
    /// </summary>
    /// <param name="id">The ID of the User.</param>
    /// <returns>Whether the User exists.</returns>
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