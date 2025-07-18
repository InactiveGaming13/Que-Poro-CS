﻿using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling the tester commands.
/// </summary>
[SlashCommandGroup("testers", "Tester commands to check the bot is responding")]
public class TesterCommands : ApplicationCommandsModule
{
    /// <summary>
    /// A command that sends a test emoji.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    [SlashCommand("emoji", "Sends the test emoji")]
    public async Task Emoji(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        await e.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent("<:test:1341993967046103113>"));
    }
    
    /// <summary>
    /// A command that sends a pong response.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    [SlashCommand("ping", "Sends pong")]
    public async Task Ping(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        await e.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"Pong! ({e.Client.Ping}ms)"));
    }
}