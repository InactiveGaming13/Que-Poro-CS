using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Exceptions;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

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
        /// <param name="e">The context of the command.</param>
        /// <param name="channel">The channel to set the 'create a VC' channel to.</param>
        [SlashCommand("set", "Sets the create a vc channel for this guild")]
        public static async Task SetCreateAVc(InteractionContext e,
            [Option("channel", "The channel to set the 'create a vc' channel to"), ChannelTypes(ChannelType.Voice)]
            DiscordChannel channel)
        {
            if (e.Guild is null)
                return;
            
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (channel.Type != ChannelType.Voice)
            {
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"{channel.Mention} is not a valid voice channel"));
                return;
            }
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 0, channel))
                return;
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully set the 'Create A VC' channel for **{e.Guild.Name}** to {channel.Mention}."));
        }
        
        /// <summary>
        /// A command to reset the 'create a VC' channel for the current guild.
        /// </summary>
        /// <param name="e">The context of the command.</param>
        [SlashCommand("reset", "Resets the create a vc channel for this guild")]
        public static async Task ResetCreateAVc(InteractionContext e)
        {
            if (e.Guild is null)
                return;
            
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 1))
                return;

            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully reset the 'Create A VC' channel for **{e.Guild}**"));
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
        /// <param name="e">The context of the command.</param>
        /// <param name="limit">The new member limit to set the default to.</param>
        [SlashCommand("set", "Sets the default member limit for a temp vc for this guild")]
        public static async Task SetCreateAVcMemberLimit(InteractionContext e,
            [Option("member_limit", "The member limit to set the temp vc default to"), MinimumValue(0), MaximumValue(99)]
            int limit)
        {
            if (e.Guild is null)
                return;
            
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 2, newValue: limit))
                return;
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully set the 'Create A VC' default member limit to **{limit}** in **{e.Guild.Name}**."));
        }
        
        /// <summary>
        /// A command to reset the 'default_member_limit' channel for the current guild.
        /// </summary>
        /// <param name="e">The new bitrate to set the default to.</param>
        [SlashCommand("reset", "Resets the default member limit for a temp vc for this guild")]
        public static async Task ResetCreateAVcMemberLimit(InteractionContext e)
        {
            if (e.Guild is null)
                return;
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 3))
                return;
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully reset the 'Create A VC' default member limit in **{e.Guild.Name}**."));
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
        /// <param name="e">The context of the command.</param>
        /// <param name="bitrate">The new bitrate to set the default to.</param>
        [SlashCommand("set", "Sets the default bitrate for a temp vc for this guild")]
        public static async Task SetCreateAVcBitrate(InteractionContext e,
            [Option("bitrate", "The bitrate to set the temp vc default to"), MinimumValue(8), MaximumValue(384)]
            int bitrate)
        {
            if (e.Guild is null)
                return;
            
            await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 4, newValue: bitrate))
                return;
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully set the 'Create A VC' default bitrate to **{bitrate}kbps** in **{e.Guild.Name}**."));
        }
        
        /// <summary>
        /// A command to reset the default bitrate for the 'create a vc' function for the current guild.
        /// </summary>
        /// <param name="e">The context of the command.</param>
        [SlashCommand("reset", "Resets the default bitrate for a temp vc for this guild")]
        public static async Task ResetCreateAVcMemberLimit(InteractionContext e)
        {
            if (e.Guild is null)
                return;
            
            if (!await CreateAVcHandler.HandleTempVcCommand(e, 5))
                return;
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Successfully reset the 'Create A VC' default bitrate in **{e.Guild.Name}**."));
        }
    }

    /// <summary>
    /// A command to enable the 'create a VC' function for the current guild.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    [SlashCommand("enable", "Enables the 'create a vc' functionality fot this guild")]
    public static async Task EnableCreateAVc(InteractionContext e)
    {
        if (e.Guild is null)
            return;
        
        if (!await CreateAVcHandler.HandleTempVcCommand(e, 6))
            return;
            
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully **enabled** the 'Create A VC' functionality in **{e.Guild.Name}**."));
    }
    
    /// <summary>
    /// A command to disable the 'create a VC' function for the current guild.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    [SlashCommand("disable", "Disables the 'create a vc' functionality fot this guild")]
    public static async Task DisableCreateAVc(InteractionContext e)
    {
        if (e.Guild is null)
            return;
        
        if (!await CreateAVcHandler.HandleTempVcCommand(e, 7))
            return;
            
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully **enabled** the 'Create A VC' functionality in **{e.Guild.Name}**."));
    }
}


/// <summary>
/// The class for handling the 'temp_vc' commands.
/// </summary>
[SlashCommandGroup("temp_vc", "The temp vc commands")]
public class TempVcCommands : ApplicationCommandsModule
{
    /// <summary>
    /// A command administrators, or channel masters use to modify a temporary VC.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    /// <param name="name">The optional new name of the channel.</param>
    /// <param name="limit">The optional new member limit of the channel.</param>
    /// <param name="bitrate">The optional new bitrate of the channel.</param>
    [SlashCommand("modify", "Modifies a temp vc")]
    public static async Task ModifyTempVc(InteractionContext e,
        [Option("name", "The name to set the channel to")] string? name = null,
        [Option("member_limit", "The member limit to set the channel to")] int? limit = null,
        [Option("bitrate", "The bitrate to set the channel to (e.g. 64kbps)"),
         MinimumValue(8), MaximumValue(384)] int? bitrate = null)
    {
        if (e.Guild is null || e.Member is null)
            return;
        
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        if (name is null && limit is null && bitrate is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You must provide at least 1 parameter to edit the vc with."));
            return;
        }
        
        if (e.Member.VoiceState?.Channel is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You are not in a VC."));
            return;
        }
        
        TempVcRow? databaseTempVc = await TempVcs.GetTempVc(e.Member.VoiceState.Channel.Id);

        if (databaseTempVc is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"{e.Member.VoiceState.Channel.Mention} is not a Temporary VC."));
            return;
        }

        if (!databaseTempVc.Master.Equals(e.UserId))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are unable to modify temp VCs since you are not the channel master."));
            return;
        }
        
        DiscordMember bot = await e.Client.CurrentUser.ConvertToMember(e.Guild);
        if (!bot.Roles.Any(role => role.Permissions.HasPermission(Permissions.ManageChannels)))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I lack the permissions to use this feature."));
            return;
        }
        
        await CreateAVcHandler.ModifyTempVc(e.User, e.Member.VoiceState.Channel, name, limit, bitrate);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Edited {e.Member.VoiceState.Channel.Mention}"));
    }

    /// <summary>
    /// A command administrators use to remove a temporary VC.
    /// </summary>
    /// <param name="e">The context of the command.</param>
    /// <param name="channel">The required channel to remove.</param>
    [SlashCommand("remove", "Removes a temp vc (admin only)")]
    public static async Task RemoveTempVc(InteractionContext e,
        [Option("channel", "The temp vc to remove"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel channel)
    {
        if (e.Guild is null || e.Member is null)
            return;
        
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        TempVcRow? databaseTempVc = await TempVcs.GetTempVc(channel.Id);

        if (databaseTempVc is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"{channel.Mention} is not a Temporary VC."));
            return;
        }
        
        if (!e.Member.Permissions.HasPermission(Permissions.ManageChannels) || !databaseTempVc.Master.Equals(e.UserId))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You do not have permission to modify temp VCs."));
            return;
        }

        DiscordMember bot = await e.Client.CurrentUser.ConvertToMember(e.Guild);
        if (!bot.Roles.Any(role => role.Permissions.HasPermission(Permissions.ManageChannels)))
        {
            await e.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("I lack the permissions to use this feature."));
            return;
        }
        
        await CreateAVcHandler.RemoveTempVc(channel, "a command used by an admin");
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed temp vc {channel.Name}"));
    }
}

/// <summary>
/// The class for handling the 'create a VC' feature.
/// </summary>
public static class CreateAVcHandler
{
    /// <summary>
    /// Creates a temporary VC.
    /// </summary>
    /// <param name="e">The arguments created by a users voice state changing.</param>
    public static async Task<DiscordChannel?> CreateTempVc(VoiceStateUpdateEventArgs e)
    {
        if (e.After.Channel is null)
        {
            Console.WriteLine("User is not in a VC.");
            return null;
        }

        GuildRow? databaseGuild = await Guilds.GetGuild(e.Guild.Id);

        if (databaseGuild is null && await Guilds.AddGuild(e.Guild.Id, e.Guild.Name))
            databaseGuild = await Guilds.GetGuild(e.Guild.Id);
        
        string channelName = (e.User.GlobalName ?? e.User.Username).ToLower().EndsWith('s')
            ? $"{e.User.GlobalName ?? e.User.Username}' VC"
            : $"{e.User.GlobalName ?? e.User.Username}'s VC";
        
        DiscordChannel newChannel = await e.Guild.CreateChannelAsync(channelName, ChannelType.Voice,
            e.After.Channel.Parent, userLimit: databaseGuild?.TempVcDefaultMemberLimit,
            bitrate: databaseGuild?.TempVcDefaultBitrate * 1000, reason: $"Temp VC created by {e.User.GlobalName}");
        Console.WriteLine($"Created Temp VC: {newChannel.Name}");
        
        await newChannel.PlaceMemberAsync(e.After.Member);
        Console.WriteLine($"Moved Member: {e.After.Member.GlobalName} to Temp VC: {newChannel.Name}");
        return newChannel;
    }

    /// <summary>
    /// Removes a temporary VC.
    /// </summary>
    /// <param name="channel">The channel to remove.</param>
    /// <param name="reason">The optional reason shown in the audit log (sentence starts with “due to...").</param>
    public static async Task RemoveTempVc(DiscordChannel channel, string? reason = null)
    {
        reason ??= "No reason specified";
        await channel.DeleteAsync($"Temp VC removed due to {reason}");
    }
    
    /// <summary>
    /// Modifies a temp VC.
    /// </summary>
    /// <param name="user">The user to display in the audit log</param>
    /// <param name="channel">The channel to modify</param>
    /// <param name="name">The new name of the channel</param>
    /// <param name="memberLimit">The new member limit of the channel</param>
    /// <param name="bitrate">The new bitrate of the channel</param>
    public static async Task ModifyTempVc(DiscordUser user, DiscordChannel channel, string? name, int? memberLimit,
        int? bitrate)
    {
        await channel.ModifyAsync(discordChannel =>
        {
            discordChannel.Name = name ?? channel.Name;
            discordChannel.UserLimit = memberLimit ?? channel.UserLimit;
            discordChannel.Bitrate = bitrate * 1000 ?? channel.Bitrate;
            discordChannel.AuditLogReason = $"Temp VC modified by {user.GlobalName ?? user.Username}.";
        });
    }

    /// <summary>
    /// The default handler for a Temp VC config change for a guild.
    /// </summary>
    /// <param name="e">The interaction context for the command</param>
    /// <param name="action">
    /// 0-Set 'Create A VC' Channel
    /// 1–Reset 'Create A VC' Channel
    /// 2–Set default member limit
    /// 3–Reset default member limit
    /// 4–Set default bitrate
    /// 5–Reset default bitrate
    /// 6–Enable function
    /// 7–Disable function</param>
    /// <param name="channel">The Discord channel to set the new vc to (Used for 0)</param>
    /// <param name="newValue">The value to set an option to (Used for 2 and 4)</param>
    /// <returns>Whether the operation was successful</returns>
    public static async Task<bool> HandleTempVcCommand(InteractionContext e, int action, DiscordChannel? channel = null,
        int? newValue = null)
    {
        if (e.Guild is null || e.Member is null)
            return false;
        
        if (!await Config.ConfigExists())
            await Config.AddConfig(); 
        ConfigRow config = await Config.GetConfig();
        
        if (!await Users.UserExists(e.User.Id))
            await Users.AddUser(e.User.Id, e.User.Username, e.User.GlobalName);
        UserRow user = await Users.GetUser(e.UserId);

        if (!await Guilds.GuildExists(e.Guild.Id))
            await Guilds.AddGuild(e.Guild.Id, e.Guild.Name);

        if (e.Member.Permissions.HasPermission(Permissions.ManageChannels) is false &&
            user is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"You are not authorised to modify the 'Create A VC' settings for **{e.Guild.Name}**."));
            return false;
        }

        switch (action)
        {
            case 0 when channel is not null && await Guilds.ModifyGuild(e.Guild.Id, tempVcChannel: channel.Id):
            case 1 when await Guilds.ModifyGuild(e.Guild.Id, tempVcChannel: 0):
            case 2 when newValue is not null && await Guilds.ModifyGuild(e.Guild.Id, tempVcDefaultMemberLimit: newValue):
            case 3 when await Guilds.ModifyGuild(e.Guild.Id, tempVcDefaultMemberLimit: config.TempVcDefaultMemberLimit):
            case 4 when newValue is not null && await Guilds.ModifyGuild(e.Guild.Id, tempVcDefaultBitrate: newValue):
            case 5 when await Guilds.ModifyGuild(e.Guild.Id, tempVcDefaultBitrate: config.TempVcDefaultBitrate):
            case 6 when await Guilds.ModifyGuild(e.Guild.Id, tempVcEnabled: true):
            case 7 when await Guilds.ModifyGuild(e.Guild.Id, tempVcEnabled: false):
                return true;
            
            default:
                await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "An unexpected database error occured."));
                return false;
        }
    }

    public static async Task ValidateTempVcs(DiscordClient client)
    {
        Console.WriteLine("Validating Temporary VCs...");
        List<TempVcRow> tempVcs = await TempVcs.GetTempVcs();

        if (tempVcs.Count is 0)
        {
            Console.WriteLine("Temporary VCs validated.");
            return;
        }

        foreach (TempVcRow tempVc in tempVcs)
        {
            DiscordChannel channel;
            try
            {
                channel = await client.GetChannelAsync(tempVc.Id);
            }
            catch (NotFoundException)
            {
                await TempVcs.RemoveTempVc(tempVc.Id);
                continue;
            }

            if (channel.Users.Count is 0 || channel.Users is [{ IsBot: true }])
            {
                await TempVcs.RemoveTempVc(channel.Id);
                await RemoveTempVc(channel, "being empty");
                continue;
            }

            tempVc.UserCount = channel.Users.Count;

            List<ulong> memberIds = [];
            foreach (DiscordMember discordUser in channel.Users)
            {
                memberIds.Add(discordUser.Id);
                
                if (discordUser.IsBot || tempVc.UserQueue.Contains(discordUser.Id))
                    continue;
                    
                tempVc.UserQueue.Add(discordUser.Id);
            }
            
            tempVc.UserQueue = tempVc.UserQueue.Intersect(memberIds).ToList();
            

            await TempVcs.ModifyTempVc(channel.Id, tempVc.Master, tempVc.Name, tempVc.Bitrate, tempVc.UserLimit,
                tempVc.UserCount, tempVc.UserQueue);
        }
        
        Console.WriteLine("Temporary VCs validated.");
    }
}