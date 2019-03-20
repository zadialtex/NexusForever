using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{

    [Message(GameMessageOpcode.ServerGuildJoin)]
    public class ServerGuildJoin : IWritable
    {
        public GuildData GuildData { get; set; } = new GuildData();
        public GuildMember PlayerMembership { get; set; } = new GuildMember();
        public GuildUnknown GuildUnknown { get; set; } = new GuildUnknown();
        public bool Unknown0 { get; set; } = false;

        public void Write(GamePacketWriter writer)
        {
            GuildData.Write(writer);
            PlayerMembership.Write(writer);
            GuildUnknown.Write(writer);
            writer.Write(Unknown0);
        }
    }
}
