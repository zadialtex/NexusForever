using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0981, MessageDirection.Server)]
    public class Server0981 : IWritable
    {
        public byte[] AccountItemList { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountItemList.Length, 32);
            for (uint i = 0u; i < AccountItemList.Length; i++)
                writer.Write(AccountItemList[i], 8);
        }
    }
}
