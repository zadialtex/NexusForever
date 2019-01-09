using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server098B, MessageDirection.Server)]
    public class Server098B : IWritable
    {
        public class StoreCategory: IWritable
        {
            public class StoreItem : IWritable
            {
                public class UnknownStructure0 : IWritable
                {
                    public byte Unknown10 { get; set; } // 5
                    public uint Unknown11 { get; set; }
                    public byte Unknown12 { get; set; } // 2
                    public uint Unknown13 { get; set; }
                    public ulong Unknown14 { get; set; } // Should be signed long 
                    public ulong Unknown15 { get; set; }

                    public void Write(GamePacketWriter writer)
                    {
                        writer.Write(Unknown10, 5);
                        writer.Write(Unknown11);
                        writer.Write(Unknown12, 2);
                        writer.Write(Unknown13);
                        writer.Write(Unknown14);
                        writer.Write(Unknown15);
                    }
                }

                public class AccountItemData : IWritable
                {
                    public uint Type { get; set; } // Should be 0
                    public ushort AccountItemId { get; set; } // 15
                    public uint Unknown16 { get; set; }

                    public void Write(GamePacketWriter writer)
                    {
                        writer.Write(Type);
                        if (Type == 0)
                        {
                            writer.Write(AccountItemId, 15);
                            writer.Write(Unknown16);
                        }
                    }
                }

                public uint Unknown3 { get; set; }
                public string ItemName { get; set; }
                public string ItemDescription { get; set; }
                public ulong Unknown4 { get; set; }
                public uint Unknown5 { get; set; }
                public ulong Unknown6 { get; set; } // Should be singed long
                public byte Unknown7 { get; set; }
                public List<UnknownStructure0> Unknown8 { get; set; }
                public List<AccountItemData> Unknown9 { get; set; }

                public void Write(GamePacketWriter writer)
                {
                    writer.Write(Unknown3);
                    writer.WriteStringWide(ItemName);
                    writer.WriteStringWide(ItemDescription);
                    writer.Write(Unknown4);
                    writer.Write(Unknown5);
                    writer.Write(Unknown6);
                    writer.Write(Unknown7);

                    writer.Write(Unknown8.Count);
                    Unknown8.ForEach(e => e.Write(writer));

                    writer.Write(Unknown9.Count);
                    Unknown9.ForEach(e => e.Write(writer));
                }
            }

            public uint Unknown0 { get; set; }
            public uint Unknown1 { get; set; }
            public string SubCategoryName { get; set; }
            public string SubCategoryDescription { get; set; }
            public ushort Unknown2 { get; set; } // 14
            public List<StoreItem> AccountItemList { get; set; }
            public uint ArraySize { get; set; }
            public byte[] Unknown16 { get; set; } // Array1
            public byte[] Unknown17 { get; set; } // Array2


            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown0);
                writer.Write(Unknown1);
                writer.WriteStringWide(SubCategoryName);
                writer.WriteStringWide(SubCategoryDescription);
                writer.Write(Unknown2, 14);

                writer.Write(AccountItemList.Count);
                AccountItemList.ForEach(e => e.Write(writer));

                writer.Write(ArraySize);
                for (uint i = 0u; i < Unknown16.Length; i++)
                    writer.Write(Unknown16[i]);
                for (uint i = 0u; i < Unknown17.Length; i++)
                    writer.Write(Unknown17[i]);
            }
        }

        public List<StoreCategory> StoreCategories { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(StoreCategories.Count);
            StoreCategories.ForEach(e => e.Write(writer));
        }
    }
}
