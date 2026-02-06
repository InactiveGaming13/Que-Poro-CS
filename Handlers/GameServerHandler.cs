using System.Diagnostics;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Game Servers.
/// </summary>
[SlashCommandGroup("game_servers", "Game Server management commands")]
public class GameServerCommands : ApplicationCommandsModule
{
    //TODO: Add the Discord commands once the DB interaction is done.
    [SlashCommand("add", "Adds a Game Server to the bot (bot admin)")]
    public static async Task AddGameServer(InteractionContext e,
        [Option("proc_id", "The Unix Process ID.")]
        string procId,
        [Option("name", "A friendly identifier for the server")]
        string serverName,
        [Option("description", "A friendly description for the server")]
        string serverDescription,
        [Option("restartable", "Determines if the bot can restart this server")]
        bool restartable,
        [Option("restart_method", "The keybind(s) to restart the server")]
        string restartMethod,
        [Option("shutdown_method", "The keybind(s) to shut down the server")]
        string shutdownMethod,
        [Option("screen_name", "The name of the screen the server is running within")]
        string? screenName = null,
        [Option("broadcast_method", "The command used to say something in game chat")]
        string? broadcastMethod = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (e.Guild is null || e.Member is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I do not work in DMs."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
        {
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not authorised to use this command."));
            return;
        }

        UserRow userRow = await Users.GetUser(e.UserId);
        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("You are not authorised to use this command."));
            return;
        }

        // Checks if the Proc ID exists on the system.
        ProcessStartInfo processIdStartInfo = new("/usr/bin/ps", $"-p {procId}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

        Process processIdCheck = new();
        processIdCheck.StartInfo = processIdStartInfo;
        processIdCheck.Start();

        string processIdOutput = await processIdCheck.StandardOutput.ReadToEndAsync();
        string[] processIdResults = processIdOutput.Split("\n");
        string processIdResult = processIdResults[^2];
        
        if (processIdResults.Length <= 2 || string.IsNullOrWhiteSpace(processIdResult))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Process ID doesn't exist."));
            return;
        }
        
        // Checks if the screen exists (if it isn't null).
        if (screenName is not null)
        {
            ProcessStartInfo screenStartInfo = new("/usr/bin/screen", "-ls")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process screen = new();
            screen.StartInfo = screenStartInfo;
            screen.Start();

            string screenOutput = await screen.StandardOutput.ReadToEndAsync();
            string[] screenResults = screenOutput.Split("\n");
            Console.WriteLine(screenName);
            Console.WriteLine(screenOutput);
            if (!screenResults.Where(line => !string.IsNullOrWhiteSpace(line)).Any(line => line.Contains(screenName)))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Screen name doesn't exist."));
                return;
            }
        }

        await GameServers.AddGameServer(procId, serverName, serverDescription, restartable, screenName, restartMethod,
            shutdownMethod, broadcastMethod);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
    }
}