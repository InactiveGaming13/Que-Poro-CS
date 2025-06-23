using System.Data;
using Npgsql;
using NpgsqlTypes;
using QuePoro.Database.Types;

namespace QuePoro.Database.Handlers;

public class Config
{
    public static async Task AddConfig(short? statusType = null, string? statusMessage = null, ulong? logChannel = null,
        bool? tempVcEnabled = null, int? tempVcDefaultMemberLimit = null, int? tempVcDefaultBitrate = null,
        bool? robloxAlertsEnabled = null, bool? repliesEnabled = null, bool? testersEnabled = null,
        ulong? shutdownChannel = null, ulong? shutdownMessage = null)
    {
        if (statusType == null && statusMessage == null && logChannel == null && tempVcEnabled == null &&
            tempVcDefaultMemberLimit == null && tempVcDefaultBitrate == null && robloxAlertsEnabled == null &&
            repliesEnabled == null && testersEnabled == null && shutdownMessage == null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();

        string query = "INSERT INTO config (last_modified";
        string data = "VALUES (CURRENT_TIMESTAMP";
        
        if (statusType != null)
        {
            query += ", status_type";
            data += ", @statusType";
            command.Parameters.Add(new NpgsqlParameter("statusType", DbType.Int16) { Value = statusType });
        }

        if (statusMessage != null)
        {
            query += ", status_message";
            data += ", @statusMessage";
            command.Parameters.Add(new NpgsqlParameter("statusMessage", NpgsqlDbType.Text) { Value = statusMessage });
        }
        
        if (logChannel != null)
        {
            query += ", log_channel";
            data += ", @logChannel";
            command.Parameters.Add(new NpgsqlParameter("logChannel", NpgsqlDbType.Numeric) { Value = logChannel });
        }

        if (tempVcEnabled != null)
        {
            query += ", temp_vc_enabled";
            data += ", @tempVcEnabled";
            command.Parameters.Add(new NpgsqlParameter("tempVcEnabled", NpgsqlDbType.Boolean) { Value = tempVcEnabled });
        }
        
        if (tempVcDefaultMemberLimit != null)
        {
            query += ", temp_vc_default_member_limit";
            data += ", @tempVcDefaultMemberLimit";
            command.Parameters.Add(new NpgsqlParameter("tempVcDefaultMemberLimit", DbType.Int16) { Value = tempVcDefaultMemberLimit });
        }

        if (tempVcDefaultBitrate != null)
        {
            query += ", temp_vc_default_bitrate";
            data += ", @tempVcDefaultBitrate";
            command.Parameters.Add(new NpgsqlParameter("tempVcDefaultBitrate", DbType.Int32) { Value = tempVcDefaultBitrate });
        }
        
        if (robloxAlertsEnabled != null)
        {
            query += ", roblox_alerts_enabled";
            data += ", @robloxAlertsEnabled";
            command.Parameters.Add(new NpgsqlParameter("robloxAlertsEnabled", NpgsqlDbType.Boolean) { Value = robloxAlertsEnabled });
        }

        if (repliesEnabled != null)
        {
            query += ", replies_enabled";
            data += ", @repliesEnabled";
            command.Parameters.Add(new NpgsqlParameter("repliesEnabled", NpgsqlDbType.Boolean) { Value = repliesEnabled });
        }
        
        if (testersEnabled != null)
        {
            query += ", testers_enabled";
            data += ", @testersEnabled";
            command.Parameters.Add(new NpgsqlParameter("testersEnabled", NpgsqlDbType.Boolean) { Value = testersEnabled });
        }
        
        if (shutdownChannel != null)
        {
            query += ", shutdown_channel";
            data += ", @shutdownChannel";
            command.Parameters.Add(new NpgsqlParameter("shutdownChannel", NpgsqlDbType.Numeric) { Value = shutdownChannel });
        }
        
        if (shutdownMessage != null)
        {
            query += ", shutdown_message";
            data += ", @shutdownMessage";
            command.Parameters.Add(new NpgsqlParameter("shutdownMessage", NpgsqlDbType.Numeric) { Value = shutdownMessage });
        }

        data += ")";
        query += $") {data}";

        command.CommandText = query;
        
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259)
            {
                Console.WriteLine("Unexpected Postgres Error");
            }
            
            Console.WriteLine($"{e.ErrorCode} | {e.Message}");
        }
    }

    public static async Task<ConfigRow?> GetConfig()
    {
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
        
        string query = "SELECT * FROM config";

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
    
    public static async Task ModifyConfig(short? statusType = null, string? statusMessage = null, ulong? logChannel = null,
        bool? tempVcEnabled = null, int? tempVcDefaultMemberLimit = null, int? tempVcDefaultBitrate = null,
        bool? robloxAlertsEnabled = null, bool? repliesEnabled = null, bool? testersEnabled = null,
        ulong? shutdownChannel = null, ulong? shutdownMessage = null)
    {
        if (statusType == null && statusMessage == null && logChannel == null && tempVcEnabled == null &&
            tempVcDefaultMemberLimit == null && tempVcDefaultBitrate == null && robloxAlertsEnabled == null &&
            repliesEnabled == null && testersEnabled == null && shutdownMessage == null)
            return;
        
        await using NpgsqlConnection connection = await Database.GetConnection();
        await using var command = connection.CreateCommand();
            
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
        }
        catch (PostgresException e)
        {
            if (e.ErrorCode != -2147467259)
            {
                Console.WriteLine("Unexpected Postgres Error");
            }
            
            Console.WriteLine($"{e.ErrorCode} | {e.Message}");
        }
    }
}