using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling the music commands.
/// </summary>
[SlashCommandGroup("music", "Music commands")]
public class MusicCommands : ApplicationCommandsModule
{
    [SlashCommand("play", "Plays a song")]
    public async Task PlayTrack(InteractionContext ctx,
        [Option("song", "Song title to play")] string query,
        [Option("force", "Force play and override the queue")]
        bool force = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        // Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }
        
        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        
        if (guildPlayer == null)
        {
            var session = lavalink.ConnectedSessions.Values.First();
            await session.ConnectAsync(ctx.Member.VoiceState.Channel);
            guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        }
        
        if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        LavalinkTrackLoadingResult loadResult;
        
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
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
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
            
            if (guildPlayer.CurrentTrack != null)
            {
                string embedTitle = "Added playlist to queue";
                switch (force)
                {
                    case true when ctx.Member.Permissions.HasPermission(Permissions.Administrator) && track != null:
                        await guildPlayer.PlayAsync(track);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                            $"Force played [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author}."));
                        return;
                    
                    case true when ctx.Member.Permissions.HasPermission(Permissions.Administrator) && queue != null:
                        embedTitle = "Force added playlist to queue";
                        break;
                    
                    case false when !ctx.Member.Permissions.HasPermission(Permissions.Administrator) && track != null:
                        guildPlayer.AddToQueue(track);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }
                
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
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
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Failed to play {title} because I lost it."));
                return;
            }
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Now playing [{track?.Info.Title}]({track?.Info.Uri}) by {track?.Info.Author}."));
        }
        await GetResult(query);
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task PauseTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        if (guildPlayer.Player.Paused)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The track is already paused."));
            return;
        }

        await guildPlayer.PauseAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Paused [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}."));
    }
    
    [SlashCommand("resume", "Resumes the current song")]
    public async Task ResumeTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        await guildPlayer.ResumeAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Resumed [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}."));
    }
    
    [SlashCommand("stop", "Stops the current song")]
    public async Task StopTrack(InteractionContext ctx,
        [Option("clear_queue", "Clears the queue")] bool clear = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }

        if (!guildPlayer.Queue.Any() && guildPlayer.CurrentTrack == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There is nothing to stop."));
        }

        string response = "Stopped the current song";

        if (clear)
        {
            guildPlayer.ClearQueue();
            response += " and cleared the queue";
        }
        
        await guildPlayer.StopAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(response));
    }
    
    [SlashCommand("skip", "skips the current song")]
    public async Task SkipTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        
        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing is currently playing."));
            return;
        }

        LavalinkTrack nextTrack = guildPlayer.Queue.First();
        await guildPlayer.SkipAsync();
        await ctx.EditResponseAsync($"Skipped to [{nextTrack.Info.Title}]({nextTrack.Info.Uri}) by {nextTrack.Info.Author}.");
    }
    
    [SlashCommand("currently_playing", "Gets the current song")]
    public async Task CurrentlyPlaying(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
            return;
        }
        
        if (guildPlayer.CurrentTrack == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing is currently playing."));
            return;
        }

        if (!guildPlayer.Player.Paused)
        {
            await ctx.EditResponseAsync($"Currently playing [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
            return;
        }
        
        await ctx.EditResponseAsync($"Currently paused [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
    }

    [SlashCommandGroup("queue", "The queue commands")]
    public class MusicQueueCommands : ApplicationCommandsModule
    {
        [SlashCommand("list", "Lists the current queue")]
        public async Task GetQueue(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
                return;
            }

            var lavalink = ctx.Client.GetLavalink();
            var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

            if (guildPlayer == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }

            string title = guildPlayer.Queue.Count == 0
                ? "The queue is empty"
                : $"The current queue for {ctx.Guild.Name}";

            string queue = guildPlayer.Queue.Aggregate("", (current, track) => current + $"{track.Info.Title} by {track.Info.Author}\n");

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = title,
                Description = queue
            }.Build();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }

        [SlashCommand("clear", "Clears the current queue")]
        public static async Task ClearQueue(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
                return;
            }

            var lavalink = ctx.Client.GetLavalink();
            var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

            if (guildPlayer == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            guildPlayer.ClearQueue();
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Cleared the queue for {ctx.Guild.Name}"));
        }

        [SlashCommand("shuffle", "Shuffles the current queue")]
        public static async Task ShuffleQueue(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
                return;
            }

            var lavalink = ctx.Client.GetLavalink();
            var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

            if (guildPlayer == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            guildPlayer.ShuffleQueue();
            string title = guildPlayer.Queue.Count == 0
                ? "The queue is empty"
                : $"The current queue for {ctx.Guild.Name} (shuffled)";

            string queue = guildPlayer.Queue.Aggregate("", (current, track) => current + $"{track.Info.Title} by {track.Info.Author}\n");

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = title,
                Description = queue
            }.Build();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }
    }
    
    [SlashCommandGroup("volume", "The volume control commands")]
    public class MusicVolumeCommands : ApplicationCommandsModule
    {
        [SlashCommand("get", "Gets the volume of the bot")]
        public static async Task GetVolume(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
                return;
            }
    
            var lavalink = ctx.Client.GetLavalink();
            var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
    
            if (guildPlayer == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
    
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"The current volume is {guildPlayer.Player.Volume}"));
        }
    
        [SlashCommand("set", "Sets the volume of the bot")]
        public static async Task SetVolume(InteractionContext ctx,
            [Option("volume", "The volume to set the bot to"), MinimumValue(0), MaximumValue(200)] int vol)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Member?.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
                return;
            }
    
            var lavalink = ctx.Client.GetLavalink();
            var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
    
            if (guildPlayer == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a voice channel."));
                return;
            }
            
            if (guildPlayer.Channel != ctx.Channel && !ctx.Member.Permissions.HasPermission(Permissions.MoveMembers))
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent("You are not in my vc and you lack permission to move me."));
                return;
            }
    
            await guildPlayer.SetVolumeAsync(vol);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Set the volume to {vol}."));
        }
    }
}