using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Channels
{
    /// <summary>
    /// Adds a Channel to the database.
    /// </summary>
    /// <param name="id">The ID of the Channel.</param>
    /// <param name="guildId">The ID of the Guild.</param>
    /// <param name="name">The name of the Channel.</param>
    /// <param name="topic">The topic of the Channel.</param>
    /// <param name="messages">The number of messages in the channel.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddChannel(ulong id, ulong guildId, string name, string? topic = null, 
        int messages = 0)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "INSERT INTO channels (id, created_at, guild_id, name, topic) VALUES " + 
            "(@id, CURRENT_TIMESTAMP, @guildId, @name, @topic)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        command.Parameters.Add(topic is null
            ? new NpgsqlParameter("topic", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("topic", NpgsqlDbType.Text) { Value = topic });
        command.Parameters.Add(new NpgsqlParameter("messages", NpgsqlDbType.Integer) { Value = messages });
            
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
    /// Removes a Channel from the database.
    /// </summary>
    /// <param name="id">The ID of the channel.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveChannel(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = "DELETE FROM channels WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
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
    /// Modifies a Channel in the database.
    /// </summary>
    /// <param name="id">The ID of the Channel.</param>
    /// <param name="name">The name of the Channel.</param>
    /// <param name="topic">The topic of the Channel.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyChannel(ulong id, string? name = null,
        string? topic = null)
    {
        if (name == null && topic == null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE channels SET";

        if (name != null)
        {
            query += " name=@name,";
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        }

        if (topic != null)
        {
            query += " topic=@topic";
            command.Parameters.Add(new NpgsqlParameter("topic", NpgsqlDbType.Text) { Value = topic });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
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
    /// Gets a Channel from the database.
    /// </summary>
    /// <param name="id">The ID of the Channel.</param>
    /// <returns>The Channel.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Channel doesn't exist.</exception>
    public static async Task<ChannelRow> GetChannel(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = $"SELECT * FROM channels WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        
        try
        {
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string? description = null;
                try
                {
                    description = reader.GetString(reader.GetOrdinal("description"));
                }
                catch (Exception)
                {
                    // ignored
                }

                return new ChannelRow
                {
                    Id = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                    Description = description,
                };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        throw new KeyNotFoundException($"No Channel exists with ID: {id}");
    }
    
    /// <summary>
    /// Checks if a Channel exists in the database.
    /// </summary>
    /// <param name="id">The ID of the Channel.</param>
    /// <returns>Whether the Channel exists.</returns>
    public static async Task<bool> ChannelExists(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM channels WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        return command.ExecuteScalar() is not null;
    }
}