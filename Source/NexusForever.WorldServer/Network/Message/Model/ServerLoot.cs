using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerLoot)]
    public class ServerLoot : IWritable
    {
        public class LootItem : IWritable
        {
            public uint UniqueId { get; set; }
            public uint Type { get; set; }
            public uint StaticId { get; set; }
            public uint Amount { get; set; }
            public bool CanLoot { get; set; }
            public bool NeedsRoll { get; set; }
            public bool Explosion { get; set; }
            public bool Unknown1 { get; set; }
            public uint RollTime { get; set; }
            public ulong RandomCircuitData { get; set; }
            public ulong RandomGlyphData { get; set; }
            public uint Unknown2 { get; set; }
            public List<TargetPlayerIdentity> MasterList { get; set; } = new List<TargetPlayerIdentity>();

            public void Write(GamePacketWriter writer)
            {
                writer.Write(UniqueId);
                writer.Write(Type);
                writer.Write(StaticId);
                writer.Write(Amount);
                writer.Write(CanLoot);
                writer.Write(NeedsRoll);
                writer.Write(Explosion);
                writer.Write(Unknown1);
                writer.Write(RollTime);
                writer.Write(RandomCircuitData);
                writer.Write(RandomGlyphData);
                writer.Write(Unknown2);
                writer.Write(MasterList.Count, 32u);
                MasterList.ForEach(e => e.Write(writer));
            }
        }

        public uint UnitId { get; set; }
        public uint Unknown0 { get; set; }
        public bool Explosion { get; set; }
        public List<LootItem> LootItems { get; set; } = new List<LootItem>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.Write(Unknown0);

            writer.Write(LootItems.Count, 32u);
            LootItems.ForEach(e => e.Write(writer));
        }
    }
}
