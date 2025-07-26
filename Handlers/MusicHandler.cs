using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling the music commands.
/// </summary>
[SlashCommandGroup("music", "Music commands")]
public class MusicCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Plays a song in a Voice Channel.
    /// </summary>
    /// <param name="e">The Interaction Context.</param>
    /// <param name="query">The search term or link.</param>
    /// <param name="force">Whether to force the query to the top of the queue.</param>
    [SlashCommand("play", "Plays a song")]
    public static async Task PlayTrack(InteractionContext e,
        [Option("song", "Song title to play")] string query,
        [Option("force", "Force play and override the queue")]
        bool force = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        // Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (e.Member.VoiceState is { Channel: null } or null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not in a vc."));
            return;
        }

        if (!VoiceHandler.UsingLavaLink)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I am not configured for voice at the moment."));
            return;
        }

        if (!await VoiceHandler.ConnectToLavaLink(e.Client, e.Guild))
        {
            DiscordUser owner =
                await e.Client.GetUserAsync(Convert.ToUInt64(Environment.GetEnvironmentVariable("BOT_OWNER_ID")));
            await owner.SendMessageAsync("LavaLink has failed to connect (play command)!");
            
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am unable to connect. I have reported this to my owner."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow user = await Users.GetUser(e.UserId);

        if (!await CheckGuildPlayer(VoiceHandler.Lavalink.GetGuildPlayer(e.Guild), e.Guild, e.Member.VoiceState.Channel))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I am not configured for voice at the moment."));
            return;
        }
        
        LavalinkGuildPlayer guildPlayer = VoiceHandler.Lavalink.GetGuildPlayer(e.Guild)!;
        
        if (guildPlayer.Channel != e.Channel && (!user.Admin || 
            !e.Member.Permissions.HasPermission(Permissions.MoveMembers)))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not in my vc and you lack permission to move me."));
            return;
        }

        LavalinkSearchType searchType = query.Contains("https://", StringComparison.CurrentCultureIgnoreCase) ? LavalinkSearchType.Plain : LavalinkSearchType.Youtube;

        await MusicHandler.GetResult(e, guildPlayer, query, searchType, force);
    }

    [SlashCommand("pause", "Pause the current song")]
    public static async Task PauseTrack(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

        if (guildPlayer == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != e.Channel && !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        if (guildPlayer.Player.Paused)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The track is already paused."));
            return;
        }

        await guildPlayer.PauseAsync();
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Paused [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}."));
    }
    
    [SlashCommand("resume", "Resumes the current song")]
    public static async Task ResumeTrack(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

        if (guildPlayer == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != e.Channel && !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        await guildPlayer.ResumeAsync();
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Resumed [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}."));
    }
    
    [SlashCommand("stop", "Stops the current song")]
    public static async Task StopTrack(InteractionContext e,
        [Option("clear_queue", "Clears the queue")] bool clear = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

        if (guildPlayer == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != e.Channel && !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }

        if (!guildPlayer.Queue.Any() && guildPlayer.CurrentTrack == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There is nothing to stop."));
        }

        string response = "Stopped the current song";

        if (clear)
        {
            guildPlayer.ClearQueue();
            response += " and cleared the queue";
        }
        
        await guildPlayer.StopAsync();
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
    }
    
    [SlashCommand("skip", "skips the current song")]
    public static async Task SkipTrack(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);
        
        if (guildPlayer == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != e.Channel && !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing is currently playing."));
            return;
        }

        LavalinkTrack nextTrack = guildPlayer.Queue.First();
        await guildPlayer.SkipAsync();
        await e.EditResponseAsync($"Skipped to [{nextTrack.Info.Title}]({nextTrack.Info.Uri}) by {nextTrack.Info.Author}.");
    }
    
    [SlashCommand("currently_playing", "Gets the current song")]
    public static async Task CurrentlyPlaying(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

        if (guildPlayer == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing is currently playing."));
            return;
        }

        if (!guildPlayer.Player.Paused)
        {
            await e.EditResponseAsync($"Currently playing [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
            return;
        }
        
        await e.EditResponseAsync($"Currently paused [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
    }

    [SlashCommandGroup("queue", "The queue commands")]
    public class MusicQueueCommands : ApplicationCommandsModule
    {
        [SlashCommand("list", "Lists the current queue")]
        public static async Task GetQueue(InteractionContext e)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }

            LavalinkExtension lavalink = e.Client.GetLavalink();
            LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

            if (guildPlayer == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }

            string title = guildPlayer.Queue.Count == 0
                ? "The queue is empty"
                : $"The current queue for {e.Guild.Name}";

            string queue = guildPlayer.Queue.Aggregate("", (current, track) =>
            {
                string nextTrack = $"{track.Info.Title} by {track.Info.Author}\n";

                return (current.Length + nextTrack.Length) switch
                {
                    > 4096 when current.Length + "...".Length < 4096 => current + "...",
                    > 4096 when current.Length + "...".Length > 4096 => string.Empty,
                    _ => current + nextTrack
                };
            });

            if (queue.Length > 4096)
                queue = queue.Remove(4096);

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = title,
                Description = queue
            }.Build();
            await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }

        [SlashCommand("clear", "Clears the current queue")]
        public static async Task ClearQueue(InteractionContext e)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }

            LavalinkExtension lavalink = e.Client.GetLavalink();
            LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

            if (guildPlayer == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            guildPlayer.ClearQueue();
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Cleared the queue for {e.Guild.Name}"));
        }

        [SlashCommand("shuffle", "Shuffles the current queue")]
        public static async Task ShuffleQueue(InteractionContext e)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }

            LavalinkExtension lavalink = e.Client.GetLavalink();
            LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

            if (guildPlayer == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            guildPlayer.ShuffleQueue();
            string title = guildPlayer.Queue.Count == 0
                ? "The queue is empty"
                : $"The current queue for {e.Guild.Name} (shuffled)";

            string queue = guildPlayer.Queue.Aggregate("", (current, track) => current + $"{track.Info.Title} by {track.Info.Author}\n");

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = title,
                Description = queue
            }.Build();
            await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }
    }
    
    [SlashCommandGroup("volume", "The volume control commands")]
    public class MusicVolumeCommands : ApplicationCommandsModule
    {
        [SlashCommand("get", "Gets the volume of the bot")]
        public static async Task GetVolume(InteractionContext e)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
    
            LavalinkExtension lavalink = e.Client.GetLavalink();
            LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);
    
            if (guildPlayer == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
    
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"The current volume is {guildPlayer.Player.Volume}"));
        }
    
        [SlashCommand("set", "Sets the volume of the bot")]
        public static async Task SetVolume(InteractionContext e,
            [Option("volume", "The volume to set the bot to"),
             MinimumValue(0), MaximumValue(200)] int volume)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
    
            LavalinkExtension lavalink = e.Client.GetLavalink();
            LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);
    
            if (guildPlayer == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            if (guildPlayer.Channel != e.Channel && !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
            {
                await e.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
                return;
            }
    
            await guildPlayer.SetVolumeAsync(volume);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Set the volume to {volume}."));
        }
    }
    
    public class LavalinkQueueEntry : IQueueEntry
    {
        public LavalinkTrack? Track { get; set; }
        
        public async Task<bool> BeforePlayingAsync(LavalinkGuildPlayer guildPlayer)
        {
            Track ??= guildPlayer.Queue[0];
            
            await guildPlayer.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(
                $"Now playing BEFORE [{Track.Info.Title}]({Track.Info.Uri}) by {Track.Info.Author}."));
            return true;
        }
        
        public async Task AfterPlayingAsync(LavalinkGuildPlayer guildPlayer)
        {
            if (guildPlayer.Queue.Count is 0)
                return;
            
            Track = guildPlayer.Queue[0];
            await guildPlayer.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(
                $"Now playing AFTER [{Track.Info.Title}]({Track.Info.Uri}) by {Track.Info.Author}."));
        }
    }

    private static class MusicHandler
    {
        public static async Task GetResult(InteractionContext e, LavalinkGuildPlayer guildPlayer, string title, LavalinkSearchType searchType = LavalinkSearchType.Plain, bool force = false)
        {
            LavalinkTrackLoadingResult loadResult = await guildPlayer.LoadTracksAsync(searchType, title);

            if (loadResult.LoadType is LavalinkLoadResultType.Empty or LavalinkLoadResultType.Error)
            {
                if (searchType is not LavalinkSearchType.Youtube)
                {
                    await GetResult(e, guildPlayer, title, LavalinkSearchType.Youtube, force);
                    return;
                }
                
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to search for {title}."));
                return;
            }

            LavalinkTrack? searchTrack = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => null
            };

            LavalinkPlaylist? playlist = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>(),
                _ => null
            };

            if (playlist is { Tracks.Count: 0 } or null && searchTrack is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Failed to find {title}"));
                return;
            }
            
            switch (playlist)
            {
                case null when !force && guildPlayer.CurrentTrack is null:
                    await guildPlayer.PlayAsync(searchTrack);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Now playing [{searchTrack.Info.Title}]({searchTrack.Info.Uri}) by {searchTrack.Info.Author}."));
                    break;
                
                case null when !force && guildPlayer.CurrentTrack is not null:
                    AddTrackToQueue(guildPlayer, searchTrack);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Added [{searchTrack.Info.Title}]({searchTrack.Info.Uri}) by {searchTrack.Info.Author} to the queue."));
                    break;
                
                case null when force && e.Member.Permissions.HasPermission(Permissions.Administrator):
                    ForceAddTrackToQueue(guildPlayer, searchTrack);
                    await guildPlayer.SkipAsync();
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Force playing [{searchTrack.Info.Title}]({searchTrack.Info.Uri}) by {searchTrack.Info.Author}."));
                    break;
                    
                case { Tracks.Count: 1 } when !force && guildPlayer.CurrentTrack is null:
                    await guildPlayer.PlayAsync(playlist.Tracks.First());
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Now playing [{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by {playlist.Tracks.First().Info.Author}."));
                    break;
                
                case { Tracks.Count: 1 } when !force && guildPlayer.CurrentTrack is not null:
                    AddTrackToQueue(guildPlayer, playlist.Tracks.First());
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Added [{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by {playlist.Tracks.First().Info.Author} to the queue."));
                    break;
                
                case { Tracks.Count: 1 } when force && e.Member.Permissions.HasPermission(Permissions.Administrator):
                    ForceAddTrackToQueue(guildPlayer, playlist.Tracks.First());
                    await guildPlayer.SkipAsync();
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Force playing [{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by {playlist.Tracks.First().Info.Author}."));
                    break;
                
                case { Tracks.Count: > 1 } when !force && guildPlayer.CurrentTrack is null:
                    AddPlaylistToQueue(guildPlayer, playlist);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "Added playlist to queue."));
                    guildPlayer.PlayQueue();
                    await e.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(
                        $"Now playing [{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by {playlist.Tracks.First().Info.Author}."));
                    break;
                
                case { Tracks.Count: > 1 } when !force && guildPlayer.CurrentTrack is not null:
                    AddPlaylistToQueue(guildPlayer, playlist);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "Added playlist to queue."));
                    break;
                
                case { Tracks.Count: > 1 } when force && e.Member.Permissions.HasPermission(Permissions.Administrator):
                    ForceAddPlaylistToQueue(guildPlayer, playlist);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "Force added playlist to queue."));
                    guildPlayer.PlayQueue();
                    await e.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(
                        $"Force playing [{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by {playlist.Tracks.First().Info.Author}."));
                    break;
                
                default:
                    AddPlaylistToQueue(guildPlayer, playlist);
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Added {(playlist.Tracks.Count is 1 ? $"[{playlist.Tracks.First().Info.Title}]({playlist.Tracks.First().Info.Uri}) by " +
                                                               $"{playlist.Tracks.First().Info.Author}" : "playlist")} to the queue " +
                        $"(You lack the permission to force play tracks)."));
                    break;
            }
        }
    }

    private static void AddPlaylistToQueue(LavalinkGuildPlayer guildPlayer, LavalinkPlaylist playlist)
    {
        switch (playlist.Tracks)
        {
            case { Count: 0 }:
                return;
            
            case { Count: 1 }:
                AddTrackToQueue(guildPlayer, playlist.Tracks[0]);
                return;
        }
        
        guildPlayer.AddToQueue(playlist);
    }
        
    private static void ForceAddPlaylistToQueue(LavalinkGuildPlayer guildPlayer, LavalinkPlaylist playlist)
    {
        switch (playlist.Tracks)
        {
            case { Count: 0 }:
                return;
            
            case { Count: 1 }:
                ForceAddTrackToQueue(guildPlayer, playlist.Tracks[0]);
                return;
        }
        
        for (int i = 0; i < playlist.Tracks.Count -1; i++)
            guildPlayer.AddToQueueAt(i, playlist.Tracks[i]);
    }
        
    private static void AddTrackToQueue(LavalinkGuildPlayer guildPlayer, LavalinkTrack track)
    {
        guildPlayer.AddToQueue(track);
    }
        
    private static void ForceAddTrackToQueue(LavalinkGuildPlayer guildPlayer, LavalinkTrack track)
    {
        guildPlayer.AddToQueueAt(0, track);
    }

    private static async Task<bool> CheckGuildPlayer(LavalinkGuildPlayer? guildPlayer, DiscordGuild guild,
        DiscordChannel channel)
    {
        if (guildPlayer is not null)
            return true;

        if (!VoiceHandler.UsingLavaLink || VoiceHandler.Lavalink is null)
            return false;

        LavalinkSession session = VoiceHandler.Lavalink.ConnectedSessions.Values.First();

        await session.ConnectAsync(channel);

        return VoiceHandler.Lavalink.GetGuildPlayer(guild) is not null;
    }
}