using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("reactions", "Reaction commands")]
public class ReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public async Task AddReaction(InteractionContext ctx,
        [Option("emoji", "emoji you want to have reacted")] String emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser user = null)
    {
        if (user != null && ctx.User != user && !ctx.User.IsStaff)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"You are not authorized to add a reaction to other users."
                });
        } else if (ctx.User == user || user == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"Added {emoji} to your reactions."
                });
        } else if (ctx.User != user && ctx.User.IsStaff)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"Added {emoji} to {user.Mention} reactions."
                });
        }
    }
}