using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

public class AdminHandler
{
    public static async Task AddUserAdmin(InteractionContext ctx, DiscordUser user)
    {
        foreach (var VARIABLE in ctx.Member.Roles)
        {
            Console.WriteLine(VARIABLE.ToString());
        }
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = $"Added {user.Mention} to the admin list."
            });
    }
    
    public static async Task RemoveUserAdmin(InteractionContext ctx, DiscordUser user)
    {
        foreach (var VARIABLE in ctx.Member.Roles)
        {
            Console.WriteLine(VARIABLE.ToString());
        }
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = $"Removed {user.Mention} from the admin list."
            });
    }
}