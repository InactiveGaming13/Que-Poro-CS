using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using dotenv.net;
using QuePoro.Handlers;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro;

internal static class Program
{
    private static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        // Load the environment variables
        DotEnv.Load();

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

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

        if (usingLavalink && lavalinkHost is not null && lavalinkPassword is not null)
        {
            // Sets the lavalink port to default if it wasn't set in the config.
            lavalinkPort ??= "2333";
            
            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = lavalinkHost,
                Port = Convert.ToInt32(lavalinkPort)
            };

            lavalinkConfiguration = new LavalinkConfiguration
            {
                Password = lavalinkPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
                EnableBuiltInQueueSystem = true,
                DefaultVolume = 15
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
        
        // Register ApplicationCommands
        ApplicationCommandsExtension appCommands = discord.UseApplicationCommands();
        appCommands.RegisterGlobalCommands<AdminCommands>();
        appCommands.RegisterGlobalCommands<BannedPhraseCommands>();
        appCommands.RegisterGlobalCommands<ConfigCommands>();
        appCommands.RegisterGlobalCommands<CreateAVcCommands>();
        appCommands.RegisterGlobalCommands<MediaCommands>();
        appCommands.RegisterGlobalCommands<MessageCommands>();
        appCommands.RegisterGlobalCommands<MusicCommands>();
        appCommands.RegisterGlobalCommands<PrivacyCommands>();
        appCommands.RegisterGlobalCommands<ReactionCommands>();
        appCommands.RegisterGlobalCommands<ResponseCommands>();
        appCommands.RegisterGlobalCommands<TempVcCommands>();
        if (config is { TestersEnabled: true })
            appCommands.RegisterGlobalCommands<TesterCommands>();
        else
            appCommands.RegisterGuildCommands<TesterCommands>(1023182344087146546);
        appCommands.RegisterGlobalCommands<VoiceCommands>();
        
        // Handle the bot Ready event
        discord.Ready += async (client, _) =>
        {
            if (usingLavalink && lavalink is not null && lavalinkConfiguration is not null)
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
                Console.WriteLine("Lavalink is disabled");
            
            Console.WriteLine("Checking config...");

            if (config.TempVcEnabled)
                await CreateAVcHandler.ValidateTempVcs(client);

            if (string.IsNullOrEmpty(config.StatusMessage))
            {
                Console.WriteLine("Bot is ready.");
                return;
            }

            ActivityType? activityType = config.StatusType switch
            {
                0 => ActivityType.Playing,
                2 => ActivityType.ListeningTo,
                3 => ActivityType.Watching,
                _ => null
            };

            if (activityType == null)
            {
                Console.WriteLine("Bot is ready.");
                return;
            }
            
            Console.WriteLine("Setting status...");
            await discord.UpdateStatusAsync(new DiscordActivity(config.StatusMessage, (ActivityType)activityType), UserStatus.Online);
            Console.WriteLine("Status set.");
            Console.WriteLine("Bot is ready.");
        };
        
        // Connect to discord and stop the app from closing prematurely.
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}