using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Media commands.
/// </summary>
[SlashCommandGroup("media", "The media commands for the bot")]
public class MediaCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Adds Media to the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="url">The address for the Media.</param>
    /// <param name="alias">The alias for the Media.</param>
    /// <param name="category">The category for the Media.</param>
    [SlashCommand("add", "Adds media to the bot")]
    public static async Task AddMedia(InteractionContext e,
        [Option("url", "The URL of the media")]
        string url,
        [Option("alias", "The Alias of the media")]
        string alias,
        [Option("category", "The Category of the media")]
        string? category = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (await Media.MediaExists(url))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Media with provided url already exists."));
            return;
        }
        
        if (await Media.MediaExists(alias: alias))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Media with provided alias already exists."));
            return;
        }

        if (!await Media.AddMedia(e.UserId, url, alias, category))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to add media to bot (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully added media with alias **{alias}**" +
            $"{(category is null ? "" : $" with category **{category}**")} to the bot."));
    }

    /// <summary>
    /// Removes Media from the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="url">The address for the Media.</param>
    [SlashCommand("remove", "Removes media from the bot")]
    public static async Task RemoveMedia(InteractionContext e,
        [Option("url", "The URL of the media to remove")]
        string url)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Media.MediaExists(url))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "No Media with provided url exists."));
            return;
        }
        
        if (!await Media.RemoveMedia(await Media.GetMediaId(url)))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to remove the media (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully removed the media from the bot."));
    }

    /// <summary>
    /// Modifies Media in the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="url">The existing address for the Media.</param>
    /// <param name="newUrl">The new address for the Media.</param>
    /// <param name="newAlias">The new alias for the Media.</param>
    /// <param name="newCategory">The new category for the Media.</param>
    [SlashCommand("modify", "Modifies media in the bot")]
    public static async Task ModifyMedia(InteractionContext e,
        [Option("url", "The URL of the media")]
        string url,
        [Option("new_url", "The new URL for the media")]
        string? newUrl = null,
        [Option("new_alias", "The new alias for the media")]
        string? newAlias = null,
        [Option("new_category", "The new category for the media")]
        string? newCategory = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (newUrl is null && newAlias is null && newCategory is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You must provide at least 1 optional parameter."));
            return;
        }

        if (!await Media.MediaExists(url))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "No Media with provided url exists."));
            return;
        }

        if (!await Media.ModifyMedia(await Media.GetMediaId(url), newAlias, newCategory, newUrl))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to modify the media (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully modified the media."));
    }
}