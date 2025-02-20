using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

public class ReactionHandler
{
    public static async Task AddUserReaction(InteractionContext ctx, DiscordUser user, string emoji)
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