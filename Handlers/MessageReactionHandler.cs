using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

[SlashCommandGroup("message_reactions", "Message Reaction commands")]
public class MessageReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public static async Task AddMessageReaction(InteractionContext e,
        [Option("emoji", "emoji you want to have reacted")] string emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser? user = null,
        [Option("trigger", "A message to trigger the reaction")] string? triggerMessage = null,
        [Option("exact_trigger", "Whether the trigger message should equal the message content")]
        bool exactTrigger = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        DiscordEmoji? discordEmoji = MessageReactionHandler.GetEmoji(e.Client, emoji);
        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid!"));
            return;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow databaseUser = await Users.GetUser(e.UserId);

        switch (user)
        {
            case null when databaseUser is { Admin: true }:
                if (!await MessageReactions.AddMessageReaction(e.UserId, discordEmoji.Name, null, triggerMessage, exactTrigger))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added {emoji}{(triggerMessage is not null 
                        ? $" with {(exactTrigger ? "exact" : "")} trigger message `{triggerMessage}`" 
                        : "")} to global reactions."));
                return;
                
            case null when databaseUser is { Admin: false}:
            case not null when user == e.User:
                if (!await MessageReactions.AddMessageReaction(e.UserId, discordEmoji.Name, e.UserId, triggerMessage, exactTrigger))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added {emoji}{(triggerMessage is not null 
                        ? $" with {(exactTrigger ? "exact" : "")} trigger message `{triggerMessage}`" 
                        : "")} to your reactions."));
                return;

            case not null when user != e.User && databaseUser is { Admin: true }:
                if (!await MessageReactions.AddMessageReaction(e.UserId, discordEmoji.Name, user.Id, triggerMessage, exactTrigger))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added {emoji}{(triggerMessage is not null 
                        ? $" with {(exactTrigger ? "exact" : "")} trigger message `{triggerMessage}`" 
                        : "")} to {user.Mention} reactions."));
                return;
            
            case not null when user != e.User && databaseUser is { Admin: false }:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not authorized to add reactions to other users."));
                return;
            
            default:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "An unexpected error occured."));
                return;
        }
    }
    
    [SlashCommand("remove", "Removes a Reaction from you or a specified user (admin)")]
    public static async Task RemoveReaction(InteractionContext e,
        [Option("emoji", "The emoji of a reaction to remove")] string emoji,
        [Option("user", "The user of a reaction to remove")] DiscordUser? user = null,
        [Option("trigger", "The trigger message of the reaction to remove")] string? trigger = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        DiscordEmoji? discordEmoji = MessageReactionHandler.GetEmoji(e.Client, emoji);

        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid!"));
            return;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow databaseUser = await Users.GetUser(e.UserId);

        if (!databaseUser.Admin && user is null)
            user = e.User;
        
        if (!await MessageReactions.MessageReactionExists(emoji, user?.Id, trigger))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "A reaction doesn't exist with the supplied parameters."));
            return;
        }
        
        Guid emojiId = await MessageReactions.GetMessageReactionId(discordEmoji.Name, user?.Id, trigger);

        switch (user)
        {
            case not null when user == e.User:
                await MessageReactions.RemoveMessageReaction(emojiId);
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Removed {emoji} from your reactions."));
                return;
                
            case not null when user != e.User && databaseUser is { Admin: true }:
                await MessageReactions.RemoveMessageReaction(emojiId);
                await e.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Removed {emoji} from {user.Mention} reactions."));
                return;
            
            case not null when user != e.User && databaseUser is { Admin: false }:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not authorized to remove reactions from other users."));
                return;
            
            default:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "An unexpected error occured."));
                return;
        }
    }

    [SlashCommand("list", "Lists the reactions for a specified user")]
    public static async Task ListReactions(InteractionContext e,
        [Option("user", "The user to list the reactions for")]
        DiscordUser? user = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        List<MessageReactionRow> reactions;
        string title;

        switch (user)
        {
            case null:
                reactions = await MessageReactions.GetMessageReactions(e.UserId);
                title = $"Reactions for {e.User.GlobalName}";
                break;
            
            default:
                reactions = await MessageReactions.GetMessageReactions(user.Id);
                title = $"Reactions for {user.GlobalName}";
                break;
        }

        string description = reactions.Aggregate("",
            (current, reaction) =>
                current +
                $"Emoji: {reaction.Emoji} | Trigger: {reaction.TriggerMessage ??= "None"} | Exact Trigger: {reaction.ExactTrigger}\n");
        
        DiscordEmbed embedBuilder = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Blue,
            Title = title,
            Description = description
        }.Build();
        await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
    }
}

public static class MessageReactionHandler
{
    public static DiscordEmoji? GetEmoji(DiscordClient client, string emoji)
    {
        DiscordEmoji.TryFromUnicode(emoji, out DiscordEmoji? global);
        if (global is not null) return global;
        DiscordEmoji.TryFromGuildEmote(client, (ulong)Convert.ToInt64(emoji.Split(":")[2].Replace(">", "")),
            out DiscordEmoji? guild);
        if (guild is not null) return guild;
        DiscordEmoji.TryFromName(client, emoji, out DiscordEmoji? name);
        return name;
    }
    
    private static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }

    public static async Task HandleUserReactions(DiscordClient client, MessageCreateEventArgs e, UserRow user)
    {
        if (!user.ReactedTo)
            return;
        
        List<MessageReactionRow> userReactions = await MessageReactions.GetMessageReactions(e.Author.Id);
        
        if (userReactions.Count == 0)
            return;
        
        foreach (MessageReactionRow reaction in userReactions)
        {
            DiscordEmoji? discordEmoji = GetEmoji(client, reaction.Emoji);
            
            if (discordEmoji is null)
                return;
            
            switch (reaction.TriggerMessage)
            {
                case not null when reaction is { ExactTrigger: false } && 
                                   !e.Message.Content.Contains(reaction.TriggerMessage, StringComparison.CurrentCultureIgnoreCase):
                case not null when reaction is { ExactTrigger: true } &&
                                   !reaction.TriggerMessage.ToLower().Equals(e.Message.Content.ToLower()):
                    continue;
                default:
                    await AddMessageReaction(e, discordEmoji);
                    break;
            }
        }
    }
}