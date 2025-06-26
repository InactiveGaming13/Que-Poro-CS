using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using QuePoro.Database.Types;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Message commands.
/// </summary>
[SlashCommandGroup("messages", "Message management commands")]
public class MessageCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Purges a number of messages.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="amount">The amount of Messages to purge.</param>
    /// <param name="channel">The Channel to purge.</param>
    [SlashCommand("clear", "Clears (purges) a set amount of messages")]
    public static async Task ClearMessages(InteractionContext e,
        [Option("amount", "The amount of messages to clear (defaults to 3)"), MinimumValue(1), MaximumValue(100)]
        int amount = 3,
        [Option("channel", "The channel to clear (defaults to current channel)"), ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null!
    )
    {
        // If the command came from a DM, ignore it.
        if (e.Member is null || e.Guild is null)
        {
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        // Ensure the Channel is never null.
        channel ??= e.Channel;

        // Purge the Messages.
        await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(amount),
            $"Purged by User: {e.User.GlobalName}");
        
        // Tell the user that messages were purged.
        string clearedMessage = amount == 1 ? "Cleared 1 message." : $"Cleared {amount} messages.";
        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(clearedMessage));
        
        // Wait 1 second, then delete the message.
        Thread.Sleep(2000);
        await e.DeleteResponseAsync();
    }
}

/// <summary>
/// The class for handling Message events.
/// </summary>
public static class MessageHandler
{
    /// <summary>
    /// Handles the Message creation event.
    /// </summary>
    /// <param name="client">The Bot.</param>
    /// <param name="e">The Interaction arguments.</param>
    public static async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
    {
        // Check if the Guild is null.
        if (e.Guild is null)
            return;
        
        // Check if the database contains the Guild, Channel, User and User Stats and add them if it doesn't.
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        GuildRow guild = await Guilds.GetGuild(e.Guild.Id);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        ChannelRow channel = await Channels.GetChannel(e.Channel.Id);
        
        if (!await Users.UserExists(e.Author.Id))
            await Users.AddUser(e.Author.Id, e.Author.Username, e.Author.GlobalName);
        UserRow user = await Users.GetUser(e.Author.Id);
        
        if (!await UserStats.StatExists(e.Author.Id, e.Channel.Id, e.Guild.Id))
            await UserStats.AddStat(e.Author.Id, e.Channel.Id, e.Guild.Id);
        UserStatRow userStats = await UserStats.GetStat(e.Author.Id, e.Channel.Id, e.Guild.Id);
        
        // If the message starts with “@ignore”, ignore the message.
        if (e.Message.Content.StartsWith("@ignore", StringComparison.CurrentCultureIgnoreCase))
            return;
        
        // If the User, Channel and Guild has tracking enabled, update the User Stats.
        if (user is { Tracked: true } && channel is { Tracked: true } && guild is { Tracked: true })
            await UserStats.ModifyStat(e.Author.Id, e.Channel.Id, e.Guild.Id, sent: userStats.SentMessages ++);

        string? delete = await BannedPhraseHandler.HandleBannedPhrases(e.Message.Content);
        if (delete is not null)
        {
            string response = $"Hey {e.Author.Mention}! You can't send that here.";
            DiscordChannel discordChannel = e.Message.Channel;
            await e.Message.DeleteAsync(delete);
            await discordChannel.SendMessageAsync(response);
        }

        // If the Message came from a Bot, ignore it.
        if (e.Message.Author.IsBot)
            return;

        // If the User has Reactions enabled, handle them.
        if (user is { ReactedTo: true })
            await ReactionHandler.HandleUserReactions(client, e, user);

        // If the User has Responses enabled, handle them.
        if (user is { RepliedTo: true })
            await ResponseHandler.HandleUserResponses(e, user);
    }

    /// <summary>
    /// Handles the Message deleted event.
    /// </summary>
    /// <param name="client">The Bot.</param>
    /// <param name="e">The Interaction arguments.</param>
    /// <returns></returns>
    public static async Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
    {
        // If the Guild is null, ignore.
        if (e.Guild is null || e.Message.Author is null)
            return;
        
        // Check if the database contains the Guild, Channel, User and User Stats and add them if it doesn't.
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        GuildRow guild = await Guilds.GetGuild(e.Guild.Id);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        ChannelRow channel = await Channels.GetChannel(e.Channel.Id);
        
        if (!await Users.UserExists(e.Message.Author.Id))
            await Users.AddUser(e.Message.Author.Id, e.Message.Author.Username, e.Message.Author.GlobalName);
        UserRow user = await Users.GetUser(e.Message.Author.Id);
        
        if (!await UserStats.StatExists(e.Message.Author.Id, e.Channel.Id, e.Guild.Id))
            await UserStats.AddStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id);
        UserStatRow userStats = await UserStats.GetStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id);

        // Increase the Deleted Messages by one for the user.
        if (guild is { Tracked: true } && channel is { Tracked: true } && user is { Tracked: true })
            await UserStats.ModifyStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id,
                deleted: userStats.DeletedMessages ++);
    }

    /// <summary>
    /// Handles the Message updated event.
    /// </summary>
    /// <param name="client">The Bot.</param>
    /// <param name="e">The Interaction arguments.</param>
    /// <returns></returns>
    public static async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
    {
        if (e.Guild is null || e.Author is null)
            return;
        
        // Check if the database contains the Guild, Channel, User and User Stats and add them if it doesn't.
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        GuildRow guild = await Guilds.GetGuild(e.Guild.Id);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        ChannelRow channel = await Channels.GetChannel(e.Channel.Id);
        
        if (!await Users.UserExists(e.Message.Author.Id))
            await Users.AddUser(e.Message.Author.Id, e.Message.Author.Username, e.Message.Author.GlobalName);
        UserRow user = await Users.GetUser(e.Message.Author.Id);
        
        if (!await UserStats.StatExists(e.Message.Author.Id, e.Channel.Id, e.Guild.Id))
            await UserStats.AddStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id);
        UserStatRow userStats = await UserStats.GetStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id);

        // Increase the Edited Messages by one for the user.
        if (guild is { Tracked: true } && channel is { Tracked: true } && user is { Tracked: true })
            await UserStats.ModifyStat(e.Message.Author.Id, e.Channel.Id, e.Guild.Id,
                edited: userStats.EditedMessages ++);
    }
}