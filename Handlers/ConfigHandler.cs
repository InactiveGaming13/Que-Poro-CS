using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Config commands.
/// </summary>
[SlashCommandGroup("config", "Configuration commands")]
public class ConfigCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Sets whether the bot Responds to a User.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="respond">Whether to Respond.</param>
    [SlashCommand("response", "Sets weather or not the bot responds to you")]
    public async Task Response(InteractionContext e, 
        [Option("value", "True for response, false for silence")] bool respond = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow user = await Users.GetUser(e.UserId);

        if (respond.Equals(user.RepliedTo))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot already {(respond ? "responds" : "doesn't respond")} to you."));
            return;
        }

        await Users.ModifyUser(e.UserId, e.User.GlobalName, repliedTo: respond);
        
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"The bot will {(respond ? "now" : "no longer")} respond to your messages."));
    }
    
    /// <summary>
    /// Sets whether the bot Reacts to a User.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="react">Whether to React.</param>
    [SlashCommand("react", "Sets weather or not the bot replies to you")]
    public async Task React(InteractionContext e, 
        [Option("value", "True for response, false for silence")] bool react = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow user = await Users.GetUser(e.UserId);

        if (react.Equals(user.ReactedTo))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot already {(react ? "reacts" : "doesn't react")} to you."));
            return;
        }

        await Users.ModifyUser(e.UserId, e.User.GlobalName, reactedTo: react);
        
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bot will {(react ? "now" : "no longer")} react to your messages."));
    }
}