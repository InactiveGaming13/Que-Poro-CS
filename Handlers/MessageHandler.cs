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
public class MessageManager : ApplicationCommandsModule
{
    [SlashCommand("clear", "Clears (purges) a set amount of messages")]
    public static async Task ClearMessages(InteractionContext ctx,
        [Option("amount", "The amount of messages to clear (defaults to 3)"), MinimumValue(1), MaximumValue(100)]
        int amount = 3,
        [Option("channel", "The channel to clear (defaults to current channel)"), ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null!
    )
    {
        if (channel == null)
            channel = ctx.Channel;

        await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(amount),
            $"Purged by User: {ctx.User.GlobalName}");
        string clearedMessage = amount == 1 ? "Cleared 1 message." : $"Cleared {amount} messages.";
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(clearedMessage));
        Thread.Sleep(2000);
        await ctx.DeleteResponseAsync();
    }

    [SlashCommandGroup("responses", "Message response management commands")]
    public class MessageResponseManager : ApplicationCommandsModule
    {
        [SlashCommand("list", "Lists the responses")]
        public static async Task ListMessageResponse(InteractionContext ctx,
            [Option("user", "A specific user to get responses for")]
            DiscordUser? user = null,
            [Option("channel", "A specific channel to get responses for"), ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            string username = user?.GlobalName ?? "**all users**";
            string channelMention = channel?.Mention ?? "**all channels**";

            // DiscordUser discordUser = await ctx.Client.GetUserAsync(id);
            // \n{discordUser.Mention}

            DiscordEmbed embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Title = $"Responses for {username} in {channelMention}",
                Description = $"Not yet implemented..."
            }.Build();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
        }

        [SlashCommand("add", "Adds a response to a message")]
        public static async Task AddMessageResponse(InteractionContext ctx,
            [Option("message", "The message content to trigger the response")]
            string message,
            [Option("response", "The content of the response")]
            string response,
            [Option("user", "A specific user that the response should only work for")]
            DiscordUser? user = null,
            [Option("channel", "A specific channel that the response should only work in"),
             ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null,
            [Option("exact", "Whether the message should be an exact match or just be inside a message")]
            bool exact = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        [SlashCommand("edit", "Edits a response for a message")]
        public static async Task EditMessageResponse(InteractionContext ctx,
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
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }

        [SlashCommand("remove", "Removes a response from a message")]
        public static async Task RemoveMessageResponse(InteractionContext ctx,
            [Option("id", "The ID of the response to remove")]
            int id)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        [SlashCommand("enable", "Enables a response for a message")]
        public static async Task EnableMessageResponse(InteractionContext ctx,
            [Option("id", "The ID of the response to enable")]
            int id)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        [SlashCommand("disable", "Disables a response for a message")]
        public static async Task DisableMessageResponse(InteractionContext ctx,
            [Option("id", "The ID of the response to disable")]
            int id)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.EditResponseAsync(
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
    public static async Task MessageCreated(DiscordClient s, MessageCreateEventArgs e)
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

        if (e.Message.Content.ToLower().StartsWith("@ignore"))
            return;
        
        channel = await Channels.GetChannel(e.Channel.Id);
        userStats = await UserStats.GetUser(e.Author.Id);
        
        Console.WriteLine(
            $"{e.Message.Author.Username} sent a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        
        await Channels.ModifyChannel(e.Channel.Id, messages: channel?.Messages + 1);
        await UserStats.ModifyUser(e.Author.Id, sent: userStats?.SentMessages + 1);

        if (e.Message.Author.IsBot)
        {
            Console.WriteLine("Message came from a bot! Ignoring...");
            return;
        }

        List<ReactionRow> userReactions = await Reactions.GetReactions(e.Author.Id);
        foreach (ReactionRow reaction in userReactions)
        {
            Console.WriteLine($"User: {e.Author.GlobalName} | Reaction: {reaction.Emoji}");
        }

        if (Convert.ToString(e.Message.Author.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await AddMessageReaction(e, DiscordEmoji.FromUnicode("\U0001F913"));
        }

        switch (e.Message.Content.ToLower())
        {
            case "cock":
                DiscordMember member = await e.Author.ConvertToMember(e.Guild);
                if (member.Roles.Any(role => role.Name.ToLower().Contains("horny")))
                {
                    await e.Message.RespondAsync("Stop being a horny cunt.");
                    return;
                }
                await e.Message.RespondAsync("I will give you the horny role.");
                break;
        }

        if (e.Message.Content.Contains("moyai", StringComparison.CurrentCultureIgnoreCase) || e.Message.Content.Contains("moai", StringComparison.CurrentCultureIgnoreCase))
            await AddMessageReaction(e, DiscordEmoji.FromUnicode("\U0001F5FF"));

        if (e.Message.Content.Contains("balls", StringComparison.CurrentCultureIgnoreCase))
            await e.Message.RespondAsync($"Hey {e.Author.Mention}, nice balls bro!");
        
        if (e.Message.Content.Contains("cupbop", StringComparison.CurrentCultureIgnoreCase) ||
            e.Message.Content.Contains("are you hungry", StringComparison.CurrentCultureIgnoreCase) ||
            e.Message.Content.Contains("i am hungry", StringComparison.CurrentCultureIgnoreCase))
            await e.Message.RespondAsync(
            "https://cdn.discordapp.com/attachments/940110680055492638/1364827704788254781/XKrpwRr.jpg?ex=680b165a&is=6809c4da&hm=375ce060dd40e1e5b6e3100989665b2153a5934bc9ac85b7117263f84b91b6e5&");
    }

    public static Task MessageDeleted(DiscordClient s, MessageDeleteEventArgs e)
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

    public static Task MessageUpdated(DiscordClient s, MessageUpdateEventArgs e)
    {
        Console.WriteLine(
            $"{e.Message.Author.Username} updated a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }

    public static async Task AddMessageReaction(MessageCreateEventArgs e, DiscordEmoji emoji)
    {
        await e.Message.CreateReactionAsync(emoji);
    }
}