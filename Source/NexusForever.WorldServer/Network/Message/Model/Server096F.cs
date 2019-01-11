using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server096F, MessageDirection.Server)]
    public class Server096F : IWritable
    {
        public uint Unknown0 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unknown0);
        }
    }
}
