using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("voice", "Voice commands")]
public class VoiceCommands : ApplicationCommandsModule
{
    [SlashCommand("join", "Joins a Voice Channel")]
    public async Task Join(InteractionContext ctx,
        [Option("channel", "Channel to join"), ChannelTypes(ChannelType.Voice)] DiscordChannel channel = null!)
    {
        if (channel is null && ctx.Member.VoiceState != null && ctx.Member.VoiceState.Channel != null)
        {
            channel = ctx.Member.VoiceState.Channel;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        if (guildPlayer != null)
        {
            await VoiceHandler.SwitchChannel(ctx, channel);
            return;
        }
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established (The bot needs to be restarted)"));
            return;
        }

        var session = lavalink.ConnectedSessions.Values.First();

        /*if (channel.Type != ChannelType.Voice || channel.Type != ChannelType.Stage)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a valid voice channel."));
            return;
        }*/

        await session.ConnectAsync(channel);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel.Mention}!"));
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Lavalink not connected."));
            return;
        }

        await guildPlayer.DisconnectAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Left {guildPlayer.Channel.Mention}!"));
    }

    [SlashCommand("play", "Play some music")]
    public async Task Play(InteractionContext ctx,
        [Option("song", "Song title to play")] string track,
        [Option("force", "Force play and override the queue")] bool force = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        // Important to check the voice state itself first, as it may throw a NullReferenceException if they don't have a voice state.
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync("You are not in a voice channel.");
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
            } else if (force && !ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                guildPlayer.AddToQueue(track);
                await ctx.EditResponseAsync($"Added [{track.Info.Title}]({track.Info.Uri}) to the queue (you lack permissions to force play).");
                return;
            }

            if (guildPlayer.CurrentTrack != null)
            {
                guildPlayer.AddToQueue(track);
                await ctx.EditResponseAsync($"Added [{track.Info.Title}]({track.Info.Uri}) to the queue.");
                return;
            }

            await guildPlayer.PlayAsync(track);

            await ctx.EditResponseAsync($"Now playing [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {track.Info.Author}!");
        }
        
        await GetResult(track);
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task Pause(InteractionContext ctx)
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
    public async Task Resume(InteractionContext ctx)
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
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        await guildPlayer.ResumeAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Resumed [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}."));
    }
    
    [SlashCommand("stop", "Stops the current song")]
    public async Task Stop(InteractionContext ctx)
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
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
            return;
        }

        guildPlayer.ClearQueue();
        await guildPlayer.StopAsync();
        await ctx.EditResponseAsync("Stopped the track.");
    }
    
    [SlashCommand("skip", "skips the current song")]
    public async Task Skip(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a voice channel."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

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
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no tracks loaded."));
        }
        
        await ctx.EditResponseAsync($"Currently playing [{guildPlayer.CurrentTrack.Info.Title}]({guildPlayer.CurrentTrack.Info.Uri}) by {guildPlayer.CurrentTrack.Info.Author}.");
    }
}

public class VoiceHandler : ApplicationCommandsModule
{
    public static async Task SwitchChannel(InteractionContext ctx, DiscordChannel channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var lavalink = ctx.Client.GetLavalink();
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);

        if (guildPlayer != null)
        {
            await guildPlayer.DisconnectAsync();
        }

        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established"));
            return;
        }

        var session = lavalink.ConnectedSessions.Values.First();

        await session.ConnectAsync(channel);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Switched to {channel.Mention}!"));
    }

    public static async Task VoiceStateUpdated(DiscordClient s, VoiceStateUpdateEventArgs e)
    {
        Console.WriteLine($"User: {e.User.Username}");
        
        if (e.Before != null)
        {
            Console.WriteLine($"Before: {e.Before.Channel.Name}");
        }
        
        if (e.After != null)
        {
            Console.WriteLine($"After: {e.After.Channel.Name}");
        }
    }
}