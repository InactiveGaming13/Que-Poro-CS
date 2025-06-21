using Npgsql;

namespace QuePoro.Database.Handlers;

using Types;

public static class BannedPhrases
    {
        private static readonly NpgsqlDataSource DataSource = Database.GetDataSource();

        public static async Task AddPhrase(ulong creatorId, int severity, string phrase, bool enabled = true,
            ulong? guildId = null)
        {
            const string query =
                "INSERT INTO banned_phrases (created_by, guild_id, severity, phrase, enabled) VALUES ($1, $2, $3, $4, $5)";
            await using var cmd = DataSource.CreateCommand(query);
            cmd.Parameters.AddWithValue(creatorId);
            cmd.Parameters.AddWithValue(guildId);
            cmd.Parameters.AddWithValue(severity);
            cmd.Parameters.AddWithValue(phrase);
            cmd.Parameters.AddWithValue(enabled);
            
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
                
                if (e.Message.Contains("banned_phrases_phrase_key", StringComparison.CurrentCultureIgnoreCase))
                    Console.WriteLine("Phrase already exists!");
            }
        }
        
        public static async Task RemovePhrase(Guid id)
        {
            const string query = "DELETE FROM banned_phrases WHERE id=$1";
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

        public static async Task ModifyPhrase(Guid id, ulong? guildId = null, int? severity = null,
            string? phrase = null, bool? enabled = null)
        {
            if (guildId == null && severity == null && phrase == null && enabled == null)
                return;
            
            string query = "UPDATE banned_phrases SET";

            if (guildId != null)
                query += $" guild_id={guildId},";

            if (severity != null)
                query += $" severity={severity},";

            if (phrase != null)
                query += $" phrase='{phrase}',";

            if (enabled != null)
                query += $" enabled={enabled}";

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

        public static async Task<Guid?> GetPhraseId(string phrase)
        {
            string query = "SELECT * FROM banned_phrases WHERE phrase LIKE $1";
            
            await using NpgsqlCommand command = DataSource.CreateCommand(query);
            command.Parameters.AddWithValue(phrase);
            
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                return reader.GetGuid(reader.GetOrdinal("id"));
            }

            return null;
        }

        public static async Task<BannedPhraseRow?> GetPhrase(Guid id)
        {
            string query = "SELECT * FROM banned_phrases WHERE id=$1";

            await using NpgsqlCommand command = DataSource.CreateCommand(query);
            command.Parameters.AddWithValue(id);
            
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Guid phraseId = reader.GetGuid(reader.GetOrdinal("id"));
                DateTime createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
                long createdBy = reader.GetInt64(reader.GetOrdinal("created_by"));
                int severity = reader.GetInt32(reader.GetOrdinal("severity"));
                string bannedPhrase = reader.GetString(reader.GetOrdinal("phrase"));
                bool enabled = reader.GetBoolean(reader.GetOrdinal("enabled"));
                
                return new BannedPhraseRow
                {
                    Id = phraseId,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Severity = severity,
                    Phrase = bannedPhrase,
                    Enabled = enabled
                };
            }

            return null;
        }
    }