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
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
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
        
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        GuildRow guild = await Guilds.GetGuild(e.Guild.Id);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        ChannelRow channel = await Channels.GetChannel(e.Channel.Id);
        
        if (!await Users.UserExists(e.Author.Id))
            await Users.AddUser(e.Author.Id, e.Author.Username, e.Author.GlobalName);
        UserRow user = await Users.GetUser(e.Author.Id);
        
        if (!await UserStats.StatExists(e.Author.Id))
            await UserStats.AddStat(e.Author.Id);
        UserStatRow userStats = await UserStats.GetStat(e.Author.Id);
        

        if (e.Message.Content.StartsWith("@ignore", StringComparison.CurrentCultureIgnoreCase))
            return;
        
        Console.WriteLine(
            $"{e.Message.Author.Username} sent a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        
        if (guild is { Tracked: true } && channel is { Tracked: true })
            await Channels.ModifyChannel(e.Channel.Id, messages: channel.Messages + 1);
        
        if (user is { Tracked: true })
            await UserStats.ModifyStat(e.Author.Id, sent: userStats?.SentMessages + 1);

        if (e.Message.Author.IsBot)
        {
            Console.WriteLine("Message came from a bot! Ignoring...");
            return;
        }

        if (user is { ReactedTo: true })
            await ReactionHandler.HandleUserReactions(client, e, user);

        if (user is { RepliedTo: true })
            await ResponseHandler.HandleUserResponses(e, user);
    }

    public static Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
    {
        if (e.Guild is null)
            return Task.CompletedTask;
        
        if (e.Message.Author is not null)
        {
            Console.WriteLine(
                $"{e.Message.Author.GlobalName ?? e.Message.Author.Username} deleted a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
            return Task.CompletedTask;
        }
        
        Console.WriteLine(
            $"A message was deleted in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }

    public static Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
    {
        if (e.Guild is null)
            return Task.CompletedTask;
        
        Console.WriteLine(
            $"{e.Message.Author.Username} updated a message in Guild: {e.Guild.Name} in Channel: {e.Channel.Name}");
        return Task.CompletedTask;
    }
}