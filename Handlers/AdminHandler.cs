using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace QuePoro.Handlers;

[SlashCommandGroup("admin", "Admin commands")]
public class AdminCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds an Admin to the Database (Not implemented yet)")]
    public async Task AddAdmin(InteractionContext ctx, [Option("user", "The user to add as an admin")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Adding {user.Mention} to the database as a bot admin..."));
            await Task.Delay(1);
            await ctx.EditResponseAsync($"Added {user.Mention} to the database as a bot admin.");
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("remove", "Removes an Admin from the Database (Not implemented yet)")]
    public async Task RemoveAdmin(InteractionContext ctx, [Option("user", "The admin to remove")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removing {user.Mention} from the database as a bot admin..."));
            await Task.Delay(1);
            await ctx.EditResponseAsync($"Removed {user.Mention} from the database as a bot admin.");
            return;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("enable_create_a_vc", "Disables the 'create_a_vc' function globally (Not implemented)")]
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

    [SlashCommand("shutdown", "Shuts the bot down (Not implemented yet)")]
    public async Task RemoveAdmin(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (Convert.ToString(ctx.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutting down..."));
            await ctx.Client.UpdateStatusAsync(new DiscordActivity(), UserStatus.Offline);
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
}