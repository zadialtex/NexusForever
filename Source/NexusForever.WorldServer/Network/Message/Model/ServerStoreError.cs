using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Storefront.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerStoreError)]
    public class ServerStoreError : IWritable
    {
        public StoreError Error { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Error, 5u);
        }
    }
}
