using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server098C, MessageDirection.Server)]
    public class Server098C : IWritable
    {
        public bool Unknown0 { get; set; }
        public byte Unknown1 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unknown0);
            writer.Write(Unknown1, 5);
        }
    }
}
