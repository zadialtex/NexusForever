using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0987, MessageDirection.Server)]
    public class Server0987 : IWritable
    {
        public void Write(GamePacketWriter writer)
        {
        }
    }
}
