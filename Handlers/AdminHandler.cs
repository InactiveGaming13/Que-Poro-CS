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
    public async Task AddAdmin(InteractionContext ctx, [Option("user", "The user to add as an admin")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Adding {user.Mention} to the database as a bot admin..."));
            await Task.Delay(1);
            await ctx.EditResponseAsync($"{user.Mention} is now a bot admin.");
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("remove", "Removes a bot admin from the Database (Not implemented yet)")]
    public async Task RemoveAdmin(InteractionContext ctx, [Option("user", "The admin to remove")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removing {user.Mention} from the database as a bot admin..."));
            await Task.Delay(1);
            await ctx.EditResponseAsync($"{user.Mention} is no longer a bot admin.");
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("enable_create_a_vc", "Enables the 'create_a_vc' function globally (Not implemented)")]
    public static async Task EnableGlobalCreateAVc(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Enabled the 'create a vc' function globally."));
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("disable_create_a_vc", "Disables the 'create_a_vc' function globally (Not implemented)")]
    public static async Task DisableGlobalCreateAVc(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Disabled the 'create a vc' function globally."));
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("enable_message_replies", "Enables the message reply function globally (Not implemented)")]
    public static async Task EnableGlobalReplies(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Enabled the message reply function globally."));
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("disable_message_replies", "Disables the message reply function globally (Not implemented)")]
    public static async Task DisableGlobalReplies(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Disabled the message reply function globally."));
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("shutdown", "Shuts the bot down.")]
    public async Task ShutdownBot(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutting down..."));
            await ctx.Client.UpdateStatusAsync(new DiscordActivity(), UserStatus.Offline);
            await ctx.Client.DisconnectAsync();
            
            DiscordMessage message = await ctx.GetOriginalResponseAsync();
            await Config.ModifyConfig(shutdownChannel: ctx.ChannelId, shutdownMessage: message.Id);
            
            Environment.Exit(0);
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You lack the permissions to run this command."));
    }
}