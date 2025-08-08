using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Guilds
{
    /// <summary>
    /// Adds a Guild to the database.
    /// </summary>
    /// <param name="id">The ID of the Guild.</param>
    /// <param name="name">The name of the Guild.</param>
    /// <param name="tracked"></param>
    /// <param name="tempVcChannel"></param>
    /// <param name="tempVcDefaultMemberLimit"></param>
    /// <param name="tempVcDefaultBitrate"></param>
    /// <param name="robloxAlertChannel"></param>
    /// <param name="robloxAlertInterval"></param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> AddGuild(ulong id, string name, bool tracked = true, ulong tempVcChannel = 0,
        int tempVcDefaultMemberLimit = 5, int tempVcDefaultBitrate = 64, ulong robloxAlertChannel = 0,
        int robloxAlertInterval = 60)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = 
            "INSERT INTO guilds (id, created_at, name, tracked, temp_vc_channel, temp_vc_default_member_limit," +
            "temp_vc_default_bitrate, roblox_alert_channel, roblox_alert_interval) VALUES (@id, CURRENT_TIMESTAMP, " +
            "@name, @tracked, @tempVcChannel, @tempVcDefaultMemberLimit, @tempVcDefaultBitrate, @robloxAlertChannel, " +
            "@robloxAlertInterval)";

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
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    /// <summary>
    /// Removes a Guild from the database.
    /// </summary>
    /// <param name="id">The ID of the Guild.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> RemoveGuild(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "DELETE FROM guilds WHERE id=@id";

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
    /// Modifies a Guild in the database.
    /// </summary>
    /// <param name="id">The ID of the Guild.</param>
    /// <param name="name">The name of the Guild.</param>
    /// <param name="tracked">Whether to track the Guild.</param>
    /// <param name="tempVcChannel">The Channel ID for creating Temporary VCs.</param>
    /// <param name="tempVcEnabled">Whether to handle Temporary VCs for the Guild.</param>
    /// <param name="tempVcDefaultMemberLimit">The default member limit for Temporary VCs.</param>
    /// <param name="tempVcDefaultBitrate">The default bitrate for Temporary VCs.</param>
    /// <param name="robloxAlertChannel">The channel ID for roblox alerts.</param>
    /// <param name="robloxAlertEnabled">Whether to handle roblox alerts.</param>
    /// <param name="robloxAlertInterval">The interval for each roblox alert.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyGuild(ulong id, string? name = null, ulong? tempVcChannel = null,
        bool? tempVcEnabled = null, int? tempVcDefaultMemberLimit = null, int? tempVcDefaultBitrate = null,
        ulong? robloxAlertChannel = null, bool? robloxAlertEnabled = null, int? robloxAlertInterval = null)
    {
        if (name == null && tempVcChannel == null && tempVcDefaultMemberLimit == null && tempVcDefaultBitrate == null &&
            robloxAlertChannel == null && robloxAlertInterval == null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE guilds SET";

        if (name != null)
        {
            query += " name=@name,";
            command.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Text) { Value = name });
        }

        if (tempVcChannel is not null)
        {
            query += " temp_vc_channel=@tempVcChannel,";
            command.Parameters.Add(tempVcChannel is 0 
                ? new NpgsqlParameter("tempVcChannel", NpgsqlDbType.Numeric) { Value = DBNull.Value } 
                : new NpgsqlParameter("tempVcChannel", NpgsqlDbType.Numeric) { Value = (long)tempVcChannel });
        }

        if (tempVcEnabled is not null)
        {
            query += " temp_vc_enabled=@tempVcEnabled";
            command.Parameters.Add(new NpgsqlParameter("tempVcEnabled", NpgsqlDbType.Boolean)
                { Value = tempVcEnabled });
        }

        if (tempVcDefaultMemberLimit != null)
        {
            query += " temp_vc_default_member_limit=@tempVcDefaultMemberLimit,";
            command.Parameters.Add(new NpgsqlParameter("tempVcDefaultMemberLimit", NpgsqlDbType.Integer)
                { Value = tempVcDefaultMemberLimit });
        }

        if (tempVcDefaultBitrate != null)
        {
            query += " temp_vc_default_bitrate=@tempVcDefaultBitrate,";
            command.Parameters.Add(new NpgsqlParameter("tempVcDefaultBitrate", NpgsqlDbType.Integer)
                { Value = tempVcDefaultBitrate });
        }

        if (robloxAlertChannel is not null)
        {
            query += " roblox_alert_channel=@robloxAlertChannel,";
            command.Parameters.Add(robloxAlertChannel is 0
                ? new NpgsqlParameter("robloxAlertChannel", NpgsqlDbType.Numeric) { Value = DBNull.Value }
                : new NpgsqlParameter("robloxAlertChannel", NpgsqlDbType.Numeric) { Value = (long)robloxAlertChannel });
        }
        
        if (robloxAlertEnabled is not null)
        {
            query += " roblox_alert_enabled=@robloxAlertEnabled,";
            command.Parameters.Add(new NpgsqlParameter("robloxAlertEnabled", NpgsqlDbType.Boolean)
                { Value = robloxAlertEnabled });
        }

        if (robloxAlertInterval != null)
        {
            query += " roblox_alert_interval=@robloxAlertInterval";
            command.Parameters.Add(new NpgsqlParameter("robloxAlertInterval", NpgsqlDbType.Integer)
                { Value = robloxAlertInterval });
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
    /// Gets a Guild from the database.
    /// </summary>
    /// <param name="id">The ID of the Guild.</param>
    /// <returns>The Guild.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the Guild doesn't exist.</exception>
    public static async Task<GuildRow> GetGuild(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM guilds WHERE id=@id";

        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });
        
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ulong? tempVcChannel = null;
            try
            {
                tempVcChannel = (ulong)reader.GetInt64(reader.GetOrdinal("temp_vc_channel"));
            }
            catch (Exception)
            {
                // ignore
            }
            ulong? robloxAlertChannel = null;
            try
            {
                robloxAlertChannel = (ulong)reader.GetInt64(reader.GetOrdinal("roblox_alert_channel"));
            }
            catch (Exception)
            {
                // ignore
            }

            return new GuildRow
            {
                Id = (ulong)reader.GetInt64(reader.GetOrdinal("id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                TempVcChannel = tempVcChannel,
                TempVcEnabled = reader.GetBoolean(reader.GetOrdinal("temp_vc_enabled")),
                TempVcDefaultMemberLimit = reader.GetInt16(reader.GetOrdinal("temp_vc_default_member_limit")),
                TempVcDefaultBitrate = reader.GetInt32(reader.GetOrdinal("temp_vc_default_bitrate")),
                RobloxAlertChannel = robloxAlertChannel,
                RobloxAlertEnabled = reader.GetBoolean(reader.GetOrdinal("roblox_alert_enabled")),
                RobloxAlertInterval = reader.GetInt32(reader.GetOrdinal("roblox_alert_interval"))
            };
        }

        throw new KeyNotFoundException($"No Guild exists with ID: {id}");
    }
    
    /// <summary>
    /// Checks if a Guild exists in the database.
    /// </summary>
    /// <param name="id">The ID of the Guild.</param>
    /// <returns>Whether the Guild exists.</returns>
    public static async Task<bool> GuildExists(ulong id)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM guilds WHERE id=@id";
        
        command.CommandText = query;
        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Numeric) { Value = (long)id });

        return command.ExecuteScalar() is not null;
    }
}