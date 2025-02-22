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
using DisCatSharp.Net;

namespace Que_Poro_CS.Handlers;

public class LavalinkCfg
{
    private static readonly ConnectionEndpoint Endpoint = new ConnectionEndpoint()
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
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established! Attempting to re-connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }

        var session = lavalink.ConnectedSessions.Values.First();

        /*if (channel.Type != ChannelType.Voice || channel.Type != ChannelType.Stage)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a valid voice channel."));
            return;
        }*/

        await session.ConnectAsync(channel);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel.Mention}."));
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
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
    public static async Task SwitchChannel(InteractionContext ctx, DiscordChannel channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

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
            Console.WriteLine($"{e.User.Username} has switched from {e.Before.Channel.Name} to {e.Channel.Name}");
            if (CreateAVcHandler.TempVcs.Contains(e.Before.Channel) && e.Before.Channel.Users.Count == 0)
            {
                await CreateAVcHandler.RemoveTempVc(e);
            }
            if (Convert.ToString(e.After.ChannelId) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
            {
                await CreateAVcHandler.CreateTempVc(e);
            }
            return;
        }
        
        if (e.Before != null)
        {
            Console.WriteLine($"{e.User.Username} has left {e.Before.Channel.Name}");
            if (CreateAVcHandler.TempVcs.Contains(e.Before.Channel) && e.Before.Channel.Users.Count == 0)
            {
                await CreateAVcHandler.RemoveTempVc(e);
            }
        }
        
        if (e.After.ChannelId != null)
        {
            Console.WriteLine($"{e.User.Username} has joined {e.After.Channel.Name}");
            if (Convert.ToString(e.After.ChannelId) == Environment.GetEnvironmentVariable("TEMP_VC_ID"))
            {
                await CreateAVcHandler.CreateTempVc(e);
            }
        }
    }
}