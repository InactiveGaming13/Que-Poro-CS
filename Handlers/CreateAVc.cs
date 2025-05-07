using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling the 'create_a_vc' commands.
/// </summary>
[SlashCommandGroup("create_a_vc", "The create a VC commands")]
public class CreateAVcCommands : ApplicationCommandsModule
{
    /// <summary>
    /// The class for handling the 'channel' commands.
    /// </summary>
    [SlashCommandGroup("channel", "The temp vc channel commands")]
    public class ChannelCommands : ApplicationCommandsModule
    {
        /// <summary>
        /// A command to set the 'create a VC' channel for the current guild.
        /// </summary>
        /// <param name="ctx">The context of the command.</param>
        /// <param name="channel">The channel to set the 'create a VC' channel to.</param>
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
        
        /// <summary>
        /// A command to reset the 'create a VC' channel for the current guild.
        /// </summary>
        /// <param name="ctx">The context of the command.</param>
        [SlashCommand("reset", "Resets the create a vc channel for this guild")]
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
    }

    /// <summary>
    /// The class for handling the 'default_member_limit' commands for the current guild.
    /// </summary>
    [SlashCommandGroup("default_member_limit", "The temp vc default member limit commands")]
    public class DefaultMemberLimitCommands : ApplicationCommandsModule
    {
        /// <summary>
        /// A command to set the 'default_member_limit' for the current guild.
        /// </summary>
        /// <param name="ctx">The context of the command.</param>
        /// <param name="limit">The new member limit to set the default to.</param>
        [SlashCommand("set", "Sets the default member limit for a temp vc for this guild")]
        public static async Task SetCreateAVcMemberLimit(InteractionContext ctx,
            [Option("member_limit", "The member limit to set the temp vc default to"), MinimumValue(0), MaximumValue(99)]
            int limit)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
                ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Set the default member limit for a temp vc to {limit}"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        /// <summary>
        /// A command to reset the 'default_member_limit' channel for the current guild.
        /// </summary>
        /// <param name="ctx">The new bitrate to set the default to.</param>
        [SlashCommand("reset", "Resets the default member limit for a temp vc for this guild")]
        public static async Task ResetCreateAVcMemberLimit(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
                ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent("Reset the default member limit."));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
    }

    /// <summary>
    /// The class for handling the 'default_bitrate' commands for the current guild.
    /// </summary>
    [SlashCommandGroup("default_bitrate", "The temp vc default bitrate commands")]
    public class DefaultBitrateCommands : ApplicationCommandsModule
    {
        /// <summary>
        /// A command to set the default bitrate of the 'create a VC' function to for the current guild.
        /// </summary>
        /// <param name="ctx">The context of the command.</param>
        /// <param name="bitrate">The new bitrate to set the default to.</param>
        [SlashCommand("set", "Sets the default bitrate for a temp vc for this guild")]
        public static async Task SetCreateAVcBitrate(InteractionContext ctx,
            [Option("bitrate", "The bitrate to set the temp vc default to"), MinimumValue(8), MaximumValue(384)]
            int bitrate)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
                ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent($"Set the default bitrate for a temp vc to {bitrate}"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
        
        /// <summary>
        /// A command to reset the default bitrate for the 'create a vc' function for the current guild.
        /// </summary>
        /// <param name="ctx">The context of the command.</param>
        [SlashCommand("reset", "Resets the default bitrate for a temp vc for this guild")]
        public static async Task ResetCreateAVcMemberLimit(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (Convert.ToString(ctx.Member.Id) != Environment.GetEnvironmentVariable("BOT_OWNER_ID") ||
                ctx.Member.Permissions.HasPermission(Permissions.Administrator))
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().WithContent("Reset the default bitrate."));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("This command is not yet implemented."));
        }
    }

    /// <summary>
    /// A command to enable the 'create a VC' function for the current guild.
    /// </summary>
    /// <param name="ctx">The context of the command.</param>
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
    
    /// <summary>
    /// A command to disable the 'create a VC' function for the current guild.
    /// </summary>
    /// <param name="ctx">The context of the command.</param>
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


/// <summary>
/// The class for handling the 'temp_vc' commands.
/// </summary>
[SlashCommandGroup("temp_vc", "The temp vc commands")]
public class TempVcCommands : ApplicationCommandsModule
{
    /// <summary>
    /// A command that allows an administrator, or a channel master to modify a temporary VC.
    /// </summary>
    /// <param name="ctx">The context of the command.</param>
    /// <param name="name">The optional new name of the channel.</param>
    /// <param name="limit">The optional new member limit of the channel.</param>
    /// <param name="bitrate">The optional new bitrate of the channel.</param>
    [SlashCommand("modify", "Modifies a temp vc")]
    public static async Task ModifyTempVc(InteractionContext ctx,
        [Option("name", "The name to set the channel to")] string name="",
        [Option("member_limit", "The member limit to set the channel to")] int limit=5,
        [Option("bitrate", "The bitrate to set the channel to (e.g. 64kbps)"), MinimumValue(8), MaximumValue(384)] int bitrate=64)
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
        
        DiscordMember bot = await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild);
        if (!bot.Roles.Any(role => role.Permissions.HasPermission(Permissions.ManageChannels)))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I lack the permissions to use this feature."));
            return;
        }
        
        await CreateAVcHandler.ModifyTempVc(ctx.Member.VoiceState.Channel, name, limit, bitrate);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Edited {ctx.Member.VoiceState.Channel.Mention}"));
    }

    /// <summary>
    /// A command that allows administrators to remove a temporary VC.
    /// </summary>
    /// <param name="ctx">The context of the command.</param>
    /// <param name="channel">The required channel to remove.</param>
    [SlashCommand("remove", "Removes a temp vc (admin only)")]
    public static async Task RemoveTempVc(InteractionContext ctx,
        [Option("channel", "The temp vc to remove"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        if (ctx.Member is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("DMs are not supported."));
        }
        
        if (!CreateAVcHandler.TempVcs.Contains(channel))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Specified channel is not a temp vc."));
            return;
        }
        
        if (!ctx.Member.Permissions.HasPermission(Permissions.Administrator))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You do not have permission to modify temp VCs."));
            return;
        }

        DiscordMember bot = await ctx.Client.CurrentUser.ConvertToMember(ctx.Guild);
        if (!bot.Roles.Any(role => role.Permissions.HasPermission(Permissions.ManageChannels)))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I lack the permissions to use this feature."));
            return;
        }
        
        await CreateAVcHandler.RemoveTempVc(channel, "a command used by an admin");
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed temp vc {channel.Name}"));
    }
}

/// <summary>
/// The class for handling the 'create a VC' feature.
/// </summary>
public class CreateAVcHandler
{
    public static List<DiscordChannel> TempVcs = new();
    
    /// <summary>
    /// Creates a temporary VC.
    /// </summary>
    /// <param name="e">The arguments created by a users voice state changing.</param>
    public static async Task CreateTempVc(VoiceStateUpdateEventArgs e)
    {
        string channelName = e.User.GlobalName.ToLower().EndsWith("s")
            ? $"{e.User.GlobalName}' VC"
            : $"{e.User.GlobalName}'s VC";
        Optional<string> topic = new Optional<string>("Tested");
        DiscordChannel newChannel = await e.Guild.CreateChannelAsync(channelName, ChannelType.Voice,
            e.After.Channel.Parent, userLimit: 5, bitrate: 64000, reason: $"Temp VC created by {e.User.GlobalName}");
        Console.WriteLine($"Created Temp VC: {newChannel.Name}");
        await newChannel.PlaceMemberAsync(e.After.Member);
        Console.WriteLine($"Moved Member: {e.After.Member.GlobalName} to Temp VC: {newChannel.Name}");
        TempVcs.Add(newChannel);
    }

    /// <summary>
    /// Removes a temporary VC.
    /// </summary>
    /// <param name="channel">The channel to remove.</param>
    /// <param name="reason">The optional reason shown in the audit log (sentence starts with “due to...").</param>
    public static async Task RemoveTempVc(DiscordChannel channel, string? reason = null)
    {
        reason ??= "No reason specified";
        string channelName = channel.Name;
        TempVcs.Remove(channel);
        await channel.DeleteAsync($"Temp VC removed due to {reason}");
        Console.WriteLine($"Deleted Temp VC: {channelName}");
    }

    /// <summary>
    /// Modifies a temporary VC.
    /// </summary>
    /// <param name="channel">The channel to edit.</param>
    /// <param name="name">The new name for the channel.</param>
    /// <param name="memberLimit">The new member limit for the channel.</param>
    /// <param name="bitrate">The new bitrate for the channel (for example 64)</param>
    public static async Task ModifyTempVc(DiscordChannel channel, string name, int memberLimit, int bitrate)
    {
        await channel.ModifyAsync(x =>
        {
            x.Name = name;
            x.UserLimit = memberLimit;
            x.Bitrate = bitrate * 1000;
            x.AuditLogReason = $"Temp VC modified by user.";
        });
    }
}