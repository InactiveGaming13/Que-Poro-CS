using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;

namespace QuePoro.Handlers;

public class LavalinkCfg
{
    private static readonly ConnectionEndpoint Endpoint = new()
    {
        Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOST"),
        Port = Convert.ToInt32(Environment.GetEnvironmentVariable("LAVALINK_PORT"))
    };

    public static readonly LavalinkConfiguration LavalinkConfig = new(new LavalinkConfiguration()
    {
        Password = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD"),
        RestEndpoint = Endpoint,
        SocketEndpoint = Endpoint,
        EnableBuiltInQueueSystem = true
    });
}

[SlashCommandGroup("voice", "Voice commands")]
public class VoiceCommands : ApplicationCommandsModule
{
    bool usingLavaLink = Environment.GetEnvironmentVariable("LAVALINK_HOST") != null && Environment.GetEnvironmentVariable("LAVALINK_PASSWORD") != null;
    
    [SlashCommand("join", "Joins a Voice Channel (defaults to your current channel)")]
    public async Task Join(InteractionContext ctx,
        [Option("channel", "The specified channel to join"), ChannelTypes(ChannelType.Voice)] DiscordChannel? channel = null!)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (!usingLavaLink)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am not currently configured for voice."));
            return;
        }

        if (ctx.Member.VoiceState?.Channel == null!)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a VC."));
            return;
        }
        
        if (channel is null && ctx.Member.VoiceState?.Channel != null!)
        {
            channel = ctx.Member.VoiceState.Channel;
        }

        if (Convert.ToString(channel.Id) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am unable to join a 'Create A VC' channel."));
            return;
        }
        
        /*if (channel.Type != ChannelType.Voice || channel.Type != ChannelType.Stage)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a valid voice channel."));
            return;
        }*/

        var lavalink = ctx.Client.GetLavalink();
        
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        if (guildPlayer != null)
        {
            await VoiceHandler.SwitchChannel(ctx, channel);
            return;
        }
        
        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established! Attempting to re-connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }

        var session = lavalink.ConnectedSessions.Values.First();

        await session.ConnectAsync(channel);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel.Mention}."));
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (!usingLavaLink)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am not currently configured for voice."));
            return;
        }
        
        var lavalink = ctx.Client.GetLavalink();
        
        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established! Attempting to re-connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }
        
        var guildPlayer = lavalink.GetGuildPlayer(ctx.Guild);
        
        if (guildPlayer == null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a VC."));
            return;
        }

        await guildPlayer.DisconnectAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Left {guildPlayer.Channel.Mention}."));
    }
}

public class VoiceHandler
{
    static bool usingLavaLink = Environment.GetEnvironmentVariable("LAVALINK_HOST") != null && Environment.GetEnvironmentVariable("LAVALINK_PASSWORD") != null;
    
    public static async Task SwitchChannel(InteractionContext ctx, DiscordChannel? channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (!usingLavaLink)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am not currently configured for voice."));
            return;
        }

        var lavalink = ctx.Client.GetLavalink();

        if (!lavalink.ConnectedSessions.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established! Attempting to re-connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }
        
        
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
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Switched to {channel.Mention}."));
    }

    public static async Task VoiceStateUpdated(DiscordClient s, VoiceStateUpdateEventArgs e)
    {
        if (e is { Before: not null, After.ChannelId: not null })
        {
            if (CreateAVcHandler.TempVcs.Contains(e.Before.Channel) && e.Before.Channel.Users.Count == 0)
            {
                if (Convert.ToString(e.After.ChannelId) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
                {
                    await e.Before.Channel.PlaceMemberAsync(e.After.Member);
                    return;
                }
                await CreateAVcHandler.RemoveTempVc(e.Before.Channel, "being empty");
            }
            if (Convert.ToString(e.After.ChannelId) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
            {
                await CreateAVcHandler.CreateTempVc(e);
            }
            return;
        }
        
        if (e.Before != null)
        {
            if (CreateAVcHandler.TempVcs.Contains(e.Before.Channel) && e.Before.Channel.Users.Count == 0)
            {
                await CreateAVcHandler.RemoveTempVc(e.Before.Channel, "being empty");
            }

            if (CreateAVcHandler.TempVcs.Contains(e.Before.Channel) && e.Before.Channel.Users is [{ IsBot: true }])
            {
                await CreateAVcHandler.RemoveTempVc(e.Before.Channel, "being empty with a bot");
            }
        }
        
        if (e.After.ChannelId != null)
        {
            if (Convert.ToString(e.After.ChannelId) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
            {
                await CreateAVcHandler.CreateTempVc(e);
            }
        }
    }

    public static async Task VoiceChannelStatusUpdated(DiscordClient s, VoiceChannelStatusUpdateEventArgs e)
    {
        Console.WriteLine($"Status updated for {e.Channel.Name} to {e.Status}");
    }
}