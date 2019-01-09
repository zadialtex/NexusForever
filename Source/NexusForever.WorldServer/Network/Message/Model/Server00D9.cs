using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{

    [Message(GameMessageOpcode.Server00D9, MessageDirection.Server)]
    public class Server00D9 : IWritable
    {
        public class UnknownItem: IWritable
        {
            public uint Unknown0 { get; set; }
            public int Unknown1 { get; set; }
            public byte Unknown2 { get; set; }
            public uint[] Unknown3 { get; set; } = new uint[28];
            public uint[] Unknown4 { get; set; } = new uint[28];

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown0);
                writer.Write(Unknown1);
                writer.Write(Unknown2, 2);

                for (uint i = 0u; i < Unknown3.Length; i++)
                    writer.Write(Unknown3[i]);

                for (uint i = 0u; i < Unknown4.Length; i++)
                    writer.Write(Unknown4[i]);
            }
        }

        public List<UnknownItem> UnknownCostumes = new List<UnknownItem>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnknownCostumes.Count);
            UnknownCostumes.ForEach(e => e.Write(writer));
        }
    }
}
