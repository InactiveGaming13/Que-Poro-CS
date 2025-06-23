using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using Octokit;
using QuePoro.Database.Types;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

[SlashCommandGroup("messages", "Message management commands")]
public class MessageManager : ApplicationCommandsModule
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

    [SlashCommandGroup("responses", "Message response management commands")]
    public class MessageResponseManager : ApplicationCommandsModule
    {
        [SlashCommand("list", "Lists the responses")]
        public static async Task ListMessageResponse(InteractionContext e,
            [Option("user", "A specific user to get responses for")]
            DiscordUser? user = null,
            [Option("channel", "A specific channel to get responses for"), ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            string title = "";
            List<ResponseRow> responses = [];

            if (user is null && channel is null)
            {
                title = $"Global responses for {e.User.GlobalName}";
                responses = await Responses.GetUserResponses(e.UserId);
            }
            
            if (user is not null && channel is null)
            {
                title = $"Global responses for {e.User.GlobalName}";
                responses = await Responses.GetUserResponses(user.Id);
            }

            if (user is null && channel is not null)
            {
                title = $"Responses for {e.User.GlobalName} in #{channel.Name}";
                responses = await MessageHandler.GetUserChannelResponses(e.UserId, channel.Id);
            }

            if (user is not null && channel is not null)
            {
                title = $"Responses for {user.GlobalName} in #{channel.Name}";
                responses = await MessageHandler.GetUserChannelResponses(user.Id, channel.Id);
            }

            string description = responses.Aggregate("", (current, response) => current + $"Trigger: {response.TriggerMessage} | Response: {response.ResponseMessage}" + $" | Exact: {response.ExactTrigger} | Enabled: {response.Enabled}\n");

            if (string.IsNullOrEmpty(description))
                description = "No responses found.";

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Title = title,
                Description = description
            }.Build();
            await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }

        [SlashCommand("add", "Adds a response to a message")]
        public static async Task AddMessageResponse(InteractionContext e,
            [Option("message", "The message content to trigger the response")]
            string message,
            [Option("response", "The content of the response")]
            string? response = null,
            [Option("user", "A specific user that the response should only work for")]
            DiscordUser? user = null,
            [Option("channel", "A specific channel that the response should only work in"),
             ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null,
            [Option("exact", "Whether the message should be an exact match or just be inside a message")]
            bool exact = false)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (user is null && response is not null)
            {
                await Responses.AddResponse(e.UserId, message, userId: e.UserId, response: response);
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added response `{response}` to trigger `{message}` to user {e.User.Mention}"));
            }
        }
        
        [SlashCommand("edit", "Edits a response for a message")]
        public static async Task EditMessageResponse(InteractionContext e,
            [Option("id", "The ID of the response to edit")]
            int id,
            [Option("message", "The message to trigger the response")]
            string? message = null,
            [Option("response", "The response for the trigger message")]
            string? response = null,
            [Option("user", "The user that the response will trigger for (leave blank for all users")]
            DiscordUser? user = null,
            [Option("channel", "The channel that the response will work in (leave blank for all channels)"),
             ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null,
            [Option("exact", "Determines if the trigger message must equal the message content")]
            bool? exact = null,
            [Option("enabled", "Determines if the response will be enabled or disabled")]
            bool? enabled = null)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }

        [SlashCommand("remove", "Removes a response from a message")]
        public static async Task RemoveMessageResponse(InteractionContext e,
            [Option("id", "The ID of the response to remove")]
            int id)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        [SlashCommand("enable", "Enables a response for a message")]
        public static async Task EnableMessageResponse(InteractionContext e,
            [Option("id", "The ID of the response to enable")]
            int id)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        [SlashCommand("disable", "Disables a response for a message")]
        public static async Task DisableMessageResponse(InteractionContext e,
            [Option("id", "The ID of the response to disable")]
            int id)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
    }
    
    [SlashCommandGroup("reactions", "Message reaction management commands")]
    public class MessageReactionCommands : ApplicationCommandsModule
    {
        
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