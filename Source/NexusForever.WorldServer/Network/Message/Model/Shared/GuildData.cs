using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Guild.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GuildData: IWritable
    {
        public class Info: IWritable
        {
            public string MessageOfTheDay { get; set; }
            public string AdditionalInfo { get; set; }
            public byte Unknown2 { get; set; } // 3
            public uint Unknown3 { get; set; }
            public uint Unknown4 { get; set; }
            public uint Unknown5 { get; set; }
            public uint Unknown6 { get; set; }
            public uint Unknown7 { get; set; }
            public uint Unknown8 { get; set; }
            public float AgeInDays { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.WriteStringWide(MessageOfTheDay);
                writer.WriteStringWide(AdditionalInfo);
                writer.Write(Unknown2, 3u);
                writer.Write(Unknown3);
                writer.Write(Unknown4);
                writer.Write(Unknown5);
                writer.Write(Unknown6);
                writer.Write(Unknown7);
                writer.Write(Unknown8);
                writer.Write(AgeInDays);
            }
        }

        public class UnknownStructure1: IWritable
        {
            public ushort Unknown0 { get; set; } // 14
            public uint Unknown1 { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown0, 14u);
                writer.Write(Unknown1);
            }
        }

        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
        public uint Taxes { get; set; }
        public GuildType Type { get; set; } // 4

        public List<GuildRank> Ranks { get; set; } = new List<GuildRank>(new GuildRank[10]);

        public GuildStandard GuildStandard { get; set; } = new GuildStandard();

        public uint TotalMembers { get; set; }
        public uint UsersOnline { get; set; }
        public uint CurrentInfluence { get; set; }
        public uint DailyInfluenceRemaining { get; set; }
        public ulong BankCurrency { get; set; }
        public uint Unknown6 { get; set; }
        public uint Unknown7 { get; set; }

        public List<string> BankTabNames { get; set; } = new List<string>(new string[10]);

        public byte[] Perks { get; set; } = new byte[16];

        public List<UnknownStructure1> Unknown10 { get; set; } = new List<UnknownStructure1>();

        public Info GuildInfo { get; set; } = new Info();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GuildId);
            writer.WriteStringWide(GuildName);
            writer.Write(Taxes);
            writer.Write(Type, 4u);

            if (Ranks.Count < 10)
                for (int i = Ranks.Count; i < 10; i++)
                    Ranks.Add(new GuildRank());
            Ranks.ForEach(c => c.Write(writer));

            GuildStandard.Write(writer);

            writer.Write(TotalMembers);
            writer.Write(UsersOnline);
            writer.Write(CurrentInfluence);
            writer.Write(DailyInfluenceRemaining);
            writer.Write(BankCurrency);
            writer.Write(Unknown6);
            writer.Write(Unknown7);

            foreach (string str in BankTabNames)
                writer.WriteStringWide(str);

            writer.WriteBytes(Perks);
            writer.Write(Unknown10.Count);

            if (Unknown10.Count <= 0)
                GuildInfo.Write(writer);
            else
                Unknown10.ForEach(c => c.Write(writer));
        }
    }
}
