using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Guilds
{
    public static async Task AddGuild(ulong id, string name, bool tracked = true, ulong tempVcChannel = 0,
        int tempVcDefaultMemberLimit = 5, int tempVcDefaultBitrate = 64, ulong robloxAlertChannel = 0,
        int robloxAlertInterval = 60)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query = 
            "INSERT INTO guilds (id, name, tracked, temp_vc_channel, temp_vc_default_member_limit," +
            "temp_vc_default_bitrate, roblox_alert_channel, roblox_alert_interval) VALUES (@id, @name, @tracked," +
            " @tempVcChannel, @tempVcDefaultMemberLimit, @tempVcDefaultBitrate, @robloxAlertChannel, @robloxAlertInterval)";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        command.Parameters.Add(new NpgsqlParameter("tracked", NpgsqlDbType.Boolean) { Value = tracked });
        command.Parameters.Add(new NpgsqlParameter("tempVcChannel", NpgsqlDbType.Numeric) { Value = (long)tempVcChannel });
        command.Parameters.Add(new NpgsqlParameter("tempVcDefaultMemberLimit", NpgsqlDbType.Integer) { Value = tempVcDefaultMemberLimit });
        command.Parameters.Add(new NpgsqlParameter("tempVcDefaultBitrate", NpgsqlDbType.Integer) { Value = tempVcDefaultBitrate });
        command.Parameters.Add(new NpgsqlParameter("robloxAlertChannel", NpgsqlDbType.Numeric) { Value = (long)robloxAlertChannel });
        command.Parameters.Add(new NpgsqlParameter("robloxAlertInterval", NpgsqlDbType.Integer) { Value = robloxAlertInterval });
        
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static async Task RemoveGuild(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query = "DELETE FROM guilds WHERE id=$1";

        command.CommandText = query;
        command.Parameters.AddWithValue(id);
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static async Task ModifyGuild(ulong id, string? name = null, ulong? tempVcChannel = null,
        int? tempVcMemberDefault = null, int? tempVcBitrateDefault = null, ulong? robloxAlertChannel = null,
        int? robloxAlertInterval = null)
    {
        if (name == null && tempVcChannel == null && tempVcMemberDefault == null && tempVcBitrateDefault == null &&
            robloxAlertChannel == null && robloxAlertInterval == null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
            
        string query = "UPDATE banned_phrases SET";

        if (name != null)
            query += $" name='{name}',";

        if (tempVcChannel != null)
            query += $" temp_vc_channel='{tempVcChannel}'";

        if (tempVcMemberDefault != null)
            query += $" temp_vc_default_member_limit={tempVcMemberDefault},";

        if (tempVcBitrateDefault != null)
            query += $" temp_vc_default_bitrate={tempVcBitrateDefault},";
        
        if (robloxAlertChannel != null)
            query += $" roblox_alert_channel={robloxAlertChannel},";

        if (robloxAlertInterval != null)
            query += $" roblox_alert_interval={robloxAlertInterval}";

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        query += " WHERE id=$1";

        command.CommandText = query;
        command.Parameters.AddWithValue(id);
            
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public static async Task<GuildRow?> GetGuild(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query = $"SELECT * FROM guilds WHERE id={id}";

        command.CommandText = query;
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong guildId = (ulong)reader.GetInt64(reader.GetOrdinal("id"));
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            string guildName = reader.GetString(reader.GetOrdinal("name"));
            bool tracked = reader.GetBoolean(reader.GetOrdinal("tracked"));
            ulong tempVcChannel = (ulong)reader.GetInt64(reader.GetOrdinal("temp_vc_channel"));
            short tempVcDefaultMemberLimit = reader.GetInt16(reader.GetOrdinal("temp_vc_default_member_limit"));
            int tempVcDefaultBitrate = reader.GetInt32(reader.GetOrdinal("temp_vc_default_bitrate"));
            ulong robloxAlertChannel = (ulong)reader.GetInt64(reader.GetOrdinal("roblox_alert_channel"));
            int robloxAlertInterval = reader.GetInt32(reader.GetOrdinal("roblox_alert_interval"));

            return new GuildRow
            {
                Id = guildId,
                CreatedAt = createdAt,
                Name = guildName,
                Tracked = tracked,
                TempVcChannel = tempVcChannel,
                TempVcDefaultMemberLimit = tempVcDefaultMemberLimit,
                TempVcDefaultBitrate = tempVcDefaultBitrate,
                RobloxAlertChannel = robloxAlertChannel,
                RobloxAlertInterval = robloxAlertInterval
            };
        }

        return null;
    }
}