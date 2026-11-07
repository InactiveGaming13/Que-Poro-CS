using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;

namespace QuePoro.Handlers;

[SlashCommandGroup("timer", "Timer commands")]
public class TimerCommandHandler : ApplicationCommandsModule
{
    /// <summary>
    /// A command that adds a timer.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="type"></param>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="user"></param>
    /// <param name="repeat"></param>
    /// <param name="duration"></param>
    /// <param name="time"></param>
    public async Task AddTimer(InteractionContext e, 
        [Option("message", "The message to send when the timer is up")] string message,
        [Option("channel", "The channel to send the message in")] DiscordChannel? channel = null,
        [Option("user", "The user to DM (BOT ADMIN ONLY)")] DiscordUser? user = null,
        [Option("repeat", "Whether to repeat the timer or not")] bool repeat = false,
        [Option("duration", "The duration of the timer in seconds")] int duration = 3600,
        [Option("time", "The time for the alarm")] TimeOnly time = default)
    {
        // Implementation for adding a timer
    }
    
    /// <summary>
    /// A command that adds an alarm.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="user"></param>
    /// <param name="repeat"></param>
    /// <param name="duration"></param>
    /// <param name="time"></param>
    public async Task AddAlarm(InteractionContext e, 
        [Option("message", "The message to send when the timer is up")] string message,
        [Option("channel", "The channel to send the message in")] DiscordChannel? channel = null,
        [Option("user", "The user to DM (BOT ADMIN ONLY)")] DiscordUser? user = null,
        [Option("repeat", "Whether to repeat the timer or not")] bool repeat = false,
        [Option("duration", "The duration of the timer in seconds")] int duration = 3600,
        [Option("time", "The time for the alarm")] TimeOnly time = default)
    {
        // Implementation for adding a timer
    }
}

public class TimerHandler
{
    public async Task HandleTimedEvents()
    {
        
    }
}