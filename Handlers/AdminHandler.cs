using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using QuePoro.Database.Handlers;
using QuePoro.Database.Types;

namespace QuePoro.Handlers;

/// <summary>
/// The class for handling Admin commands.
/// </summary>
[SlashCommandGroup("admin", "Admin commands")]
public class AdminCommands : ApplicationCommandsModule
{
    /// <summary>
    /// Adds an Admin to the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="user">The User to add as an Admin.</param>
    [SlashCommand("add", "Adds a bot admin to the Database")]
    public async Task AddAdmin(InteractionContext e, 
        [Option("user", "The user to add as an admin")] 
        DiscordUser user)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Users.UserExists(user.Id))
            await Users.AddUser(user.Id, user.Username, user.GlobalName);
        UserRow databaseUser = await Users.GetUser(user.Id);

        if (databaseUser is { Admin: true })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"{user.Mention} is already an admin."));
            return;
        }

        if (!await Users.ModifyUser(user.Id, user.GlobalName, admin: true))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Failed to make {user.Mention} an admin (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully added {user.Mention} as an admin."));
    }

    /// <summary>
    /// Removes an Admin from the database.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="user">The User to remove as an Admin.</param>
    [SlashCommand("remove", "Removes a bot admin from the Database")]
    public async Task RemoveAdmin(InteractionContext e, 
        [Option("user", "The admin to remove")] 
        DiscordUser user)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }
        
        if (!await Users.UserExists(user.Id))
            await Users.AddUser(user.Id, user.Username, user.GlobalName);
        UserRow databaseUser = await Users.GetUser(user.Id);

        if (databaseUser is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"{user.Mention} is already not an admin."));
            return;
        }

        if (!await Users.ModifyUser(user.Id, user.GlobalName, admin: true))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Failed to make {user.Mention} an admin (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully removed {user.Mention} as an admin."));
    }
    
    /// <summary>
    /// Enabled the Temporary VCs globally.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("enable_create_a_vc", "Enables the 'create_a_vc' function globally")]
    public static async Task EnableGlobalCreateAVc(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

        if (config.TempVcEnabled)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The 'Create A VC' functionality is already enabled."));
            return;
        }

        if (!await Config.ModifyConfig(tempVcEnabled: true))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to enabled 'Create A VC' functionality (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully enabled the 'Create A VC' functionality."));
    }

    /// <summary>
    /// Disables the Temporary VCs globally.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("disable_create_a_vc", "Disables the 'create_a_vc' function globally")]
    public static async Task DisableGlobalCreateAVc(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

        if (!config.TempVcEnabled)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The **Create A VC** functionality is already disabled."));
            return;
        }

        if (!await Config.ModifyConfig(tempVcEnabled: false))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to disable **Create A VC** functionality (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully disabled the **Create A VC** functionality."));
    }
    
    /// <summary>
    /// Validates all Temporary VCs.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("validate_temp_vcs", "Globally validates temporary VCs")]
    public static async Task ValidateGlobalTempAVcs(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Validating all temporary VCs..."));
        await CreateAVcHandler.ValidateTempVcs(e.Client);
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully validated all temporary VCs."));
    }
    
    /// <summary>
    /// Enables Responses globally.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("enable_message_replies", "Enables the message reply function globally")]
    public static async Task EnableGlobalReplies(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

        if (config.RepliesEnabled)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The **Replies** functionality is already enabled."));
            return;
        }

        if (!await Config.ModifyConfig(repliesEnabled: true))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to enabled **Replies** functionality (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully enabled **Replies** functionality."));
    }

    /// <summary>
    /// Disables Responses globally.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("disable_message_replies", "Disables the message reply function globally")]
    public static async Task DisableGlobalReplies(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

        if (!config.RepliesEnabled)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "The **Replies** functionality is already disabled."));
            return;
        }

        if (!await Config.ModifyConfig(repliesEnabled: false))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "Failed to disabled **Replies** functionality (Unexpected database error)."));
            return;
        }

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            "Successfully disabled **Replies** functionality."));
    }
    
    /// <summary>
    /// Sets the status of the bot.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    /// <param name="statusType">The type for the Status.</param>
    /// <param name="statusMessage">The message for the Status.</param>
    [SlashCommand("set_status", "Sets the bots status")]
    public static async Task SetStatus(InteractionContext e,
        [Option("status_type", "The type of status to set the bot to")]
        ActivityType statusType,
        [Option("status_message", "The message to set the status to")]
        string statusMessage)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());
        
        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (!await Users.UserExists(e.UserId))
            await Users.AddUser(e.UserId, e.User.Username, e.User.GlobalName);
        UserRow userRow = await Users.GetUser(e.UserId);

        if (userRow is { Admin: false })
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "You are not a bot admin"));
            return;
        }

        if (!await Config.ConfigExists())
            await Config.AddConfig();
        ConfigRow config = await Config.GetConfig();

        short statusInt = statusType switch
        {
            ActivityType.Playing => 0,
            ActivityType.ListeningTo => 2,
            ActivityType.Watching => 3,
            _ => 1
        };

        if (config.StatusType == statusInt && config.StatusMessage == statusMessage)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"The bots status is already set to **{statusType} {statusMessage}**"));
            return;
        }

        if (!await Config.ModifyConfig(statusType: statusInt, statusMessage: statusMessage))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"Failed to set my status to **{statusType} {statusMessage}** (Unexpected database error)."));
            return;
        }

        await e.Client.UpdateStatusAsync(new DiscordActivity(statusMessage, statusType));

        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Successfully set my status to **{statusType} {statusMessage}**."));
    }

    /// <summary>
    /// Shuts the bot down.
    /// </summary>
    /// <param name="e">The Interaction arguments.</param>
    [SlashCommand("shutdown", "Shuts the bot down.")]
    public async Task ShutdownBot(InteractionContext e)
    {
        await e.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (e.Member is null || e.Guild is null)
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                "I do not work in DMs."));
            return;
        }
        
        if (Convert.ToString(e.User.Id) == Environment.GetEnvironmentVariable("BOT_OWNER_ID"))
        {
            await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shutting down..."));
            await e.Client.UpdateStatusAsync(new DiscordActivity(), UserStatus.Offline);
            await e.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        await e.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You lack the permissions to run this command."));
    }
}