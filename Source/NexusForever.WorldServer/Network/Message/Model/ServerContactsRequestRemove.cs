using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerContactsRequestRemove, MessageDirection.Server)]
    public class ServerContactsRequestRemove : IWritable
    {
        public ulong ContactId { get; set; } = 0;

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ContactId);
        }
    }
}
