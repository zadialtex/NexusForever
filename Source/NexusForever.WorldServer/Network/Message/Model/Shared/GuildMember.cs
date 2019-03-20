using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GuildMember: IWritable
    {
        public ushort Realm { get; set; }
        public ulong CharacterId { get; set; }
        public uint Rank { get; set; }
        public string Name { get; set; }
        public Sex Sex { get; set; } // 2
        public Class Class { get; set; }
        public Path Path { get; set; }
        public uint Level { get; set; }
        public uint Unknown5 { get; set; }
        public uint Unknown6 { get; set; }
        public uint Unknown7 { get; set; }
        public uint Unknown8 { get; set; }
        public string Note { get; set; }
        public uint Unknown10 { get; set; } = 1;
        public float LastOnline { get; set; } = -1f;

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Realm, 14u);
            writer.Write(CharacterId);
            writer.Write(Rank);
            writer.WriteStringWide(Name);
            writer.Write(Sex, 2u);
            writer.Write(Class, 32u);
            writer.Write(Path, 32u);
            writer.Write(Level);
            writer.Write(Unknown5);
            writer.Write(Unknown6);
            writer.Write(Unknown7);
            writer.Write(Unknown8);
            writer.WriteStringWide(Note);
            writer.Write(Unknown10);
            writer.Write(LastOnline);
        }
    }
}
