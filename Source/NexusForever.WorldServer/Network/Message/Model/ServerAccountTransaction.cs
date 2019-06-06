using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountTransaction)]
    public class ServerAccountTransaction : IWritable
    {
        public string TransacationId { get; set; }
        public bool Redeemed { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.WriteStringWide(TransacationId);
            writer.Write(Redeemed);
        }
    }
}
