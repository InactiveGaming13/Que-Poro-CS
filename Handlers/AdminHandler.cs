using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;

namespace QuePoro.Handlers;

[SlashCommandGroup("admin", "Admin commands")]
public class AdminCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a bot admin to the Database (Not implemented yet)")]
    public async Task AddAdmin(InteractionContext e, [Option("user", "The user to add as an admin")] DiscordUser user)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("remove", "Removes a bot admin from the Database (Not implemented yet)")]
    public async Task RemoveAdmin(InteractionContext e, [Option("user", "The admin to remove")] DiscordUser user)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("enable_create_a_vc", "Enables the 'create_a_vc' function globally (Not implemented)")]
    public static async Task EnableGlobalCreateAVc(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("disable_create_a_vc", "Disables the 'create_a_vc' function globally (Not implemented)")]
    public static async Task DisableGlobalCreateAVc(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("validate_temp_vcs", "Globally validates temporary VCs (Not implemented)")]
    public static async Task ValidateGlobalTempAVcs(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("enable_message_replies", "Enables the message reply function globally (Not implemented)")]
    public static async Task EnableGlobalReplies(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("disable_message_replies", "Disables the message reply function globally (Not implemented)")]
    public static async Task DisableGlobalReplies(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("set_status", "Sets the bots status (Not implemented)")]
    public static async Task SetStatus(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (e.Member?.VoiceState is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("shutdown", "Shuts the bot down.")]
    public async Task ShutdownBot(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (Convert.ToString(e.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutting down..."));
            await e.Client.UpdateStatusAsync(new DiscordActivity(), UserStatus.Offline);
            await e.Client.DisconnectAsync();
            
            DiscordMessage message = await e.GetOriginalResponseAsync();
            await Config.ModifyConfig(shutdownChannel: e.ChannelId, shutdownMessage: message.Id);
            
            Environment.Exit(0);
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You lack the permissions to run this command."));
    }
}