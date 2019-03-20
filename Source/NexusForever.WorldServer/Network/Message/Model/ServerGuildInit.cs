using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{

    [Message(GameMessageOpcode.ServerGuildInit)]
    public class ServerGuildInit : IWritable
    {
        public uint Unknown0 { get; set; }
        public List<GuildData> Guilds { get; set; } = new List<GuildData>();
        public List<GuildMember> PlayerMemberships { get; set; } = new List<GuildMember>();
        public List<GuildUnknown> GuildUnknownList { get; set; } = new List<GuildUnknown>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guilds.Count);
            writer.Write(Unknown0);
            Guilds.ForEach(w => w.Write(writer));
            PlayerMemberships.ForEach(w => w.Write(writer));
            GuildUnknownList.ForEach(w => w.Write(writer));
        }
    }
}
