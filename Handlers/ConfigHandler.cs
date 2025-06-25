using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace QuePoro.Handlers;

[SlashCommandGroup("config", "Configuration commands")]
public class ConfigCommands : ApplicationCommandsModule
{
    [SlashCommand("response", "Sets weather or not the bot responds to you")]
    public async Task Response(InteractionContext e, [Option("value", "True for response, false for silence")] bool silent = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent(silent
                ? "The bot will respond to your messages."
                : "The bot will no longer respond to your messages."));
    }
}