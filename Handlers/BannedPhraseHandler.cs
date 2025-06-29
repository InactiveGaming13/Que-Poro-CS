using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class to handle Banned Phrases.
/// </summary>
[SlashCommandGroup("banned_phrase", "The banned phrase commands.")]
public class BannedPhraseCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Adds a Banned Phrase to the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="phrase">The phrase to ban.</param>
    /// <param name="severity">The severity of the phrase</param>
    /// <param name="reason">The reason for why the phrase is banned.</param>
    /// <param name="channel">The Channel to bind the Banned Phrase to.</param>
    [SlashCommand("add", "Adds a banned phrase to the bot")]
    public static async Task AddBannedPhrase(InteractionContext e,
        [Option("phrase", "The phrase to ban")]
        string phrase,
        [Option("severity", "The severity of the phrase")]
        int severity,
        [Option("reason", "The reason for banning the phrase")]
        string? reason = null,
        [Option("channel", "The channel to bind the phrase to"), ChannelTypes(ChannelType.Text)]
        DiscordChannel? channel = null)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        
        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        
        if (channel is { Guild: not null } && !await Channels.ChannelExists(channel.Id))
            await Channels.AddChannel(channel.Id, channel.Guild.Id, channel.Name, channel.Topic);

        // If the Banned Phrase and the Link exists, tell the user and return.
        if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
            await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase), channel.Id,
                channel.Guild.Id))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"That banned phrase is already bound to {channel.Mention}"));
            return;
        }
        
        if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
            !await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase), channel.Id,
                channel.Guild.Id))
        {
            if (!await BannedPhraseLinks.AddBannedPhraseLink(await BannedPhrases.GetBannedPhraseId(phrase), channel.Id,
                    channel.Guild.Id))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "Failed to add banned phrase (Unexpected database error)"));
                return;
            }

            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully bound the banned phrase to {channel.Mention}."));
            return;
        }

        if (!await BannedPhrases.BannedPhraseExists(phrase))
        {
            if (!await BannedPhrases.AddBannedPhrase(e.User.Id, severity, phrase, reason))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "Failed to add banned phrase to bot (Unexpected database error)."));
                return;
            }

            if (channel is { Guild: not null } && !await BannedPhraseLinks.AddBannedPhraseLink(
                    await BannedPhrases.GetBannedPhraseId(phrase), channel.Id, channel.Guild.Id))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Failed to bind banned phrase to {channel.Mention}."));
                return;
            }

            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully added banned phrase to the bot" +
                $"{(channel is not null ? $" and bound it to {channel.Mention}" : "")}."));
        }
    }

    /// <summary>
    /// Removes a Banned Phrase from the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="phrase">The phrase to unban.</param>
    [SlashCommand("remove", "Removes a banned phrase from the bot")]
    public static async Task RemoveBannedPhrase(InteractionContext e,
        [Option("phrase", "The phrase to unban")]
        string phrase)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);

        if (!await Channels.ChannelExists(e.Channel.Id))
            await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);

        // If the Banned Phrase and the Link exists, tell the user and return.
        if (!await BannedPhrases.BannedPhraseExists(phrase))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "That banned phrase doesn't exists."));
            return;
        }

        List<BannedPhraseLinkRow> bannedPhraseLinks = await BannedPhraseLinks.GetBannedPhraseLinks(e.Guild.Id);
        if (await BannedPhrases.BannedPhraseExists(phrase) && bannedPhraseLinks.Count is 0)
        {
            if (!await BannedPhrases.RemoveBannedPhrase(await BannedPhrases.GetBannedPhraseId(phrase)))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "Failed to remove banned phrase from the bot (Unexpected database error)."));
                return;
            }

            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Successfully removed banned phrase from the bot."));
            return;
        }

        if (await BannedPhrases.BannedPhraseExists(phrase) && bannedPhraseLinks.Count > 0)
        {
            foreach (BannedPhraseLinkRow bannedPhraseLink in bannedPhraseLinks)
            {
                if (await BannedPhraseLinks.RemoveBannedPhraseLink(bannedPhraseLink.Id)) continue;
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "Failed to remove banned phrase from the bot (Unexpected database error)."));
                return;
            }
            
            if (!await BannedPhrases.RemoveBannedPhrase(await BannedPhrases.GetBannedPhraseId(phrase)))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "Failed to remove banned phrase from the bot (Unexpected database error)."));
                return;
            }

            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Successfully removed banned phrase from the bot."));
        }
    }

    /// <summary>
    /// The class to handle Banned Phrase Links.
    /// </summary>
    [SlashCommandGroup("link", "Commands to link a phrase to a channel")]
    public class BannedPhraseLinkCommands : ApplicationCommandsModule
    {
        /// <summary>
        /// Adds a Link Between a Banned Phrase and a Channel in the database.
        /// </summary>
        /// <param name="e">The Interaction arguments.</param>
        /// <param name="phrase">The Banned Phrase to bind.</param>
        /// <param name="channel">The Channel to bind to.</param>
        [SlashCommand("add", "Adds a link between a channel and a banned phrase")]
        public static async Task AddBannedPhraseLink(InteractionContext e,
            [Option("phrase", "The phrase to link")]
            string phrase,
            [Option("channel", "The channel to link"), ChannelTypes(ChannelType.Text)]
            DiscordChannel channel)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral());
        
            if (e.Member is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
        
            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        
            if (!await Channels.ChannelExists(e.Channel.Id))
                await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        
            if (channel is { Guild: not null } && !await Channels.ChannelExists(channel.Id))
                await Channels.AddChannel(channel.Id, channel.Guild.Id, channel.Name, channel.Topic);

            // If the Banned Phrase and the Link exists, tell the user and return.
            if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
                await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase), channel.Id,
                    channel.Guild.Id))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"That banned phrase is already bound to {channel.Mention}"));
                return;
            }

            if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
                !await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase),
                    channel.Id, channel.Guild.Id))
            {
                if (!await BannedPhraseLinks.AddBannedPhraseLink(await BannedPhrases.GetBannedPhraseId(phrase),
                        channel.Id, channel.Guild.Id))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Failed to bind banned phrase with {channel.Mention} (Unexpected database error)."));
                    return;
                }

                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Successfully bound banned phrase with {channel.Mention}"));
            }
        }
        
        /// <summary>
        /// Removes a Link between a Banned Phrase and a Channel in the database.
        /// </summary>
        /// <param name="e">The Interaction arguments.</param>
        /// <param name="phrase">The Banned Phrase to unbind.</param>
        /// <param name="channel">The Channel to unbind from.</param>
        [SlashCommand("remove", "Removes a link between a channel and a banned phrase")]
        public static async Task RemoveBannedPhraseLink(InteractionContext e,
            [Option("phrase", "The phrase to unlink")]
            string phrase,
            [Option("channel", "The channel to unlink"), ChannelTypes(ChannelType.Text)]
            DiscordChannel? channel = null)
        {
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral());
        
            if (e.Member is null || e.Guild is null)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "I do not work in DMs."));
                return;
            }
        
            if (!await Guilds.GuildExists(e.Guild.Id))
                await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);
        
            if (!await Channels.ChannelExists(e.Channel.Id))
                await Channels.AddChannel(e.Channel.Id, e.Guild.Id, e.Channel.Name, e.Channel.Topic);
        
            if (channel is { Guild: not null } && !await Channels.ChannelExists(channel.Id))
                await Channels.AddChannel(channel.Id, channel.Guild.Id, channel.Name, channel.Topic);

            // If the Banned Phrase and the Link exists, tell the user and return.
            if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
                !await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase), channel.Id,
                    channel.Guild.Id))
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"That banned phrase is already not bound to {channel.Mention}"));
                return;
            }

            if (await BannedPhrases.BannedPhraseExists(phrase) && channel is { Guild: not null } &&
                await BannedPhraseLinks.BannedPhraseLinkExists(await BannedPhrases.GetBannedPhraseId(phrase),
                    channel.Id, channel.Guild.Id))
            {
                Guid banedPhraseId = await BannedPhrases.GetBannedPhraseId(phrase);
                Guid banedPhraseLinkId =
                    await BannedPhraseLinks.GetBannedPhraseLinkId(banedPhraseId, channel.Id, channel.Guild.Id);
                
                if (!await BannedPhraseLinks.RemoveBannedPhraseLink(banedPhraseLinkId))
                {
                    await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"Failed to unbind banned phrase from {channel.Mention} (Unexpected database error)."));
                    return;
                }

                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"Successfully unbound banned phrase from {channel.Mention}"));
            }
        }
    }
}

public static class BannedPhraseHandler
{
    /// <summary>
    /// Checks if any Banned Phrases are in a Message.
    /// </summary>
    /// <param name="message">The Message content.</param>
    /// <returns>Null if clear, Reason if to delete.</returns>
    public static async Task<string?> HandleBannedPhrases(string message)
    {
        List<BannedPhraseRow> bannedPhrases = await BannedPhrases.GetAllBannedPhrases();
        foreach (BannedPhraseRow bannedPhrase in bannedPhrases)
        {
            if (!message.Contains(bannedPhrase.Phrase, StringComparison.CurrentCultureIgnoreCase))
                continue;
            return bannedPhrase.Reason ?? "Phrase not allowed.";
        }

        return null; 
    }
}