using DisCatSharp;
using DisCatSharp.Enums;
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
        DotEnv.Load();
        
        if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") == null)
        {
            Console.WriteLine("Please set environment variable 'DISCORD_TOKEN'.");
        }
        
        DiscordClient discord = new(new DiscordConfiguration()
        {
            Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });
        
        discord.MessageCreated += async (s, e) =>
        {
            if (e.Message.Content.ToLower().StartsWith("ping"))
            {
                await e.Message.RespondAsync("pong");
            }

            if (e.Message.Content.ToLower() == "and so it begins...")
            {
                await e.Message.RespondAsync("Nuh uh, you procrastinating dick.");
            }
        };
        
        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}