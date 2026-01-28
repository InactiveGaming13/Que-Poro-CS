using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Role commands.
/// </summary>
[SlashCommandGroup("role_reactions", "Role Reaction commands")]
public class RoleReactionCommands : ApplicationCommandsModule
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The Interaction Context.</param>
    /// <param name="message">The Message Link to listen to.</param>
    /// <param name="role">The Role to give.</param>
    /// <param name="emoji">The Emoji to listen for.</param>
    [SlashCommand("add", "Adds a Role Reaction to a pre-existing message (admin)")]
    public static async Task AddRoleReaction(InteractionContext e,
        [Option("message", "The message link to listen to")]
        string message,
        [Option("role", "The role to give when someone reacts")]
        DiscordRole role,
        [Option("emoji", "The emoji to listen for.")]
        string emoji)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (e.Guild is null || e.Member is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
        {
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            return;
        }

        if (await Users.GetUser(e.UserId) is { Admin: false } &&
            !e.Member.Permissions.HasPermission(Permissions.ManageRoles))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You lack the permissions to use this command."));
            return;
        }

        DiscordMessage? discordMessage = null;
        try
        {
            discordMessage = await DiscordMessage.FromJumpLinkAsync(e.Client, message);
        }
        catch (NotFoundException)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided discord message is invalid."));
            return;
        }

        DiscordEmoji? discordEmoji = MessageReactionHandler.GetEmoji(e.Client, emoji);
        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid."));
            return;
        }

        if (role.IsHoisted)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided role is above me and I am unable to give it."));
            return;
        }

        try
        {
            await RoleReactions.GetRoleReaction(await RoleReactions.GetRoleReactionId(discordMessage.Guild!.Id,
                discordMessage.ChannelId, message, role.Id, emoji));
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "This Role Reaction already exists."));
            return;
        }
        catch (KeyNotFoundException)
        {
            // ignore
        }

        await RoleReactions.AddRoleReaction(e.UserId, discordMessage.Guild!.Id, discordMessage.ChannelId, message,
            role.Id, emoji);

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully added the Role Reaction."));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">The Interaction Context.</param>
    /// <param name="message">The Message Link to stop listening to.</param>
    /// <param name="role">The Role to stop giving when someone reacts.</param>
    /// <param name="emoji">The Emoji to stop listening for.</param>
    [SlashCommand("remove", "Removes a Role Reaction from a pre-existing message (admin)")]
    public static async Task RemoveRoleReaction(InteractionContext e,
        [Option("message", "The message link to stop listening to")]
        string message,
        [Option("role", "The role to stop giving when someone reacts")]
        DiscordRole role,
        [Option("emoji", "The emoji to stop listening for")]
        string emoji)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (e.Guild is null || e.Member is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }

        if (!await Users.UserExists(e.UserId))
        {
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
            return;
        }

        if (await Users.GetUser(e.UserId) is { Admin: false } &&
            !e.Member.Permissions.HasPermission(Permissions.ManageRoles))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You lack the permissions to use this command."));
            return;
        }

        DiscordMessage? discordMessage = null;
        try
        {
            discordMessage = await DiscordMessage.FromJumpLinkAsync(e.Client, message);
        }
        catch (NotFoundException)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided discord message is invalid."));
            return;
        }

        DiscordEmoji? discordEmoji = MessageReactionHandler.GetEmoji(e.Client, emoji);
        if (discordEmoji is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The provided emoji is invalid."));
            return;
        }

        try
        {
            await RoleReactions.GetRoleReaction(await RoleReactions.GetRoleReactionId(discordMessage.Guild!.Id,
                discordMessage.ChannelId, message, role.Id, emoji));
        }
        catch (KeyNotFoundException)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "This Role Reaction doesn't exist."));
            return;
        }

        await RoleReactions.RemoveRoleReaction(await RoleReactions.GetRoleReactionId(discordMessage.Guild.Id,
            discordMessage.ChannelId, message, role.Id, emoji));

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully removed the Role Reaction."));
    }
}