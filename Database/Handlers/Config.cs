using System.Data;
using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class Config
{
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
            "testers_enabled, shutdown_channel, shutdown_message) VALUES (@statusType, CURRENT_TIMESTAMP, @statusMessage, " +
            "@logChannel, @tempVcEnabled, @tempVcDefaultMemberLimit, @tempVcDefaultBitrate, @robloxAlertsEnabled, " +
            "@repliesEnabled, @testersEnabled, @shutdownChannel, @shutdownMessage)";
        
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

    public static async Task<ConfigRow> GetConfig()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT * FROM config";

        command.CommandText = query;

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            DateTime lastModified = reader.GetDateTime(reader.GetOrdinal("last_modified"));
            short statusType = reader.GetInt16(reader.GetOrdinal("status_type"));
            string statusMessage = reader.GetString(reader.GetOrdinal("status_message"));
            ulong logChannel = (ulong)reader.GetInt64(reader.GetOrdinal("log_channel"));
            bool tempVcEnabled = reader.GetBoolean(reader.GetOrdinal("temp_vc_enabled"));
            short tempVcDefaultMemberLimit = reader.GetInt16(reader.GetOrdinal("temp_vc_default_member_limit"));
            int tempVcDefaultBitrate = reader.GetInt32(reader.GetOrdinal("temp_vc_default_bitrate"));
            bool robloxAlertsEnabled = reader.GetBoolean(reader.GetOrdinal("roblox_alerts_enabled"));
            bool repliesEnabled = reader.GetBoolean(reader.GetOrdinal("replies_enabled"));
            bool testersEnabled = reader.GetBoolean(reader.GetOrdinal("testers_enabled"));
            ulong shutdownChannel = (ulong)reader.GetInt64(reader.GetOrdinal("shutdown_channel"));
            ulong shutdownMessage = (ulong)reader.GetInt64(reader.GetOrdinal("shutdown_message"));

            return new ConfigRow
            {
                CreatedAt = createdAt,
                LastModified = lastModified,
                StatusType = statusType,
                StatusMessage = statusMessage,
                LogChannel = logChannel,
                TempVcEnabled = tempVcEnabled,
                TempVcDefaultMemberLimit = tempVcDefaultMemberLimit,
                TempVcDefaultBitrate = tempVcDefaultBitrate,
                RobloxAlertsEnabled = robloxAlertsEnabled,
                RepliesEnabled = repliesEnabled,
                TestersEnabled = testersEnabled,
                ShutdownChannel = shutdownChannel,
                ShutdownMessage = shutdownMessage
            };
        }

        return null;
    }
    
    public static async Task<bool> ModifyConfig(short? statusType = null, string? statusMessage = null, ulong? logChannel = null,
        bool? tempVcEnabled = null, int? tempVcDefaultMemberLimit = null, int? tempVcDefaultBitrate = null,
        bool? robloxAlertsEnabled = null, bool? repliesEnabled = null, bool? testersEnabled = null,
        ulong? shutdownChannel = null, ulong? shutdownMessage = null)
    {
        if (statusType == null && statusMessage == null && logChannel == null && tempVcEnabled == null &&
            tempVcDefaultMemberLimit == null && tempVcDefaultBitrate == null && robloxAlertsEnabled == null &&
            repliesEnabled == null && testersEnabled == null && shutdownMessage == null)
            return false;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
            
        string query = "UPDATE config SET";

        if (statusType != null)
            query += $" status_type={statusType},";

        if (statusMessage != null)
            query += $" status_message='{statusMessage}'";

        if (tempVcEnabled != null)
            query += $" temp_vc_enabled={tempVcEnabled},";

        if (tempVcDefaultMemberLimit != null)
            query += $" temp_vc_default_member_limit={tempVcDefaultMemberLimit},";

        if (tempVcDefaultBitrate != null)
            query += $" temp_vc_default_bitrate={tempVcDefaultBitrate},";
        
        if (robloxAlertsEnabled != null)
            query += $" roblox_alerts_enabled={robloxAlertsEnabled},";

        if (repliesEnabled != null)
            query += $" replies_enabled={repliesEnabled},";
        
        if (testersEnabled != null)
            query += $" testers_enabled={testersEnabled},";
        
        if (shutdownChannel != null)
            query += $" shutdown_channel={shutdownChannel},";
        
        if (shutdownMessage != null)
            query += $" shutdown_message={shutdownMessage}";

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
    
    public static async Task<bool> ConfigExists()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using NpgsqlCommand command = connection.CreateCommand();
        
        const string query = "SELECT created_at FROM config";
        
        command.CommandText = query;

        return command.ExecuteScalar() is not null;
    }
}