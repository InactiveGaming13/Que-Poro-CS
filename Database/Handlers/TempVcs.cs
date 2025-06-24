using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class TempVcs
{
    public static async Task AddTempVc(ulong id, ulong createdBy, ulong guildId, string name, int bitrate, int userLimit,
        int userCount)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = 
            "INSERT INTO temp_vcs (id, created_by, guild_id, master, name, bitrate, user_limit, user_count) " +
            "VALUES (@id, @createdBy, @guildId, @master, @name, @bitrate, @userLimit, @userCount)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("createdBy", NpgsqlDbType.Numeric) { Value = (long)createdBy });
        command.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Numeric) { Value = (long)guildId });
        command.Parameters.Add(new NpgsqlParameter("master", NpgsqlDbType.Numeric) { Value = (long)createdBy });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        command.Parameters.Add(new NpgsqlParameter("bitrate", NpgsqlDbType.Integer) { Value = bitrate });
        command.Parameters.Add(new NpgsqlParameter("userLimit", NpgsqlDbType.Integer) { Value = userLimit });
        command.Parameters.Add(new NpgsqlParameter("userCount", NpgsqlDbType.Integer) { Value = userCount });
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static async Task RemoveTempVc(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        const string query = 
            "DELETE FROM temp_vcs WHERE id=@id";

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
    
    public static async Task ModifyTempVc(ulong id, string? name = null, int? bitrate = null, int? userLimit = null,
        int? userCount = null)
    {
        if (name is null && bitrate is null && userLimit is null && userCount is null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        string query = 
            "UPDATE temp_vcs SET";

        if (name is not null)
        {
            query += " name=@name,";
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        }

        if (bitrate is not null)
        {
            query += " bitrate=@bitrate,";
            command.Parameters.Add(new NpgsqlParameter("bitrate", NpgsqlDbType.Integer) { Value = bitrate });
        }

        if (userLimit is not null)
        {
            query += " user_limit=@userLimit";
            command.Parameters.Add(new NpgsqlParameter("userLimit", NpgsqlDbType.Integer) { Value = userLimit });
        }

        if (userCount is not null)
        {
            query += "user_count=@userCount";
            command.Parameters.Add(new NpgsqlParameter("userCount", NpgsqlDbType.Integer) { Value = userCount });
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
    
    public static async Task<TempVcRow?> GetTempVc(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        const string query = "SELECT * FROM temp_vcs WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
            
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new TempVcRow
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                CreatedBy = (ulong)reader.GetInt64(reader.GetOrdinal("created_by")),
                GuildId = (ulong)reader.GetInt64(reader.GetOrdinal("guild_id")),
                Master = (ulong)reader.GetInt64(reader.GetOrdinal("master")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Bitrate = reader.GetInt32(reader.GetOrdinal("bitrate")),
                UserLimit = reader.GetInt32(reader.GetOrdinal("user_limit")),
                UserCount = reader.GetInt32(reader.GetOrdinal("user_count"))
            };
        }

        return null;
    }
}