using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountDailyRewards, MessageDirection.Server)]
    public class ServerAccountDailyRewards : IWritable
    {
        public uint DaysAvailable { get; set; }
        public uint Unknown1 { get; set; }
        public uint DaysClaimed { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
        public int Unknown5 { get; set; }
        public byte Unknown6 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(DaysAvailable); // It gives +1 day on top of whatever is sent in here. This is more like an index than day number.
            writer.Write(Unknown1);
            writer.Write(DaysClaimed);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
            writer.Write(Unknown5);
            writer.Write(Unknown6, 3);
        }
    }
}
