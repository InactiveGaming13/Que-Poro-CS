using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace Que_Poro_CS;

[SlashCommandGroup("voice", "Voice commands")]
public class VoiceCommands : ApplicationCommandsModule
{
    [SlashCommand("join", "Joins a Voice Channel")]
    public async Task Join(InteractionContext ctx, [Option("channel", "Channel to join"), ChannelTypes(ChannelType.Voice)] DiscordChannel channel)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = $"Joining {channel.Name}"
            });
        await VoiceHandler.Connect(channel);
        await ctx.EditResponseAsync($"Joined {channel.Name}");
    }

    [SlashCommand("leave", "Leaves a Voice Channel")]
    public async Task Leave(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            {
                Content = $"Leaving {ctx.Channel.Name}"
            });
        await VoiceHandler.Disconnect(ctx.Client, ctx.Guild);
    }
}

[SlashCommandGroup("testers", "Voice commands")]
public class TesterCommands : ApplicationCommandsModule
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
public class ConfigCommands : ApplicationCommandsModule
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
public class ReactionCommands : ApplicationCommandsModule
{
    [SlashCommand("add", "Adds a Reaction to you or a specified user (admin)")]
    public async Task Add(InteractionContext ctx,
        [Option("emoji", "emoji you want to have reacted")] String emoji,
        [Option("user", "The user you want to add to your reactions")] DiscordUser user = null)
    {
        if (user != null && ctx.User != user && !ctx.User.IsStaff)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"You are not authorized to add a reaction to other users."
                });
        } else if (ctx.User == user || user == null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"Added {emoji} to your reactions."
                });
        } else if (ctx.User != user && ctx.User.IsStaff)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"Added {emoji} to {user.Mention} reactions."
                });
        }
    }
}