using Npgsql;

namespace QuePoro.Database.Handlers;

public class Guilds
{
    private static readonly NpgsqlDataSource DataSource = Database.GetDataSource();
    
    public static async Task AddGuild(ulong id, string name, ulong? tempVcChannel = null,
        int? tempVcMemberDefault = null, int? tempVcBitrateDefault = null, ulong? robloxAlertChannel = null,
        int? robloxAlertInterval = null)
    {
        const string query =
            "INSERT INTO guilds (id, name, temp_vc_channel, temp_vc_member_default, temp_vc_bitrate_default, roblox_alert_channel, roblox_alert_interval) VALUES ($1, $2, $3, $4, $5, $6, $7)";
        await using var cmd = DataSource.CreateCommand(query);
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(name);
        cmd.Parameters.AddWithValue(tempVcChannel);
        cmd.Parameters.AddWithValue(tempVcMemberDefault);
        cmd.Parameters.AddWithValue(tempVcBitrateDefault);
        cmd.Parameters.AddWithValue(robloxAlertChannel);
        cmd.Parameters.AddWithValue(robloxAlertInterval);
            
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
    
    public static async Task RemoveGuild(ulong id)
    {
        const string query = "DELETE FROM guilds WHERE id=$1";
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

    public static async Task ModifyGuild(ulong id, string? name = null, ulong? tempVcChannel = null,
        int? tempVcMemberDefault = null, int? tempVcBitrateDefault = null, ulong? robloxAlertChannel = null,
        int? robloxAlertInterval = null)
    {
        if (name == null && tempVcChannel == null && tempVcMemberDefault == null && tempVcBitrateDefault == null &&
            robloxAlertChannel == null && robloxAlertInterval == null)
            return;
            
        string query = "UPDATE banned_phrases SET";

        if (name != null)
            query += $" name='{name}',";

        if (tempVcChannel != null)
            query += $" temp_vc_channel='{tempVcChannel}',";

        if (tempVcMemberDefault != null)
            query += $" temp_vc_member_default={tempVcMemberDefault},";

        if (tempVcBitrateDefault != null)
            query += $" temp_vc_bitrate_default={tempVcBitrateDefault},";
        
        if (robloxAlertChannel != null)
            query += $" roblox_alert_channel={robloxAlertChannel},";

        if (robloxAlertInterval != null)
            query += $" roblox_alert_interval={robloxAlertInterval}";

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
}