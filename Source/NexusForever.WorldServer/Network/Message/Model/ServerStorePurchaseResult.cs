using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerStorePurchaseResult)]
    public class ServerStorePurchaseResult : IWritable
    {
        public bool Success { get; set; }
        public byte Unknown0 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Success);
            writer.Write(Unknown0, 5u);
        }
    }
}
