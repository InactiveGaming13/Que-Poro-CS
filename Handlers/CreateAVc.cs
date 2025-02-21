using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("create_a_vc", "The create a VC commands (Not implemented)")]
public class CreateAVcCommands : ApplicationCommandsModule
{
    [SlashCommand("set", "Sets the create a vc channel for this guild")]
    public static async Task SetCreateAVc(InteractionContext ctx,
        [Option("channel", "The channel to set the 'create a vc' channel to"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (channel.Type != ChannelType.Voice)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"{channel.Mention} is not a valid voice channel"));
        }

        if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
            ctx.Member.Permissions.HasPermission(Permissions.Administrator))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Set the 'create a vc' channel to {channel.Mention}"));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("reset", "Sets the create a vc channel for this guild")]
    public static async Task ResetCreateAVc(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
            ctx.Member.Permissions.HasPermission(Permissions.Administrator))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Reset the 'create a vc' channel."));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }

    [SlashCommand("enable", "Enables the 'create a vc' functionality fot this guild")]
    public static async Task EnableCreateAVc(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
            ctx.Member.Permissions.HasPermission(Permissions.Administrator))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Enabled the 'create a vc' function for this guild."));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
    
    [SlashCommand("disable", "Disables the 'create a vc' functionality fot this guild")]
    public static async Task DisableCreateAVc(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
            ctx.Member.Permissions.HasPermission(Permissions.Administrator))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent($"Disabled the 'create a vc' function for this guild."));
            return;
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
    }
}