using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Guild
{
    public class GuildInvite
    {
        public ulong GuildId { get; set; }
        public ulong InviteeId { get; set; }

        public GuildInvite() { }
    }
}
