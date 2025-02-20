using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.VoiceNext;

namespace Que_Poro_CS;

public class VoiceHandler : ApplicationCommandsModule
{
    public static async Task Connect(DiscordChannel channel)
    { 
        await channel.ConnectAsync();
    }

    public static async Task Disconnect(DiscordClient client, DiscordGuild guild)
    {
        var vnext = client.GetVoiceNext();
        var connection = vnext.GetConnection(guild);
        
        connection.Disconnect();
    }

    public static async Task UserAdded(DiscordChannel channel)
    {
        
    }

    public static async Task UserRemoved(DiscordChannel channel)
    {
        
    }
}