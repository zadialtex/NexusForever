using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerPlayerGrantXp, MessageDirection.Server)]
    public class ServerPlayerGrantXp : IWritable
    {
        public uint TotalXpGained { get; set; }
        public uint RestXpAmount { get; set; }
        public uint SignatureXpAmount { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(TotalXpGained);
            writer.Write(RestXpAmount);
            writer.Write(SignatureXpAmount);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
        }
    }
}
