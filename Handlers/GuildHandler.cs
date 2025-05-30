﻿using DisCatSharp;
using DisCatSharp.EventArgs;

namespace QuePoro.Handlers;

public class GuildHandler
{
    public static async Task MemberAdded(DiscordClient s, GuildMemberAddEventArgs e)
    {
        await e.Guild.Channels[0].SendMessageAsync($"Welcome {e.Member.Mention}!");
    }
}