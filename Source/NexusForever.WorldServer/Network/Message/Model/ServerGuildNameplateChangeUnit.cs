using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Guild.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGuildNameplateSet)]
    public class ServerGuildNameplateChangeUnit : IWritable
    {
        public uint UnitId { get; set; }
        public string GuildName { get; set; }
        public GuildType GuildType { get; set; } // 4

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.WriteStringWide(GuildName);
            writer.Write(GuildType, 4u);
        }
    }
}
