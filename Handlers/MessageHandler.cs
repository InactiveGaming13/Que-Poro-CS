using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace Que_Poro_CS.Handlers;

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

    public static Task MessageDeleted(DiscordClient s, MessageDeleteEventArgs e)
    {
        Console.WriteLine($"Deleted message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }

    public static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }
}