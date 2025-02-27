using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

namespace QuePoro.Handlers;

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

    [SlashCommand("modify", "Modifies a temp")]
    public static async Task ModifyVc(InteractionContext ctx,
        [Option("name", "The name to set the channel to")] string name="",
        [Option("mebmber_limit", "The member limit to set the channel to")] int limit=5)
    {
        if (name == "")
        {
            name = $"{ctx.User.GlobalName}'s VC";
        }
        
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("DMs are not supported."));
        }
        
        if (ctx.Member.VoiceState is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a VC."));
            return;
        }

        if (!CreateAVcHandler.TempVcs.Contains(ctx.Member.VoiceState.Channel))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a temp VC."));
            return;
        }


        if (!ctx.Member.Permissions.HasPermission(Permissions.ManageChannels))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You do not have permission to modify temp VCs (uses role perms at the moment)."));
            return;
        }
        
        await CreateAVcHandler.ModifyTempVc(ctx.Member.VoiceState.Channel, name, limit);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Edited {ctx.Member.VoiceState.Channel.Mention}"));
    }
}

public class CreateAVcHandler
{
    public static List<DiscordChannel> TempVcs = new();
    public static async Task CreateTempVc(VoiceStateUpdateEventArgs e)
    {
        DiscordChannel newChannel = await e.Guild.CreateChannelAsync($"{e.User.GlobalName}'s VC", ChannelType.Voice,
            e.After.Channel.Parent, userLimit: 5);
        Console.WriteLine($"Created Temp VC: {newChannel.Name}");
        await newChannel.PlaceMemberAsync(e.After.Member);
        Console.WriteLine($"Moved Member: {e.After.Member.GlobalName} to Temp VC: {newChannel.Name}");
        TempVcs.Add(newChannel);
    }

    public static async Task RemoveTempVc(VoiceStateUpdateEventArgs e)
    {
        string channelName = e.Before.Channel.Name;
        TempVcs.Remove(e.Before.Channel);
        await e.Before.Channel.DeleteAsync();
        Console.WriteLine($"Deleted Temp VC: {channelName}");
    }

    public static async Task ModifyTempVc(DiscordChannel channel, string name, int memberLimit)
    {
        await channel.ModifyAsync(x =>
        {
            x.Name = name;
            x.UserLimit = memberLimit;
        });
    }
}