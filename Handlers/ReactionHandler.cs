using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
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
        DiscordEmoji discordEmoji;
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        DiscordEmoji.TryFromUnicode(emoji, out DiscordEmoji? global);
        if (global is null)
        {
            DiscordEmoji.TryFromGuildEmote(e.Client, (ulong)Convert.ToInt64(emoji.Split(":")[2].Replace(">", "")),
                out DiscordEmoji? guild);
            if (guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "That is an invalid emoji!"));
                return;
            }
            discordEmoji = guild;
        }
        else
            discordEmoji = global;

        UserRow? databaseUser = await Users.GetUser(e.UserId);
        
        if (user == e.User || user == null)
        {
            await Reactions.AddReaction(e.User.Id, e.User.Id, discordEmoji.Name, triggerMessage, exactTrigger);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {emoji} to your reactions."));
            return;
        }
        
        if (user != null && user != e.User && databaseUser is { Admin: true })
        {
            await Reactions.AddReaction(e.User.Id, user.Id, emoji, triggerMessage, exactTrigger);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Added {emoji} to {user.Mention} reactions."));
            return;
        }
        
        if (user != null && user != e.User && databaseUser is { Admin: false })
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not authorized to add a reaction to other users."));
    }
    
    [SlashCommand("remove", "Removes a Reaction from you or a specified user (admin)")]
    public async Task RemoveReaction(InteractionContext e,
        [Option("emoji", "The emoji you want to remove")] String emoji,
        [Option("user", "The user you want to remove a reaction from")] DiscordUser? user = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        DiscordEmoji? discordEmoji = await EmojiHandler.GetEmoji(e.Client, emoji);

        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid!"));
            return;
        }

        UserRow? databaseUser = await Users.GetUser(e.UserId);
        
        if (user == e.User || user == null)
        {
            Guid? emojiId = await Reactions.GetReactionId(e.UserId, discordEmoji.Name);
            if (emojiId is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "That reaction doesn't exist for you."));
                return;
            }
            await Reactions.RemoveReaction((Guid)emojiId);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {emoji} from your reactions."));
            return;
        }
        
        if (user != null && user != e.User && databaseUser is { Admin: true })
        {
            Guid? emojiId = await Reactions.GetReactionId(user.Id, discordEmoji.Name);
            if (emojiId is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"That reaction doesn't exist for **{user.GlobalName}**."));
                return;
            }
            await Reactions.RemoveReaction((Guid)emojiId);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Removed {emoji} from {user.Mention} reactions."));
            return;
        }
        
        if (user != null && user != e.User && databaseUser is { Admin: false })
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not authorized to add a reaction to other users."));
    }

    [SlashCommand("list", "Lists the reactions for a specified user")]
    public static async Task ListReactions(InteractionContext e,
        [Option("user", "The user to list the reactions for")]
        DiscordUser? user = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        List<ReactionRow> reactions = [];
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

public class EmojiHandler()
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
}