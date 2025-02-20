using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS.Handlers;

[SlashCommandGroup("voice", "Voice commands")]
public class VoiceCommands : ApplicationCommandsModule
{
    [SlashCommand("join", "Joins a Voice Channel")]
    public async Task Join(InteractionContext ctx,
        [Option("channel", "Channel to join"), ChannelTypes(ChannelType.Voice)] DiscordChannel channel = null!)
    {
        if (channel is null && ctx.Member.VoiceState != null && ctx.Member.VoiceState.Channel != null)
        {
            channel = ctx.Member.VoiceState.Channel;
        }

        await VoiceHandler.Connect(ctx, channel);
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext ctx)
    {
        await VoiceHandler.Disconnect(ctx);
    }

    [SlashCommand("play", "Play some music")]
    public async Task Play(InteractionContext ctx,
        [Option("song", "Song title to play")] string track,
        [Option("force", "Force play and override the queue")] bool force = false)
    {
        await VoiceHandler.PlayTrack(ctx, track, force);
    }

    [SlashCommand("pause", "Pause the current song")]
    public async Task Pause(InteractionContext ctx)
    {
        await VoiceHandler.PauseTrack(ctx);
    }
    
    [SlashCommand("resume", "Resumes the current song")]
    public async Task Resume(InteractionContext ctx)
    {
        await VoiceHandler.ResumeTrack(ctx);
    }
    
    [SlashCommand("stop", "Stops the current song")]
    public async Task Stop(InteractionContext ctx)
    {
        await VoiceHandler.StopTrack(ctx);
    }
    
    [SlashCommand("skip", "skips the current song")]
    public async Task Skip(InteractionContext ctx)
    {
        await VoiceHandler.SkipTrack(ctx);
    }
    
    [SlashCommand("currently_playing", "Gets the current song")]
    public async Task CurrentlyPlaying(InteractionContext ctx)
    {
        await VoiceHandler.CurrentlyPlaying(ctx);
    }
}

[SlashCommandGroup("testers", "Voice commands")]
public abstract class TesterCommands : ApplicationCommandsModule
{
    [SlashCommand("emoji", "Sends the test emoji")]
    public async Task Emoji(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = "<:test:1341993967046103113>"
            });
    }
    
    [SlashCommand("ping", "Sends pong")]
    public async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = $"Pong!"
            });
    }
}

[SlashCommandGroup("config", "Configuration commands")]
public abstract class ConfigCommands : ApplicationCommandsModule
{
    [SlashCommand("response", "Sets weather or not the bot responds to you")]
    public async Task Response(InteractionContext ctx, [Option("value", "True for response, false for silence", false)] bool silent = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = silent ? "The bot will respond to your messages." : "The bot will no longer respond to your messages."
            });
    }
}

[SlashCommandGroup("reactions", "Reaction commands")]
public abstract class ReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public async Task AddReaction(InteractionContext ctx,
        [Option("emoji", "emoji you want to have reacted")] String emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser user = null)
    {
        await ReactionHandler.AddUserReaction(ctx, user, emoji);
    }
}


[SlashCommandGroup("admin", "Admin commands")]
public abstract class AdminCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds an Admin to the Database")]
    public async Task AddAdmin(InteractionContext ctx, [Option("user", "The user to add as an admin.")] DiscordUser user)
    {
        await AdminHandler.AddUserAdmin(ctx, user);
    }
    
    [SlashCommand("remove", "Removes an Admin from the Database")]
    public async Task RemoveAdmin(InteractionContext ctx, [Option("user", "The admin to remove.")] DiscordUser user)
    {
        await AdminHandler.RemoveUserAdmin(ctx, user);
    }
}