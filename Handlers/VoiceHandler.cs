using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

public static class LavalinkCfg
{
    private static readonly ConnectionEndpoint Endpoint = new()
    {
        Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOST")!,
        Port = Convert.ToInt32(Environment.GetEnvironmentVariable("LAVALINK_PORT"))
    };

    public static readonly LavalinkConfiguration LavalinkConfig = new(new LavalinkConfiguration()
    {
        Password = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD")!,
        RestEndpoint = Endpoint,
        SocketEndpoint = Endpoint,
        EnableBuiltInQueueSystem = true
    });
}

[SlashCommandGroup("voice", "Voice commands")]
public class VoiceCommands : ApplicationCommandsModule
{
    [SlashCommand("join", "Joins a Voice Channel (defaults to your current channel)")]
    public async Task Join(InteractionContext e,
        [Option("channel", "The specified channel to join"), ChannelTypes(ChannelType.Voice)] DiscordChannel? channel = null!)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!VoiceHandler.UsingLavaLink)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I am not currently configured for voice."));
            return;
        }

        if (e.Member.VoiceState.Channel is null && channel is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a VC."));
            return;
        }
        
        if (e.Member.VoiceState.Channel is not null && channel is null)
        {
            channel = e.Member.VoiceState.Channel;
        }

        if (channel is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "An unknown error occured."));
            return;
        }

        GuildRow databaseGuild;
        if (await Guilds.GuildExists(e.Guild.Id))
            databaseGuild = await Guilds.GetGuild(e.Guild.Id);
        else
        {
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
            databaseGuild = await Guilds.GetGuild(e.Guild.Id);
        }

        if (channel.Id.Equals(databaseGuild.TempVcChannel))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I am unable to join a 'Create A VC' channel."));
            return;
        }
        
        if (channel.Type is not ChannelType.Voice)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not a valid voice channel."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();
        
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);
        
        if (guildPlayer is not null)
        {
            await VoiceHandler.SwitchChannel(e, channel);
            return;
        }
        
        if (!lavalink.ConnectedSessions.Any())
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(
                    "Attempting to connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }

        LavalinkSession session = lavalink.ConnectedSessions.Values.First();

        await session.ConnectAsync(channel);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Joined {channel.Mention}."));
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!VoiceHandler.UsingLavaLink)
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am not currently configured for voice."));
            return;
        }
        
        LavalinkExtension lavalink = e.Client.GetLavalink();
        
        if (!lavalink.ConnectedSessions.Any())
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("The Lavalink connection is not established! Attempting to re-connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }
        
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);
        
        if (guildPlayer is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I am not in a VC."));
            return;
        }

        await guildPlayer.DisconnectAsync();
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Left {guildPlayer.Channel.Mention}."));
    }
}

public static class VoiceHandler
{
    public static readonly bool UsingLavaLink = Environment.GetEnvironmentVariable("LAVALINK_HOST") != null &&
                                                Environment.GetEnvironmentVariable("LAVALINK_PASSWORD") != null;
    
    public static async Task SwitchChannel(InteractionContext e, DiscordChannel channel)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!UsingLavaLink)
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I am not currently configured for voice."));
            return;
        }

        LavalinkExtension lavalink = e.Client.GetLavalink();

        if (!lavalink.ConnectedSessions.Any())
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Attempting to connect."));
            await lavalink.ConnectAsync(LavalinkCfg.LavalinkConfig);
        }
        
        
        LavalinkGuildPlayer? guildPlayer = lavalink.GetGuildPlayer(e.Guild);

        if (guildPlayer is not null)
        {
            await guildPlayer.DisconnectAsync();
        }

        DiscordUser owner =
            await e.Client.GetUserAsync(Convert.ToUInt64(Environment.GetEnvironmentVariable("BOT_OWNER_ID")));
        await owner.SendMessageAsync(
            $"Lavalink failed to connect during SwitchChannel at {DateTime.Now.TimeOfDay}");
        
        if (!lavalink.ConnectedSessions.Any())
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "The Lavalink connection is not established! I have reported this to my owner."));
            return;
        }

        LavalinkSession session = lavalink.ConnectedSessions.Values.First();

        await session.ConnectAsync(channel);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Switched to {channel.Mention}."));
    }

    public static async Task VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
    {
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        GuildRow guild = await Guilds.GetGuild(e.Guild.Id);

        if (!await Users.UserExists(e.User.Id))
            await Users.AddUser(e.User.Id, e.User.Username, e.User.GlobalName);
        
        
        if (guild is { TempVcEnabled: false} or { TempVcChannel: null or 0 })
            return;
        
        switch (e)
        {
            case { Before.Channel: not null, After.Channel: not null }:
            {
                if (e.Before.Channel == e.After.Channel)
                    return;
                
                if (await TempVcs.TempVcExists(e.Before.Channel.Id) && e.Before.Channel.Users.Count == 0)
                {
                    TempVcRow tempVc = await TempVcs.GetTempVc(e.Before.Channel.Id);
                    if (e.After.Channel.Id.Equals(guild.TempVcChannel))
                    {
                        await e.Before.Channel.PlaceMemberAsync(e.After.Member);
                        tempVc.UserQueue.Remove(e.User.Id);
                        tempVc.UserQueue.Add(e.User.Id);
                        if (tempVc.UserQueue.First().Equals(e.User.Id))
                            return;

                        while (!e.Before.Channel.Users.Any(user => user.Id.Equals(tempVc.UserQueue.First())))
                        {
                            tempVc.UserQueue.Remove(tempVc.UserQueue.First());
                        }

                        DiscordUser user = await client.GetUserAsync(tempVc.UserQueue.First());

                        string channelName = (user.GlobalName ?? user.Username).EndsWith('s')
                            ? $"{user.GlobalName ?? user.Username}' VC"
                            : $"{user.GlobalName ?? user.Username}'s VC";

                        await TempVcs.ModifyTempVc(e.Before.Channel.Id, name: channelName, master: user.Id,
                            userQueue: tempVc.UserQueue);
                        await CreateAVcHandler.ModifyTempVc(user, e.Before.Channel, channelName, null, null);
                        
                        return;
                    }
                    await TempVcs.RemoveTempVc(e.Before.Channel.Id);
                    await CreateAVcHandler.RemoveTempVc(e.Before.Channel, "being empty");
                }

                if (!e.After.Channel.Id.Equals(guild.TempVcChannel)) return;
                DiscordChannel? channel = await CreateAVcHandler.CreateTempVc(e);
                if (channel?.Bitrate is null || channel.UserLimit is null)
                    return;
                await TempVcs.AddTempVc(channel.Id, e.User.Id, e.Guild.Id, channel.Name,
                    (int)channel.Bitrate / 1000, (int)channel.UserLimit, channel.Users.Count, [e.User.Id]);
                return;
            }
            
            case { Before.Channel: not null }:
            {
                if (await TempVcs.TempVcExists(e.Before.Channel.Id) && (e.Before.Channel.Users.Count == 0 ||
                                                                        e.Before.Channel.Users is [{ IsBot: true }]))
                {
                    await TempVcs.RemoveTempVc(e.Before.Channel.Id);
                    await CreateAVcHandler.RemoveTempVc(e.Before.Channel, "being empty");
                    return;
                }

                if (await TempVcs.TempVcExists(e.Before.Channel.Id) && e.Before.Channel.Users.Count > 0)
                {
                    TempVcRow tempVc = await TempVcs.GetTempVc(e.Before.Channel.Id);
                    tempVc.UserQueue.Remove(e.User.Id);
                    if (tempVc.Master.Equals(e.User.Id))
                    {
                        tempVc.Master = tempVc.UserQueue.First();
                        DiscordUser user = await client.GetUserAsync(tempVc.UserQueue.First());
                        tempVc.Name = (user.GlobalName ?? user.Username).EndsWith('s')
                            ? $"{user.GlobalName ?? user.Username}' VC"
                            : $"{user.GlobalName ?? user.Username}'s VC";

                        await CreateAVcHandler.ModifyTempVc(e.User, e.Before.Channel, tempVc.Name, null, null);
                    }

                    await TempVcs.ModifyTempVc(e.Before.Channel.Id, tempVc.Master, tempVc.Name,
                        userQueue: tempVc.UserQueue);
                }

                break;
            }
            
            case { After.Channel: not null }:
                if (e.After.Channel.Id.Equals(guild.TempVcChannel))
                {
                    DiscordChannel? channel = await CreateAVcHandler.CreateTempVc(e);
                    if (channel?.Bitrate is null || channel.UserLimit is null)
                        return;
                    await TempVcs.AddTempVc(channel.Id, e.User.Id, e.Guild.Id, channel.Name,
                        (int)channel.Bitrate / 1000, (int)channel.UserLimit, channel.Users.Count, [e.User.Id]);
                }

                if (await TempVcs.TempVcExists(e.After.Channel.Id))
                {
                    TempVcRow tempVc = await TempVcs.GetTempVc(e.After.Channel.Id);
                    tempVc.UserQueue.Add(e.User.Id);
                    await TempVcs.ModifyTempVc(e.After.Channel.Id, userQueue: tempVc.UserQueue,
                        userCount: e.After.Channel.Users.Count);
                }
                break;
        }
    }
}