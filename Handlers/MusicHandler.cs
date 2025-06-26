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
    [SlashCommand("play", "Plays a song")]
    public async Task PlayTrack(InteractionContext e,
        [Option("song", "Song title to play")] string query,
        [Option("force", "Force play and override the queue")]
        bool force = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        // Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (e.Member.VoiceState.Channel is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not in a vc."));
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

        LavalinkGuildPlayer? guildPlayer = VoiceHandler.Lavalink.GetGuildPlayer(e.Guild);
        
        if (guildPlayer is null)
        {
            LavalinkSession session = VoiceHandler.Lavalink.ConnectedSessions.Values.First();
            await session.ConnectAsync(e.Member.VoiceState.Channel);
            guildPlayer = VoiceHandler.Lavalink.GetGuildPlayer(e.Guild);
        }
        
        if (guildPlayer is not null && guildPlayer.Channel != e.Channel && !user.Admin &&
            !e.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not in my vc and you lack permission to move me."));
            return;
        }
        
        LavalinkTrackLoadingResult loadResult;

        await GetResult(query);
        return;

        async Task GetResult(string title, bool useYt = false)
        {
            loadResult = useYt switch
            {
                true => await guildPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, title),
                false => await guildPlayer.LoadTracksAsync(LavalinkSearchType.Plain, title)
            };
            
            if (loadResult.LoadType is LavalinkLoadResultType.Empty or LavalinkLoadResultType.Error)
            {
                if (!useYt)
                {
                    await GetResult(title, true);
                    return;
                }
                
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Failed to search for {title}."));
            }
            
            LavalinkTrack? track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => null
            };
            
            List<LavalinkTrack>? queue = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks,
                _ => null
            };

            if (track is null && queue is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Failed to find {title}"));
            }

            if (guildPlayer.CurrentTrack != null)
            {
                string embedTitle = "Added playlist to queue";
                switch (force)
                {
                    case true when e.Member.Permissions.HasPermission(Permissions.Administrator) && track != null:
                        await guildPlayer.PlayAsync(track);
                        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                            $"Force played [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author}."));
                        return;
                    
                    case true when e.Member.Permissions.HasPermission(Permissions.Administrator) && queue != null:
                        embedTitle = "Force added playlist to queue";
                        break;
                    
                    case false when !e.Member.Permissions.HasPermission(Permissions.Administrator) && track != null:
                        guildPlayer.AddToQueue(track);
                        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                            $"Added [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author} to the queue."));
                        break;
                }
                
                string? tracksAdded = AddPlaylistToQueue();

                if (queue is { Count: >= 2 })
                {
                    DiscordEmbed embed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Blue,
                        Title = embedTitle,
                        Description = tracksAdded
                    }.Build();
                    await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author} to the queue."));
                return;
            }
                
            if (track != null) await guildPlayer.PlayAsync(track);
            
            if (queue is { Count: >= 2 })
            {
                AddPlaylistToQueue();
                track = guildPlayer.Queue[0];
                guildPlayer.PlayQueue();
            }
            
            if (track == null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Failed to play {title} because I lost it."));
                return;
            }
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Now playing [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author}."));
            return;

            string? AddPlaylistToQueue()
            {
                string? tracksAdded = null;
                
                if (queue == null)
                {
                    if (track == null) return tracksAdded;
                    guildPlayer.AddToQueue(track);
                    tracksAdded += $"{track.Info.Title} by {track.Info.Author}";
                    return tracksAdded;   
                }
                
                for (int i = 0; i < queue.Count -1; i++)
                {
                    if (force)
                        guildPlayer.AddToQueueAt(i, queue[i]);
                    else
                        guildPlayer.AddToQueue(queue[i]);
                
                    tracksAdded += $"{queue[i].Info.Title} by {queue[i].Info.Author}";
                    if (i < queue.Count - 1) tracksAdded += "\n";
                }
                
                return tracksAdded;
            }
        }
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task PauseTrack(InteractionContext e)
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
    public async Task ResumeTrack(InteractionContext e)
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
    public async Task StopTrack(InteractionContext e,
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
    public async Task SkipTrack(InteractionContext e)
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
    public async Task CurrentlyPlaying(InteractionContext e)
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
        public async Task GetQueue(InteractionContext e)
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
            [Option("volume", "The volume to set the bot to"), MinimumValue(0), MaximumValue(200)] int vol)
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
    
            await guildPlayer.SetVolumeAsync(vol);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Set the volume to {vol}."));
        }
    }
}