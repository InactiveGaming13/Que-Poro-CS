using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

[SlashCommandGroup("privacy", "The bots privacy commands")]
public class PrivacyCommands : ApplicationCommandsModule
{
    [SlashCommandGroup("user", "The bots per user privacy commands")]
    public class UserPrivacy : ApplicationCommandsModule
    {
        [SlashCommand("tracking", "Whether the bot should track your sent messages")]
        public static async Task UserTracking(InteractionContext e, 
            [Option("tracked", "If the bot should track your message count")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
            
            if (!await Users.UserExists(e.UserId))
                await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            UserRow user = await Users.GetUser(e.UserId);

            if (tracked.Equals(user.Tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"The bot already {(tracked ? "tracks" : "doesn't track")} your message count."));
                return;
            }

            await Users.ModifyUser(e.UserId, e.User.GlobalName, tracked: tracked);
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot will {(tracked ? "now" : "no longer")} track your message count."));
        }
    }

    [SlashCommandGroup("channel", "The bots per channel privacy commands")]
    public class ChannelPrivacy : ApplicationCommandsModule
    {
        [SlashCommand("tracking", "Whether the bot should track a specified channel")]
        public static async Task ChannelTracking(InteractionContext e, 
            [Option("channel", "The channel the bot should or shouldn't track")]
            DiscordChannel channel,
            [Option("tracked", "If the bot should track a specified channel")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }

            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);

            if (!await Channels.ChannelExists(channel.Id))
                await Channels.AddChannel(channel.Id, e.Guild.Id, channel.Name, channel.Topic);
            ChannelRow databaseChannel = await Channels.GetChannel(channel.Id);
            
            if (!await Users.UserExists(e.UserId))
                await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            UserRow user = await Users.GetUser(e.UserId);

            if (tracked.Equals(databaseChannel.Tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"The bot already {(tracked ? "tracks" : "doesn't track")} {channel.Mention}."));
                return;
            }

            await Channels.ModifyChannel(channel.Id, tracked: tracked);
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot will {(tracked ? "now" : "no longer")} track {channel.Mention}."));
        }
    }
    
    [SlashCommandGroup("guild", "The bots per guild privacy commands")]
    public class GuildPrivacy : ApplicationCommandsModule
    {
        [SlashCommand("tracking", "Whether the bot should track the current guild")]
        public static async Task GuildTracking(InteractionContext e, 
            [Option("tracked", "If the bot should track a specified guild")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Member?.VoiceState is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }

            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
            GuildRow guild = await Guilds.GetGuild(e.Guild.Id);

            if (tracked.Equals(guild.Tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"The bot already {(tracked ? "tracks" : "doesn't track")} **{e.Guild.Name}**."));
                return;
            }

            await Guilds.ModifyGuild(e.Guild.Id, tracked: tracked);
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot will {(tracked ? "now" : "no longer")} track **{e.Guild.Name}**."));
        }
    }
}