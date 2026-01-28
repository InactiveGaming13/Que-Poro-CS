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
        if (e.Guild is null)
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
        DiscordGuild? guild = e.Guild ?? e.Message.Guild;
        // Check if the Guild is null.
        if (guild is null)
            return;
        
        // Check if the database contains the Guild, Channel, User and User Stats and add them if it doesn't.
        if (!await Guilds.GuildExists(guild.Id))
            await Guilds.AddGuild(guild.Id, guild.Name);

        GuildRow tempGuild = await Guilds.GetGuild(guild.Id);
        if (!tempGuild.Name.Equals(guild.Name))
            await Guilds.ModifyGuild(guild.Id, guild.Name);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, guild.Id, e.Channel.Name, e.Channel.Topic);

        await Channels.ModifyChannel(e.Channel.Id, e.Channel.Name, e.Channel.Topic);
        
        if (!await Users.UserExists(e.Author.Id))
            await Users.AddUser(e.Author.Id, e.Author.Username, e.Author.GlobalName);
        UserRow user = await Users.GetUser(e.Author.Id);
        
        if (!await UserStats.StatExists(e.Author.Id, e.Channel.Id, guild.Id))
            await UserStats.AddStat(e.Author.Id, e.Channel.Id, guild.Id);
        UserStatRow userStats = await UserStats.GetUserStat(e.Author.Id, e.Channel.Id, guild.Id);
        
        // If the message starts with “@ignore”, ignore the message.
        if (e.Message.Content.StartsWith("@ignore", StringComparison.CurrentCultureIgnoreCase))
            return;

        bool tracked = await UserStats.GuildChannelUserTracked(guild.Id, e.Channel.Id, e.Author.Id);
        
        // If the User, Channel and Guild has tracking enabled, update the User Stats.
        if (tracked)
            await UserStats.ModifyStat(e.Author.Id, e.Channel.Id, guild.Id, sent: userStats.SentMessages + 1);

        string? delete = await BannedPhraseHandler.HandleBannedPhrases(e.Message.Content);
        if (delete is not null)
        {
            string response = $"Hey {e.Author.Mention}! You can't send that here because {delete}{(char.IsPunctuation(delete, delete.Length-1) ? "." : null)}";
            DiscordChannel discordChannel = e.Message.Channel;
            await e.Message.DeleteAsync(delete);
            await discordChannel.SendMessageAsync(response);
        }

        // If the Message came from a Bot, ignore it.
        if (e.Message.Author.IsBot)
            return;

        // If the User has Reactions enabled, handle them.
        if (user is { ReactedTo: true })
            await MessageReactionHandler.HandleUserReactions(client, e, user);

        // If the User has Responses enabled, handle them.
        if (user is { RepliedTo: true })
            await ResponseHandler.HandleUserResponses(e, user);
    }
}