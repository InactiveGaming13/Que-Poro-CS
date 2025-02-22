using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("music", "Music commands")]
public class MusicCommands : ApplicationCommandsModule
{
    [SlashCommand("play", "Play some music")]
    public async Task PlayTrack(InteractionContext ctx,
        [Option("song", "Song title to play")] string track,
        [Option("force", "Force play and override the queue")] bool force = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        // Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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

        async Task GetResult(string title, bool usingYt = false)
        {
            if (usingYt)
            {
                loadResult = await guildPlayer.LoadTracksAsync(LavalinkSearchType.Youtube, title);
            }
            else
            {
                loadResult = await guildPlayer.LoadTracksAsync(LavalinkSearchType.Plain, title);
            }
            
            
            // If something went wrong on Lavalink's end or it just couldn't find anything.
            if (loadResult.LoadType == LavalinkLoadResultType.Empty || loadResult.LoadType == LavalinkLoadResultType.Error)
            {
                if (!usingYt)
                {
                    await GetResult(title, true);
                    return;
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Track search failed for {title}."));
                return;
            }
            
            LavalinkTrack track = loadResult.LoadType switch
            {
                LavalinkLoadResultType.Track => loadResult.GetResultAs<LavalinkTrack>(),
                LavalinkLoadResultType.Playlist => loadResult.GetResultAs<LavalinkPlaylist>().Tracks.First(),
                LavalinkLoadResultType.Search => loadResult.GetResultAs<List<LavalinkTrack>>().First(),
                _ => throw new InvalidOperationException("Unexpected load result type.")
            };

            if (force && ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await guildPlayer.PlayAsync(track);
                await ctx.EditResponseAsync($"Forced [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
                return;
            }

            if (force && !ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                guildPlayer.AddToQueue(track);
                await ctx.EditResponseAsync($"Added [{track.Info.Title}]({track.Info.Uri}) by {track.Info.Author} to the queue (you lack permissions to force play).");
                return;
            }

            if (guildPlayer.CurrentTrack != null)
            {
                guildPlayer.AddToQueue(track);
                await ctx.EditResponseAsync($"Added [{track.Info.Title}]({track.Info.Uri}) by {track.Info.Author} to the queue.");
                return;
            }

            await guildPlayer.PlayAsync(track);

            await ctx.EditResponseAsync($"Now playing [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {track.Info.Author}.");
        }
        
        await GetResult(track);
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task PauseTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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
    
    [SlashCommand("stop", "Stops the song and clears the queue")]
    public async Task StopTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The queue is already clear."));
        }

        guildPlayer.ClearQueue();
        await guildPlayer.StopAsync();
        await ctx.EditResponseAsync("Stopped the song and cleared the queue.");
    }
    
    [SlashCommand("skip", "skips the current song")]
    public async Task SkipTrack(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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

        await guildPlayer.SkipAsync();
        await ctx.EditResponseAsync($"Skipped to [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
    }
    
    [SlashCommand("currently_playing", "Gets the current song")]
    public async Task CurrentlyPlaying(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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

    [SlashCommand("queue", "Gets the current queue")]
    public async Task GetQueue(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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

        if (guildPlayer.Queue.Count == 0)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The current queue is empty!"));
            return;
        }
        
        string queue = "The current queue:\n";
        
        foreach (var song in guildPlayer.Queue)
        {
            queue += $"{song.Info.Title} by {song.Info.Author}\n";
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(queue));
    }
    
    [SlashCommand("volume_get", "Gets the volume of the bot")]
    public static async Task GetVolume(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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

    [SlashCommand("volume_set", "Sets the volume of the bot")]
    public static async Task SetVolume(InteractionContext ctx,
        [Option("volume", "The volume to set the bot to"), MinimumValue(0), MaximumValue(200)] int vol)
    {
        Console.WriteLine(vol);
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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