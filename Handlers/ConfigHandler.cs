using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("config", "Configuration commands")]
public class ConfigCommands : ApplicationCommandsModule
{
    [SlashCommand("response", "Sets weather or not the bot responds to you")]
    public async Task Response(InteractionContext ctx, [Option("value", "True for response, false for silence", false)] bool silent = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(silent
                ? "The bot will respond to your messages."
                : "The bot will no longer respond to your messages."));
    }
}