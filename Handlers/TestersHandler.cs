using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("testers", "Tester commands to check the bot is responding")]
public class TesterCommands : ApplicationCommandsModule
{
    [SlashCommand("emoji", "Sends the test emoji")]
    public async Task Emoji(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("<:test:1341993967046103113>"));
    }
    
    [SlashCommand("ping", "Sends pong")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("Pong!"));
    }
}