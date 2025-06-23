using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

[SlashCommandGroup("reactions", "Reaction commands")]
public class ReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public async Task AddReaction(InteractionContext e,
        [Option("emoji", "emoji you want to have reacted")] String emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser? user = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        DiscordEmoji discordEmoji = DiscordEmoji.FromUnicode(e.Client, emoji);
        
        if (user == e.User || user == null)
        {
            Console.WriteLine(e.User.Id.GetType());
            Console.WriteLine(emoji);
            await Reactions.AddReaction(e.User.Id, e.User.Id, emoji);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {emoji} to your reactions."));
        }
        
        if (user != null && user != e.User && e.User.IsStaff)
        {
            await Reactions.AddReaction(e.User.Id, user.Id, emoji);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Added {emoji} to {user.Mention} reactions."));
        }
        
        if (user != null && user != e.User && !e.User.IsStaff)
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not authorized to add a reaction to other users."));
    }
}