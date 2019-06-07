using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0937)]
    public class Server0937 : IWritable
    {
        public uint UnitId { get; set; }
        public uint Value { get; set; }
        public bool Unknown0 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.Write(Value);
            writer.Write(Unknown0);
        }
    }
}
