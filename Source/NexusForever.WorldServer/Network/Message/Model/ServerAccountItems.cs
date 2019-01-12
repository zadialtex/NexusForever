using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountItems)]
    public class ServerAccountItems : IWritable
    {
        public List<AccountItem> AccountItems { get; set; } = new List<AccountItem>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountItems.Count, 32u);
            AccountItems.ForEach(w => w.Write(writer));
        }
    }
}
