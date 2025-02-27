using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace QuePoro.Handlers;

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

        if (Convert.ToString(e.Message.Author.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await AddMessageReaction(e, DiscordEmoji.FromUnicode("\U0001F913"));
        }

        switch (e.Message.Content.ToLower())
        {
            case "cock":
                DiscordMember member = await e.Author.ConvertToMember(e.Guild);
                foreach (var role in member.Roles)
                {
                    if (role.Name.ToLower() == "horny")
                    {
                        await e.Message.RespondAsync("Stop being a horny cunt.");
                        return;
                    }
                }
                await e.Message.RespondAsync("I will give you the horny role.");
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

    public static Task MessageUpdated(DiscordClient s, MessageUpdateEventArgs e)
    {
        Console.WriteLine($"Updated message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name} with content: {e.Message.Content}");
        return Task.CompletedTask;
    }

    public static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }
}