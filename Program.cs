using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using dotenv.net;
using Que_Poro_CS.Handlers;

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

        var endpoint = new ConnectionEndpoint
        {
            Hostname = Environment.GetEnvironmentVariable("LAVALINK_HOST"),
            Port = Convert.ToInt32(Environment.GetEnvironmentVariable("LAVALINK_PORT"))
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD"),
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint,
            EnableBuiltInQueueSystem = true
        };

        // Enable voice for the bot
        var lavalink = discord.UseLavalink();
        
        // Set functions for various events
        discord.MessageCreated += MessageHandler.MessageCreated;
        discord.MessageDeleted += MessageHandler.MessageDeleted;
        discord.MessageUpdated += MessageHandler.MessageUpdated;
        discord.GuildMemberAdded += GuildHandler.MemberAdded;
        discord.VoiceStateUpdated += VoiceHandler.VoiceStateUpdated;
        
        // Register ApplicationCommands
        ApplicationCommandsExtension appCommands = discord.UseApplicationCommands();
        appCommands.RegisterGlobalCommands<AdminCommands>();
        appCommands.RegisterGlobalCommands<ConfigCommands>();
        appCommands.RegisterGlobalCommands<CreateAVcCommands>();
        appCommands.RegisterGlobalCommands<MusicCommands>();
        appCommands.RegisterGlobalCommands<ReactionCommands>();
        appCommands.RegisterGlobalCommands<TesterCommands>();
        appCommands.RegisterGlobalCommands<VoiceCommands>();
        
        // Handle the bot Ready event
        discord.Ready += async (s, e) =>
        {
            await lavalink.ConnectAsync(lavalinkConfig);
            await discord.UpdateStatusAsync(new DiscordActivity("with my C through balls", ActivityType.Playing), UserStatus.Online);
            Console.WriteLine("Bot is ready.");
        };
        
        // Connect to discord and stop the application from closing prematurely
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}