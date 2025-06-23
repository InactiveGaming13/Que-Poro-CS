using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Types;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

[SlashCommandGroup("responses", "Message response management commands")]
public class ResponseCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a response to a message")]
    public static async Task AddMessageResponse(InteractionContext e,
        [Option("message", "The message content to trigger the response")]
        string message,
        [Option("response", "The content of the response")]
        string? response = null,
        [Option("media_alias", "The alias of the media that should be responded with")]
        string? mediaAlias = null,
        [Option("media_category", "The category of the media that should be responded with")]
        string? mediaCategory = null,
        [Option("user", "A specific user that the response should only work for")]
        DiscordUser? user = null,
        [Option("channel", "A specific channel that the response should only work in"),
         ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null,
        [Option("exact", "Whether the message should be an exact match or just be inside a message")]
        bool exact = false)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        UserRow? databaseUser = await Users.GetUser(e.UserId);
        string responseMessage;

        switch (user)
        {
            case not null when user == e.User &&
                               (response is not null || mediaAlias is not null || mediaCategory is not null):
                await Responses.AddResponse(e.UserId, message, userId: e.UserId, response: response,
                    exactTrigger: exact);
                responseMessage =
                    $"Added response to {(exact ? "exact" : "")} trigger `{message}` with" +
                    $"{(string.IsNullOrEmpty(response) ? "" : $" response `{response}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaAlias) ? "" : $" media alias `{mediaAlias}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaCategory) ? "" : $" media category `{mediaCategory}`")}";
                if (responseMessage.EndsWith(','))
                    responseMessage = responseMessage.Remove(responseMessage.Length - 1);
                responseMessage += " to yourself.";
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseMessage));
                return;

            case not null when user != e.User && response is not null && databaseUser is { Admin: true }:
                await Responses.AddResponse(e.UserId, message, userId: e.UserId, response: response,
                    exactTrigger: exact);
                responseMessage =
                    $"Added response to {(exact ? "exact" : "")} trigger `{message}` with" +
                    $"{(string.IsNullOrEmpty(response) ? "" : $" response `{response}`")}," +
                    $"{(string.IsNullOrEmpty(mediaAlias) ? "" : $" media alias `{mediaAlias}`")}," +
                    $"{(string.IsNullOrEmpty(mediaCategory) ? "" : $" media category `{mediaCategory}`")}";
                if (responseMessage.EndsWith(','))
                    responseMessage = responseMessage.Remove(responseMessage.Length - 1);
                responseMessage += $"to {user.Mention}.";
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseMessage));
                return;

            case not null when user != e.User && databaseUser is { Admin: false }:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not authorized to add responses to other users."));
                return;

            case null when response is not null && databaseUser is { Admin: true }:
                await Responses.AddResponse(e.UserId, message, response: response, mediaAlias: mediaAlias,
                    mediaCategory: mediaCategory, exactTrigger: exact);
                responseMessage =
                    $"Added global response to {(exact ? "exact" : "")} trigger `{message}` with" +
                    $"{(string.IsNullOrEmpty(response) ? "" : $" response `{response}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaAlias) ? "" : $" media alias `{mediaAlias}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaCategory) ? "" : $" media category `{mediaCategory}`.")}";
                if (responseMessage.EndsWith(','))
                    responseMessage = responseMessage.Remove(responseMessage.Length - 1) + ".";
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseMessage));
                return;

            case null when mediaAlias is not null && databaseUser is { Admin: true }:
                await Responses.AddResponse(e.UserId, message, mediaAlias: mediaAlias, exactTrigger: exact);
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added global response to {(exact ? "exact" : "")} trigger `{message}` with media alias `{mediaAlias}`."));
                return;

            case null when mediaCategory is not null && databaseUser is { Admin: true }:
                await Responses.AddResponse(e.UserId, message, mediaCategory: mediaCategory, exactTrigger: exact);
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added global response to {(exact ? "exact" : "")} trigger `{message}` with media category `{mediaCategory}`."));
                return;

            case null when databaseUser is { Admin: false }:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You are not authorized to add global responses."));
                return;

            default:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "An unexpected error occured."));
                return;
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
        string id)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        Guid guid = Guid.Parse(id);
        ResponseRow? response = await Responses.GetResponse(guid);
        if (response is not null)
        {
            await Responses.RemoveResponse(guid);
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Removed reaction: {id}"));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"No reaction was found with ID: {id}"));
    }

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

        string description = responses.Aggregate(
            "", (current, response) => current + 
                                       $"Trigger: {response.TriggerMessage}" +
                                       $"{(string.IsNullOrEmpty(response.ResponseMessage) ? "" : $" | Response: {response.ResponseMessage}")}" +
                                       $"{(string.IsNullOrEmpty(response.MediaAlias) ? "" : $" | Media Alias: {response.MediaAlias}")}" +
                                       $"{(string.IsNullOrEmpty(response.MediaCategory) ? "" : $" | Media Category: {response.MediaCategory}")}" +
                                       $" | Exact: {response.ExactTrigger} | Enabled: {response.Enabled}\n");

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