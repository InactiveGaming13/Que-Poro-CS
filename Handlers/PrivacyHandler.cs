using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Privacy.
/// </summary>
[SlashCommandGroup("privacy", "Is privacy commands")]
public class PrivacyCommands : ApplicationCommandsModule
{
    /// <summary>
    /// The class for handling User Privacy.
    /// </summary>
    [SlashCommandGroup("user", "Is per user privacy commands")]
    public class UserPrivacy : ApplicationCommandsModule
    {
        /// <summary>
        /// Sets whether a User is tracked.
        /// </summary>
        /// <param name="e">The Interaction arguments.</param>
        /// <param name="tracked">Whether the User is tracked.</param>
        [SlashCommand("tracking", "Whether I should track your sent messages")]
        public static async Task UserTracking(InteractionContext e, 
            [Option("tracked", "If I should track your message count")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Guild is null)
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
                    $"I already {(tracked ? "tracks" : "doesn't track")} your message count."));
                return;
            }

            if (!await Users.ModifyUser(e.UserId, e.User.GlobalName, tracked: tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "An unexpected database error occured."));
                return;
            }
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"I will {(tracked ? "now" : "no longer")} track your message count."));
        }
    }

    /// <summary>
    /// The class for handling Channel Privacy.
    /// </summary>
    [SlashCommandGroup("channel", "Is per channel privacy commands")]
    public class ChannelPrivacy : ApplicationCommandsModule
    {
        /// <summary>
        /// Sets whether a Channel is tracked.
        /// </summary>
        /// <param name="e">The Interaction arguments.</param>
        /// <param name="channel">The Channel.</param>
        /// <param name="tracked">Whether to track the Channel.</param>
        [SlashCommand("tracking", "Whether I should track a specified channel")]
        public static async Task ChannelTracking(InteractionContext e, 
            [Option("channel", "The channel I should or shouldn't track")]
            DiscordChannel channel,
            [Option("tracked", "If I should track a specified channel")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
            
            if (!await Users.UserExists(e.UserId))
                await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            UserRow user = await Users.GetUser(e.UserId);

            if (user is { Admin: false } && !e.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not an admin."));
                return;
            }
            
            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);

            if (!await Channels.ChannelExists(channel.Id))
                await Channels.AddChannel(channel.Id, e.Guild.Id, channel.Name, channel.Topic);
            ChannelRow databaseChannel = await Channels.GetChannel(channel.Id);

            if (tracked.Equals(databaseChannel.Tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"I already {(tracked ? "tracks" : "doesn't track")} {channel.Mention}."));
                return;
            }

            await Channels.ModifyChannel(channel.Id, tracked: tracked);
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"I will {(tracked ? "now" : "no longer")} track {channel.Mention}."));
        }
    }
    
    /// <summary>
    /// The class for handling Guild Privacy.
    /// </summary>
    [SlashCommandGroup("guild", "Is per guild privacy commands")]
    public class GuildPrivacy : ApplicationCommandsModule
    {
        /// <summary>
        /// Sets whether the Guild is tracked.
        /// </summary>
        /// <param name="e">The Interaction arguments.</param>
        /// <param name="tracked">Whether the Guild is tracked.</param>
        [SlashCommand("tracking", "Whether I should track the current guild")]
        public static async Task GuildTracking(InteractionContext e, 
            [Option("tracked", "If I should track a specified guild")]
            bool tracked = true)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
            
            if (!await Users.UserExists(e.UserId))
                await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            UserRow user = await Users.GetUser(e.UserId);
            
            if (user is { Admin: false } && !e.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not an admin."));
                return;
            }

            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
            GuildRow guild = await Guilds.GetGuild(e.Guild.Id);

            if (tracked.Equals(guild.Tracked))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"I already {(tracked ? "tracks" : "doesn't track")} **{e.Guild.Name}**."));
                return;
            }

            await Guilds.ModifyGuild(e.Guild.Id, tracked: tracked);
        
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"I will {(tracked ? "now" : "no longer")} track **{e.Guild.Name}**."));
        }
    }
}