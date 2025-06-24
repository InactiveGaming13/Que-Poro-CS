using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Channels
{
    public static async Task AddChannel(ulong id, ulong guildId, string name, string? description = null, int messages = 0)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query =
            "INSERT INTO channels (id, guild_id, name, description, messages) VALUES " + 
            "(@id, @guildId, @name, @description, @messages)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        command.Parameters.Add(description is null
            ? new NpgsqlParameter("description", NpgsqlDbType.Text) { Value = DBNull.Value }
            : new NpgsqlParameter("description", NpgsqlDbType.Text) { Value = description });
        command.Parameters.Add(new NpgsqlParameter("messages", NpgsqlDbType.Integer) { Value = messages });
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static async Task RemoveChannel(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        string query = "DELETE FROM channels WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static async Task ModifyChannel(ulong id, ulong? guildId = null, string? name = null,
        string? description = null, int? messages = null)
    {
        if (guildId == null && name == null && description == null && messages == null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE channels SET";

        if (guildId != null)
        {
            query += " guild_id=@guildId,";
            command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        }

        if (name != null)
        {
            query += " name=@name,";
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        }

        if (description != null)
        {
            query += " description=@description,";
            command.Parameters.Add(new NpgsqlParameter("description", NpgsqlDbType.Text) { Value = description });
        }

        if (messages != null)
        {
            query += " messages=@messages";
            command.Parameters.Add(new NpgsqlParameter("messages", NpgsqlDbType.Integer) { Value = messages });
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
        }
    }

    public static async Task<ChannelRow?> GetChannel(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        string query = $"SELECT * FROM channels WHERE id={id}";

        command.CommandText = query;
        
        try
        {
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ulong phraseId = (ulong)reader.GetInt64(reader.GetOrdinal("id"));
                DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
                string name = reader.GetString(reader.GetOrdinal("name"));
                bool tracked = reader.GetBoolean(reader.GetOrdinal("tracked"));
                ulong guildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));
                string? description = null;
                try
                {
                    description = reader.GetString(reader.GetOrdinal("description"));
                }
                catch (Exception)
                {
                    // ignored
                }

                int messages = reader.GetInt32(reader.GetOrdinal("messages"));

                return new ChannelRow
                {
                    Id = phraseId,
                    CreatedAt = createdAt,
                    Name = name,
                    Tracked = tracked,
                    GuildId = guildId,
                    Description = description,
                    Messages = messages
                };
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }
}