using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.VoiceNext;
using dotenv.net;

namespace Que_Poro_CS;

internal class Program
{
    static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        // Load the environment variables
        DotEnv.Load();
        
        // Check if the DISCORD_TOKEN variable exists and exit if it doesn't
        if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") == null)
        {
            Console.WriteLine("Please set environment variable 'DISCORD_TOKEN'.");
            System.Environment.Exit(1);
        }
        
        // Define the discord client with its various options
        DiscordClient discord = new(new DiscordConfiguration()
        {
            Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        // Enable voice for the bot
        discord.UseVoiceNext();
        
        // Set functions for various events
        discord.MessageCreated += MessageHandler.MessageCreated;
        discord.GuildMemberAdded += GuildHandler.MemberAdded;
        
        // Register ApplicationCommands
        ApplicationCommandsExtension appCommands = discord.UseApplicationCommands();
        appCommands.RegisterGlobalCommands<VoiceCommands>();
        appCommands.RegisterGlobalCommands<TesterCommands>();
        appCommands.RegisterGlobalCommands<ConfigCommands>();
        appCommands.RegisterGlobalCommands<ReactionCommands>();
        
        // Handle the bot Ready event
        discord.Ready += async (s, e) =>
        {
            await discord.UpdateStatusAsync(new DiscordActivity("With my C through balls", ActivityType.Playing), UserStatus.Online);
            Console.WriteLine("Bot is ready.");
        };
        
        // Connect to discord and stop the application from closing prematurely
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}