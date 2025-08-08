using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
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
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow databaseUser = await Users.GetUser(e.UserId);
        
        string responseMessage;

        switch (user)
        {
            case not null when user == e.User &&
                               (response is not null || mediaAlias is not null || mediaCategory is not null):
                if (!await Responses.AddResponse(e.UserId, message, userId: e.UserId, response: response,
                        mediaAlias: mediaAlias, mediaCategory: mediaCategory, channelId: channel?.Id, exactTrigger: exact))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                responseMessage =
                    $"Added response to {(exact ? "exact" : "")} trigger `{message}` with" +
                    $"{(string.IsNullOrEmpty(response) ? "" : $" response `{response}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaAlias) ? "" : $" media alias `{mediaAlias}`,")}" +
                    $"{(string.IsNullOrEmpty(mediaCategory) ? "" : $" media category `{mediaCategory}`")}" +
                    $"{(string.IsNullOrEmpty(channel?.Name) ? "" : $" chanel lock `{channel.Mention}`")}";
                if (responseMessage.EndsWith(','))
                    responseMessage = responseMessage.Remove(responseMessage.Length - 1);
                responseMessage += " to yourself.";
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseMessage));
                return;

            case not null when user != e.User && response is not null && databaseUser is { Admin: true }:
                if (!await Responses.AddResponse(e.UserId, message, userId: e.UserId, response: response,
                        channelId: channel?.Id, mediaAlias: mediaAlias, mediaCategory: mediaCategory, exactTrigger: exact))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
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
                if (!await Responses.AddResponse(e.UserId, message, response: response, mediaAlias: mediaAlias,
                        mediaCategory: mediaCategory, exactTrigger: exact))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
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
                if (!await Responses.AddResponse(e.UserId, message, mediaAlias: mediaAlias, exactTrigger: exact))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added global response to {(exact ? "exact" : "")} trigger **{message}** with media alias **{mediaAlias}**."));
                return;

            case null when mediaCategory is not null && databaseUser is { Admin: true }:
                if (!await Responses.AddResponse(e.UserId, message, mediaCategory: mediaCategory, exactTrigger: exact))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        "An unexpected database error occured."));
                    return;
                }
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Added global response to {(exact ? "exact" : "")} trigger **{message}** with media category **{mediaCategory}**."));
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

    [SlashCommand("remove", "Removes a response from a message")]
    public static async Task RemoveMessageResponse(InteractionContext e,
        [Option("trigger", "The trigger message of the response to remove")]
        string trigger,
        [Option("response", "The response message of the response to remove")]
        string? response = null,
        [Option("media_alias", "The media alias of the response to remove")]
        string? mediaAlias = null,
        [Option("media_category", "The media category of the response to remove")]
        string? mediaCategory = null,
        [Option("user", "The user of the response to remove")]
        DiscordUser? user = null,
        [Option("channel", "The channel of the response to remove"),
         ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null,
        [Option("exact", "The exact status of the response to remove")]
        bool? exact = null,
        [Option("enabled", "The enabled status of the response to remove")]
        bool? enabled = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        Guid? responseId = await ResponseHandler.HandleResponseCommand(e, trigger, response, mediaAlias, mediaCategory,
            user, channel, exact, enabled);
        
        if (responseId is null)
            return;

        await Responses.RemoveResponse((Guid)responseId);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully removed the response."));
    }

    [SlashCommand("list", "Lists the responses")]
    public static async Task ListMessageResponse(InteractionContext e,
        [Option("user", "A specific user to get responses for")]
        DiscordUser? user = null,
        [Option("channel", "A specific channel to get responses for"), ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        string title;
        List<ResponseRow> responses;

        switch (user)
        {
            case null when channel is null:
                title = $"Global responses for {e.User.GlobalName}";
                responses = await Responses.GetUserResponses(e.UserId);
                break;
            
            case null:
                title = $"Responses for {e.User.GlobalName} in #{channel.Name}";
                responses = await ResponseHandler.GetAllUserResponses(e.UserId, channel.Id);
                break;
            
            case not null when channel is null:
                title = $"Global responses for {user.GlobalName}";
                responses = await Responses.GetUserResponses(user.Id);
                break;
            
            case not null:
                title = $"Responses for {user.GlobalName} in #{channel.Name}";
                responses = await ResponseHandler.GetAllUserResponses(user.Id, channel.Id);
                break;
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
        [Option("trigger", "The trigger message of the response to enable")]
        string trigger,
        [Option("response", "The response message of the response to enable")]
        string? response = null,
        [Option("media_alias", "The media alias of the response to enable")]
        string? mediaAlias = null,
        [Option("media_category", "The media category of the response to enable")]
        string? mediaCategory = null,
        [Option("user", "The user of the response to enable")]
        DiscordUser? user = null,
        [Option("channel", "The channel of the response to enable"),
         ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null,
        [Option("exact", "The exact status of the response to enable")]
        bool? exact = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        Guid? responseId = await ResponseHandler.HandleResponseCommand(e, trigger, response, mediaAlias, mediaCategory,
            user, channel, exact);
        
        if (responseId is null)
            return;

        await Responses.ModifyResponse((Guid)responseId, enabled: true);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully enabled the response."));
    }

    [SlashCommand("disable", "Disables a response for a message")]
    public static async Task DisableMessageResponse(InteractionContext e,
        [Option("trigger", "The trigger message of the response to disable")]
        string trigger,
        [Option("response", "The response message of the response to disable")]
        string? response = null,
        [Option("media_alias", "The media alias of the response to disable")]
        string? mediaAlias = null,
        [Option("media_category", "The media category of the response to disable")]
        string? mediaCategory = null,
        [Option("user", "The user of the response to disable")]
        DiscordUser? user = null,
        [Option("channel", "The channel of the response to disable"),
         ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null,
        [Option("exact", "The exact status of the response to disable")]
        bool? exact = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        Guid? responseId = await ResponseHandler.HandleResponseCommand(e, trigger, response, mediaAlias, mediaCategory,
            user, channel, exact);
        
        if (responseId is null)
            return;

        await Responses.ModifyResponse((Guid)responseId, enabled: false);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully disabled the response."));
    }

    [SlashCommand("types", "Displays the response types for response messages")]
    public static async Task ResponseTypes(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        Random random = new();
        List<MediaRow> mediaRows = Media.GetMediaCategory("test").GetAwaiter().GetResult();
        string content =
            $"<userMention> - Replaced with the authors mention (e.g <userMention> -> {e.User.Mention})\n" +
            $"<username> - Replaced with the authors username (e.g <username> -> {e.User.Username})\n" +
            $"<userGlobalName> - Replaced with the authors Global Name (e.g <userGlobalName> -> {e.User.GlobalName})\n" +
            $"<channelMention> - Replaced with the current channels mention (e.g <channelMention> -> {e.Channel.Mention})\n" +
            $"<channelName> - Replaced with the current channels name (e.g <channelName> -> {e.Channel.Name})\n" +
            $"<mediaAlias:<Alias>> - Replaced with a specified Media Alias (e.g <mediaAlias:test> -> {Media.GetMedia("test").GetAwaiter().GetResult().Url})\n" +
            $"<mediaCategory:<category>> - Replaced with a random Media from a category (e.g <mediaCategory:test> -> {mediaRows[random.Next(mediaRows.Count)].Url})";
        
        DiscordEmbed embedBuilder = new DiscordEmbedBuilder
        {
            Color = DiscordColor.Blue,
            Title = "Response types for Que Poro Responses",
            Description = content
        }.Build();
        await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedBuilder));
    }
}

public static class ResponseHandler
{
    public static async Task HandleUserResponses(MessageCreateEventArgs e, UserRow user)
    {
        if (!user.RepliedTo)
            return;

        List<ResponseRow> responses = await GetAllUserResponses(user.Id, e.Channel.Id);
        
        if (responses.Count == 0) return;

        Random random = new();
        
        foreach (ResponseRow response in responses)
        {
            response.TriggerMessage = HandleResponseString(e, response.TriggerMessage);
            if (!e.Message.Content.Contains(response.TriggerMessage, StringComparison.CurrentCultureIgnoreCase))
                continue;
            
            if (!e.Message.Content.Equals(response.TriggerMessage) && response.ExactTrigger)
                continue;
            
            if (response.ResponseMessage is not null)
                await e.Message.RespondAsync(new DiscordMessageBuilder().WithContent(
                    HandleResponseString(e, response.ResponseMessage)));

            if (response.MediaAlias is not null)
            {
                try
                {
                    MediaRow mediaRow = Media.GetMedia(response.MediaAlias).GetAwaiter().GetResult();
                    await e.Message.RespondAsync(mediaRow.Url);
                }
                catch (KeyNotFoundException)
                {
                    // ignore
                }
            }

            if (response.MediaCategory is null) continue;
            List<MediaRow> mediaRows = await Media.GetMediaCategory(response.MediaCategory);
            if (mediaRows.Count is 0) continue;
            await e.Message.RespondAsync(mediaRows[random.Next(mediaRows.Count)].Url);
        }
    }

    public static async Task<List<ResponseRow>> GetAllUserResponses(ulong userId, ulong channelId)
    {
        List<ResponseRow> globalResponses = await Responses.GetGlobalResponses();
        List<ResponseRow> userChannelResponses = await Responses.GetUserChannelResponses(userId, channelId);
        globalResponses.ForEach(row => userChannelResponses.Add(row));
        return userChannelResponses;
    }

    private static string HandleResponseString(MessageCreateEventArgs e, string response)
    {
        Random random = new();
        response = response.Replace("<userMention>", e.Author.Mention);
        response = response.Replace("<username>", e.Author.Username);
        response = response.Replace("<userGlobalName>", e.Author.GlobalName);
        response = response.Replace("<channelMention>", e.Channel.Mention);
        response = response.Replace("<channelName>", e.Channel.Name);
        
        string mediaAlias = response.Split("<mediaAlias:")[0].Split(">")[0];
        if (!string.IsNullOrEmpty(mediaAlias))
        {
            try
            {
                MediaRow mediaRow = Media.GetMedia(mediaAlias).GetAwaiter().GetResult();
                response = response.Replace($"<mediaAlias:{mediaAlias}>", mediaRow.Url);
            }
            catch (KeyNotFoundException)
            {
                // ignore
            }
        }
            
        
        string mediaCategory = response.Split("<mediaCategory:")[0].Split(">")[0];
        if (string.IsNullOrEmpty(mediaCategory)) return response;
        List<MediaRow> mediaRows = Media.GetMediaCategory(mediaCategory).GetAwaiter().GetResult();
        if (mediaRows.Count is 0) return response;
        response = response.Replace($"<mediaCategory:{mediaCategory}>", mediaRows[random.Next(mediaRows.Count)].Url);
        return response;
    }

    public static async Task<Guid?>HandleResponseCommand(InteractionContext e, string trigger, string? response = null,
        string? mediaAlias = null, string? mediaCategory = null, DiscordUser? user = null,
        DiscordChannel? channel = null, bool? exact = null, bool? enabled = null)
    {
        if (response is null && mediaAlias is null && mediaCategory is null && user is null && channel is null &&
            exact is null && enabled is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You must provide at least 1 option alongside the trigger message."));
            return null;
        }

        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName ?? e.User.Username);
        UserRow databaseUser = await Users.GetUser(e.UserId);

        if (user is not null && user != e.User && databaseUser is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not authorised to modify other users responses."));
            return null;
        }

        if (await Responses.ResponseExists(trigger, user?.Id, channel?.Id, response, mediaAlias, mediaCategory, exact,
                enabled))
            return await Responses.GetResponseId(trigger, user?.Id, channel?.Id, response, mediaAlias, mediaCategory,
                exact);
        
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Unable to find any response with the given parameters."));
        return null;
    }
}