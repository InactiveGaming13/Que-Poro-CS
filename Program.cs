using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using dotenv.net;
using QuePoro.Handlers;

namespace QuePoro;

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

        string? botToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        string? lavalinkHost = Environment.GetEnvironmentVariable("LAVALINK_HOST");
        string? lavalinkPort = Environment.GetEnvironmentVariable("LAVALINK_PORT");
        string? lavalinkPassword = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD");

        LavalinkExtension? lavalink = null;
        LavalinkConfiguration? lavalinkConfiguration = null;

       bool usingLavalink = lavalinkHost != null && lavalinkPassword != null;
       Environment.SetEnvironmentVariable("USING_LAVALINK", Convert.ToString(usingLavalink));
        
        // Check if the DISCORD_TOKEN variable exists and exit if it doesn't.
        if (botToken == null)
        {
            Console.WriteLine("Please set environment variable 'DISCORD_TOKEN'.");
            Environment.Exit(1);
        }
        
        // Define the discord client with its various options
        DiscordClient discord = new(new DiscordConfiguration()
        {
            Token = botToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        if (usingLavalink)
        {
            // Sets the lavalink port to default if it wasn't set in the config.
            lavalinkPort ??= "2333";
            
            var endpoint = new ConnectionEndpoint
            {
                Hostname = lavalinkHost,
                Port = Convert.ToInt32(lavalinkPort)
            };

            lavalinkConfiguration = new LavalinkConfiguration
            {
                Password = lavalinkPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
                EnableBuiltInQueueSystem = true
            };

            // Enable voice for the bot
            lavalink = discord.UseLavalink();   
        }
        
        // Set functions for various events
        discord.MessageCreated += MessageHandler.MessageCreated;
        discord.MessageDeleted += MessageHandler.MessageDeleted;
        discord.MessageUpdated += MessageHandler.MessageUpdated;
        discord.GuildMemberAdded += GuildHandler.MemberAdded;
        discord.VoiceStateUpdated += VoiceHandler.VoiceStateUpdated;
        discord.VoiceChannelStatusUpdated += VoiceHandler.VoiceChannelStatusUpdated;
        
        // Register ApplicationCommands
        ApplicationCommandsExtension appCommands = discord.UseApplicationCommands();
        appCommands.RegisterGlobalCommands<AdminCommands>();
        appCommands.RegisterGlobalCommands<ConfigCommands>();
        appCommands.RegisterGlobalCommands<CreateAVcCommands>();
        appCommands.RegisterGlobalCommands<MessageManager>();
        appCommands.RegisterGlobalCommands<MusicCommands>();
        appCommands.RegisterGlobalCommands<ReactionCommands>();
        appCommands.RegisterGlobalCommands<TempVcCommands>();
        appCommands.RegisterGuildCommands<TesterCommands>(1023182344087146546);
        appCommands.RegisterGlobalCommands<VoiceCommands>();
        
        // Handle the bot Ready event
        discord.Ready += async (s, e) =>
        {
            await discord.UpdateStatusAsync(new DiscordActivity("with my testes", ActivityType.Playing), UserStatus.Online);
            if (usingLavalink)
            {
                try
                {
                    await lavalink.ConnectAsync(lavalinkConfiguration);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Failed to connect to the Lavalink server!");
                    Console.WriteLine(exception);
                }
            }
            else
                Console.WriteLine("Not using lavalink");
            Console.WriteLine("Bot is ready.");
        };
        
        // Connect to discord and stop the app from closing prematurely
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}