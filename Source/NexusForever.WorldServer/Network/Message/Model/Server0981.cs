using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0981, MessageDirection.Server)]
    public class Server0981 : IWritable
    {
        public List<byte[]> AccountItemList { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountItemList.Count);
            foreach(var accountItem in AccountItemList)
                for (uint i = 0u; i < accountItem.Length; i++)
                    writer.Write(accountItem[i], 8);
        }
    }
}
