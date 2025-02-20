using System.Net.NetworkInformation;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace Que_Poro_CS;

public static class MessageHandler
{
    public static async Task MessageCreated(DiscordClient s, MessageCreateEventArgs e)
    {
        Console.WriteLine($"New message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name} with content: {e.Message.Content}");

        if (e.Message.Author.IsBot)
        {
            Console.WriteLine("Message came from a bot! Ignoring...");
            return;
        }

        switch (e.Message.Content.ToLower())
        {
            case "cock":
                await e.Message.RespondAsync("I will give you the horny role.");
                break;
            
            case "ping":
                await e.Message.RespondAsync("Pong!");
                break;
        }

        if (e.Message.Content.ToLower().Contains("moyai"))
        {
            await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("\U0001F5FF"));
        }

        if (e.Message.Content.ToLower().Contains("balls"))
        {
            await e.Message.RespondAsync($"Hey {e.Author.Mention}, nice balls bro!");
        }
    }

    public static async Task MessageDeleted(DiscordClient s, MessageDeleteEventArgs e)
    {
        Console.WriteLine($"Deleted message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
    }

    public static async Task AddReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        e.Message.CreateReactionAsync(emoji);
    }
}