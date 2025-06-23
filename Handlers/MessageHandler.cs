using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using QuePoro.Database.Types;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

[SlashCommandGroup("messages", "Message management commands")]
public class MessageCommands : ApplicationCommandsModule
{
    [SlashCommand("clear", "Clears (purges) a set amount of messages")]
    public static async Task ClearMessages(InteractionContext e,
        [Option("amount", "The amount of messages to clear (defaults to 3)"), MinimumValue(1), MaximumValue(100)]
        int amount = 3,
        [Option("channel", "The channel to clear (defaults to current channel)"), ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null!
    )
    {
        channel ??= e.Channel;

        await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(amount),
            $"Purged by User: {e.User.GlobalName}");
        string clearedMessage = amount == 1 ? "Cleared 1 message." : $"Cleared {amount} messages.";
        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(clearedMessage));
        Thread.Sleep(2000);
        await e.DeleteResponseAsync();
    }
}

public static class MessageHandler
{
    public static async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
    {
        if (e.Guild is null)
            return;
        
        GuildRow? guild = await Guilds.GetGuild(e.Guild.Id);
        ChannelRow? channel = await Channels.GetChannel(e.Channel.Id);
        UserRow? user = await Users.GetUser(e.Author.Id);
        UserStatRow? userStats = await UserStats.GetUser(e.Author.Id);

        if (guild == null)
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);

        if (channel == null)
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);

        if (user == null)
            await Users.AddUser(e.Author.Id, e.Author.Username, e.Author.GlobalName ?? e.Author.Username);

        if (userStats == null)
            await UserStats.AddUser(e.Author.Id);

        if (e.Message.Content.StartsWith("@ignore", StringComparison.CurrentCultureIgnoreCase))
            return;

        guild = await Guilds.GetGuild(e.Guild.Id);
        channel = await Channels.GetChannel(e.Channel.Id);
        user = await Users.GetUser(e.Author.Id);
        userStats = await UserStats.GetUser(e.Author.Id);
        
        Console.WriteLine(
            $"{e.Message.Author.Username} sent a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        
        if (guild is { Tracked: true } && channel is { Tracked: true })
            await Channels.ModifyChannel(e.Channel.Id, messages: channel?.Messages + 1);
        
        if (user is { Tracked: true })
            await UserStats.ModifyUser(e.Author.Id, sent: userStats?.SentMessages + 1);

        if (e.Message.Author.IsBot)
        {
            Console.WriteLine("Message came from a bot! Ignoring...");
            return;
        }

        if (user is { ReactedTo: true })
            await HandleUserReactions(client, e, user);

        if (user is { RepliedTo: false })
            await HandleUserResponses(e, user);
    }

    public static Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
    {
        if (e.Message.Author is not null)
        {
            Console.WriteLine(
                $"{e.Message.Author.GlobalName} deleted a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
            return Task.CompletedTask;
        }
        Console.WriteLine(
            $"A message was deleted in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }

    public static Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
    {
        Console.WriteLine(
            $"{e.Message.Author.Username} updated a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }

    private static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }

    private static async Task HandleUserReactions(DiscordClient client, MessageCreateEventArgs e, UserRow user)
    {
        if (!user.ReactedTo)
            return;
        
        List<ReactionRow> userReactions = await Reactions.GetReactions(e.Author.Id);
        foreach (ReactionRow reaction in userReactions)
        {
            DiscordEmoji discordEmoji;
            DiscordEmoji.TryFromUnicode(reaction.Emoji, out DiscordEmoji? global);
            if (global is null)
            {
                DiscordEmoji.TryFromGuildEmote(client, (ulong)Convert.ToInt64(reaction.Emoji.Split(":")[2].Replace(">", "")),
                    out DiscordEmoji? guild);
                if (guild is null)
                    return;
                discordEmoji = guild;
            }
            else
                discordEmoji = global;
            
            switch (reaction.TriggerMessage)
            {
                case not null when reaction is { ExactTrigger: false } && 
                                   !e.Message.Content.Contains(reaction.TriggerMessage, StringComparison.CurrentCultureIgnoreCase):
                case not null when reaction is { ExactTrigger: true } &&
                               !reaction.TriggerMessage.ToLower().Equals(e.Message.Content.ToLower()):
                    continue;
                default:
                    await AddMessageReaction(e, discordEmoji);
                    break;
            }
        }
    }

    private static async Task HandleUserResponses(MessageCreateEventArgs e, UserRow user)
    {
        if (!user.RepliedTo)
            return;

        List<ResponseRow> responses = await GetUserChannelResponses(user.Id, e.Channel.Id);
        Console.WriteLine(responses.Count);
        foreach (ResponseRow response in responses)
        {
            Console.WriteLine(response.TriggerMessage);
            Console.WriteLine(response.ResponseMessage);
        }
    }

    public static async Task<List<ResponseRow>> GetUserChannelResponses(ulong userId, ulong channelId)
    {
        List<ResponseRow> globalResponses = await Responses.GetGlobalResponses();
        List<ResponseRow> channelResponses = await Responses.GetChannelResponses(channelId);
        List<ResponseRow> userResponses = await Responses.GetUserResponses(userId);
        List<ResponseRow> userChannelResponses = channelResponses.Intersect(userResponses).ToList();
        Console.WriteLine(userChannelResponses.Count);
        globalResponses.ForEach(row => userChannelResponses.Add(row));
        Console.WriteLine(userChannelResponses.Count);
        return userChannelResponses;
    }
}