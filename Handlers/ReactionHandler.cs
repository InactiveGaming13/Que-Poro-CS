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

[SlashCommandGroup("reactions", "Reaction commands")]
public class ReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public async Task AddReaction(InteractionContext e,
        [Option("emoji", "emoji you want to have reacted")] String emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser? user = null,
        [Option("trigger", "A message to trigger the reaction")] string? triggerMessage = null,
        [Option("exact_trigger", "Whether the trigger message should equal the message content")]
        bool exactTrigger = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        DiscordEmoji? discordEmoji = await ReactionHandler.GetEmoji(e.Client, emoji);
        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid!"));
            return;
        }

        UserRow? databaseUser = await Users.GetUser(e.UserId);

        if (databaseUser is null && await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName ?? e.User.Username))
            databaseUser = await Users.GetUser(e.UserId);
        else
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "An unexpected database error occured."));
            return;
        }

        switch (user)
        {
            case null when databaseUser is { Admin: true }:
                if (!await Reactions.AddReaction(e.UserId, discordEmoji.Name, null, triggerMessage, exactTrigger))
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
                if (!await Reactions.AddReaction(e.UserId, discordEmoji.Name, e.UserId, triggerMessage, exactTrigger))
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
                if (!await Reactions.AddReaction(e.UserId, discordEmoji.Name, user.Id, triggerMessage, exactTrigger))
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
    public async Task RemoveReaction(InteractionContext e,
        [Option("emoji", "The emoji you want to remove")] String emoji,
        [Option("user", "The user you want to remove a reaction from")] DiscordUser? user = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        DiscordEmoji? discordEmoji = await ReactionHandler.GetEmoji(e.Client, emoji);

        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid!"));
            return;
        }

        UserRow? databaseUser = await Users.GetUser(e.UserId);
        
        if (databaseUser is null && await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName ?? e.User.Username))
        {
            databaseUser = await Users.GetUser(e.UserId);
        }
        else
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "An unexpected database error occured."));
            return;
        }
        
        Guid? authorEmojiId = await Reactions.GetReactionId(e.UserId, discordEmoji.Name);
        Guid? userEmojiId = user != null ? await Reactions.GetReactionId(user.Id, discordEmoji.Name) : null;

        switch (user)
        {
            case not null when user == e.User && authorEmojiId is not null:
                await Reactions.RemoveReaction((Guid)authorEmojiId);
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Removed {emoji} from your reactions."));
                return;
            
            case not null when user == e.User && authorEmojiId is null:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "That reaction doesn't exist for you."));
                return;
                
            case not null when user != e.User && userEmojiId is not null && databaseUser is { Admin: true }:
                await Reactions.RemoveReaction((Guid)userEmojiId);
                await e.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Removed {emoji} from {user.Mention} reactions."));
                return;
            
            case not null when user != e.User && userEmojiId is null && databaseUser is { Admin: true }:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"That reaction doesn't exist for **{user.GlobalName}**."));
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
        
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        List<ReactionRow> reactions;
        string title;

        switch (user)
        {
            case null:
                reactions = await Reactions.GetReactions(e.UserId);
                title = $"Reactions for {e.User.GlobalName}";
                break;
            
            default:
                reactions = await Reactions.GetReactions(user.Id);
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

public class ReactionHandler()
{
    public static Task<DiscordEmoji?> GetEmoji(DiscordClient client, string emoji)
    {
        DiscordEmoji.TryFromUnicode(emoji, out DiscordEmoji? global);
        if (global is not null) return Task.FromResult(global);
        DiscordEmoji.TryFromGuildEmote(client, (ulong)Convert.ToInt64(emoji.Split(":")[2].Replace(">", "")),
            out DiscordEmoji? guild);
        if (guild is not null) return Task.FromResult(guild);
        DiscordEmoji.TryFromName(client, emoji, out DiscordEmoji? name);
        return Task.FromResult(name ?? null);
    }
    
    private static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }

    public static async Task HandleUserReactions(DiscordClient client, MessageCreateEventArgs e, UserRow user)
    {
        if (!user.ReactedTo)
            return;
        
        List<ReactionRow> userReactions = await Reactions.GetReactions(e.Author.Id);
        
        if (userReactions.Count == 0)
            return;
        
        foreach (ReactionRow reaction in userReactions)
        {
            DiscordEmoji? discordEmoji = null;
            DiscordEmoji.TryFromUnicode(reaction.Emoji, out discordEmoji);
            if (discordEmoji is null)
                DiscordEmoji.TryFromGuildEmote(client,
                    (ulong)Convert.ToInt64(reaction.Emoji.Split(":")[2].Replace(">", "")),
                    out discordEmoji);
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