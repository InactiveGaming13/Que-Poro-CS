using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public static class Config
{
    /// <summary>
    /// Adds the bots Config to the database.
    /// </summary>
    /// <param name="statusType">The Status type.</param>
    /// <param name="statusMessage">The Status message.</param>
    /// <param name="logChannel">The log Channel.</param>
    /// <param name="tempVcEnabled">Whether the bot handles Temporary VCs.</param>
    /// <param name="tempVcDefaultMemberLimit">The default Temporary VC member limit.</param>
    /// <param name="tempVcDefaultBitrate">The default Temporary VC bitrate.</param>
    /// <param name="robloxAlertsEnabled">Whether the bot handles Roblox status alerts.</param>
    /// <param name="repliesEnabled">Whether the bot handles replies to users.</param>
    /// <param name="testersEnabled">Whether the Tester commands are locked to the testing server.</param>
    /// <param name="shutdownChannel">The Channel ID of the shutdown command.</param>
    /// <param name="shutdownMessage">The Message ID of the shutdown command.</param>
    /// <returns></returns>
    public static async Task<bool> AddConfig(short statusType = 0, string statusMessage = "", ulong logChannel = 0,
        bool tempVcEnabled = true, int tempVcDefaultMemberLimit = 5, int tempVcDefaultBitrate = 64,
        bool robloxAlertsEnabled = true, bool repliesEnabled = true, bool testersEnabled = false,
        ulong shutdownChannel = 0, ulong shutdownMessage = 0)
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();

        const string query = 
            "INSERT INTO config (status_type, created_at, status_message, log_channel, temp_vc_enabled, "+
            "temp_vc_default_member_limit, temp_vc_default_bitrate, roblox_alerts_enabled, replies_enabled, "+
            "testers_enabled) VALUES (@statusType, CURRENT_TIMESTAMP, @statusMessage, " +
            "@logChannel, @tempVcEnabled, @tempVcDefaultMemberLimit, @tempVcDefaultBitrate, @robloxAlertsEnabled, " +
            "@repliesEnabled, @testersEnabled)";
        
        command.Parameters.Add(new NpgsqlParameter("statusType", NpgsqlDbType.Integer) { Value = statusType });
        command.Parameters.Add(new NpgsqlParameter("statusMessage", NpgsqlDbType.Text) { Value = statusMessage });
        command.Parameters.Add(new NpgsqlParameter("logChannel", NpgsqlDbType.Numeric) { Value = (long)logChannel });
        command.Parameters.Add(new NpgsqlParameter("tempVcEnabled", NpgsqlDbType.Boolean) { Value = tempVcEnabled });
        command.Parameters.Add(new NpgsqlParameter("tempVcDefaultMemberLimit", NpgsqlDbType.Integer) { Value = tempVcDefaultMemberLimit });
        command.Parameters.Add(new NpgsqlParameter("tempVcDefaultBitrate", NpgsqlDbType.Integer) { Value = tempVcDefaultBitrate });
        command.Parameters.Add(new NpgsqlParameter("robloxAlertsEnabled", NpgsqlDbType.Boolean) { Value = robloxAlertsEnabled });
        command.Parameters.Add(new NpgsqlParameter("repliesEnabled", NpgsqlDbType.Boolean) { Value = repliesEnabled });
        command.Parameters.Add(new NpgsqlParameter("testersEnabled", NpgsqlDbType.Boolean) { Value = testersEnabled });
        command.Parameters.Add(new NpgsqlParameter("shutdownChannel", NpgsqlDbType.Numeric) { Value = (long)shutdownChannel });
        command.Parameters.Add(new NpgsqlParameter("shutdownMessage", NpgsqlDbType.Numeric) { Value = (long)shutdownMessage });

        command.CommandText = query;
        
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
    /// Gets the Config from the database.
    /// </summary>
    /// <returns>The Config.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the Config doesn't exist.</exception>
    public static async Task<ConfigRow> GetConfig()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM config";

        command.CommandText = query;

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            return new ConfigRow
            {
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                LastModified = reader.GetDateTime(reader.GetOrdinal("last_modified")),
                StatusType = reader.GetInt16(reader.GetOrdinal("status_type")),
                StatusMessage = reader.GetString(reader.GetOrdinal("status_message")),
                LogChannel = (ulong)reader.GetInt64(reader.GetOrdinal("log_channel")),
                TempVcEnabled = reader.GetBoolean(reader.GetOrdinal("temp_vc_enabled")),
                TempVcDefaultMemberLimit = reader.GetInt16(reader.GetOrdinal("temp_vc_default_member_limit")),
                TempVcDefaultBitrate = reader.GetInt32(reader.GetOrdinal("temp_vc_default_bitrate")),
                RobloxAlertsEnabled = reader.GetBoolean(reader.GetOrdinal("roblox_alerts_enabled")),
                RepliesEnabled = reader.GetBoolean(reader.GetOrdinal("replies_enabled")),
                TestersEnabled = reader.GetBoolean(reader.GetOrdinal("testers_enabled"))
            };
        }

        throw new KeyNotFoundException("Config doesn't exist.");
    }
    
    /// <summary>
    /// Modifies the Config in the database.
    /// </summary>
    /// <param name="statusType">The startup Status type.</param>
    /// <param name="statusMessage">The startup Status message.</param>
    /// <param name="logChannel">The Channel to log errors.</param>
    /// <param name="tempVcEnabled">Whether the bot handles Temporary VCs.</param>
    /// <param name="tempVcDefaultMemberLimit">The Temporary VC default member limit.</param>
    /// <param name="tempVcDefaultBitrate">The Temporary VC default bitrate.</param>
    /// <param name="robloxAlertsEnabled">Whether the bot handles roblox status alerts.</param>
    /// <param name="repliesEnabled">Whether the bot handles replies to users.</param>
    /// <param name="testersEnabled">Whether the Tester commands are locked to the testing server.</param>
    /// <returns>Whether the operation succeeds.</returns>
    public static async Task<bool> ModifyConfig(short? statusType = null, string? statusMessage = null, ulong? logChannel = null,
        bool? tempVcEnabled = null, int? tempVcDefaultMemberLimit = null, int? tempVcDefaultBitrate = null,
        bool? robloxAlertsEnabled = null, bool? repliesEnabled = null, bool? testersEnabled = null)
    {
        if (statusType == null && statusMessage == null && logChannel == null && tempVcEnabled == null &&
            tempVcDefaultMemberLimit == null && tempVcDefaultBitrate == null && robloxAlertsEnabled == null &&
            repliesEnabled == null && testersEnabled == null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE config SET";

        if (statusType != null)
        {
            query += " status_type=@statusType,";
            command.Parameters.Add(new NpgsqlParameter("statusType", NpgsqlDbType.Integer)
                { Value = statusType });
        }

        if (statusMessage != null)
        {
            query += " status_message=@statusMessage,";
            command.Parameters.Add(new NpgsqlParameter("statusMessage", NpgsqlDbType.Text)
                { Value = statusMessage });
        }

        if (tempVcEnabled != null)
        {
            query += " temp_vc_enabled=@tempVcEnabled,";
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
        
        if (robloxAlertsEnabled != null)
        {
            query += " roblox_alerts_enabled=@robloxAlertsEnabled,";
            command.Parameters.Add(new NpgsqlParameter("robloxAlertsEnabled", NpgsqlDbType.Boolean)
                { Value = robloxAlertsEnabled });
        }

        if (repliesEnabled != null)
        {
            query += " replies_enabled=@repliesEnabled,";
            command.Parameters.Add(new NpgsqlParameter("repliesEnabled", NpgsqlDbType.Boolean)
                { Value = repliesEnabled });
        }
        
        if (testersEnabled != null)
        {
            query += " testers_enabled=@testersEnabled";
            command.Parameters.Add(new NpgsqlParameter("testersEnabled", NpgsqlDbType.Boolean)
                { Value = testersEnabled });
        }

        if (query.EndsWith(','))
            query = query.Remove(query.Length - 1);

        command.CommandText = query;
            
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
    /// Checks if the Config exists in the database.
    /// </summary>
    /// <returns>Whether the Config exists.</returns>
    public static async Task<bool> ConfigExists()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM config";
        
        command.CommandText = query;

        return command.ExecuteScalar() is not null;
    }
}