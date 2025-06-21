using Npgsql;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Channels
{
    private static readonly NpgsqlDataSource DataSource = Database.GetDataSource();
    
    public static async Task AddChannel(ulong id, ulong guildId, string name, string description, int? messages = null)
    {
        messages ??= 0;
        
        const string query =
            "INSERT INTO channels (id, guild_id, name, description, messages) VALUES ($1, $2, $3, $4, $5)";
        await using var cmd = DataSource.CreateCommand(query);
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(guildId);
        cmd.Parameters.AddWithValue(name);
        cmd.Parameters.AddWithValue(description);
        cmd.Parameters.AddWithValue(messages);
            
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259)
            {
                Console.WriteLine("Unexpected Postgres Error");
                return;
            }
                
            Console.WriteLine($"{e.ErrorCode} | {e.Message}");
        }
    }
    
    public static async Task RemoveChannel(ulong id)
    {
        const string query = "DELETE FROM channels WHERE id=$1";
        await using var cmd = DataSource.CreateCommand(query);
        cmd.Parameters.AddWithValue(id);
            
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259)
            {
                Console.WriteLine("Unexpected Postgres Error");
            }
        }
    }

    public static async Task ModifyChannel(ulong id, ulong? guildId = null, string? name = null,
        string? description = null, int? messages = null)
    {
        if (guildId == null && name == null && description == null && messages == null)
            return;
            
        string query = "UPDATE banned_phrases SET";

        if (guildId != null)
            query += $" guild_id={guildId},";

        if (name != null)
            query += $" name='{name}',";

        if (description != null)
            query += $" description='{description}',";

        if (messages != null)
            query += $" messages={messages}";

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=$1";
            
        await using var cmd = DataSource.CreateCommand(query);
        cmd.Parameters.AddWithValue(id);
            
        try
        {
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259)
            {
                Console.WriteLine("Unexpected Postgres Error");
            }
        }
    }

    public static async Task<ChannelRow?> GetChannel(ulong id)
    {
        string query = "SELECT * FROM banned_phrases WHERE id=$1";

        await using NpgsqlCommand command = DataSource.CreateCommand(query);
        command.Parameters.AddWithValue(id);
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong phraseId = (ulong)reader.GetInt64(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            bool tracked = reader.GetBoolean(reader.GetOrdinal("tracked"));
            ulong guildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id"));
            string description = reader.GetString(reader.GetOrdinal("description"));
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

        return null;
    }
}