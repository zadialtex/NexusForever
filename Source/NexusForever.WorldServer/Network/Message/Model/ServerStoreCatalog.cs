using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerStoreCatalog, MessageDirection.Server)]
    public class ServerStoreCatalog : IWritable
    {
        public class OfferGroup: IWritable
        {
            public class Offer : IWritable
            {
                public class UnknownStructure0 : IWritable // 99% sure that this is "currency & costs"
                {
                    public byte CurrencyId { get; set; } // 5
                    public uint Unknown11 { get; set; }
                    public byte Unknown12 { get; set; } // 2
                    public uint Unknown13 { get; set; }
                    public double Unknown14 { get; set; }
                    public double Unknown15 { get; set; }

                    public void Write(GamePacketWriter writer)
                    {
                        writer.Write(CurrencyId, 5);
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

                public uint Unknown3 { get; set; } // nId
                public string OfferName { get; set; } // strVariantName
                public string OfferDescription { get; set; } // strVariantDescription
                public byte[] PriceArray { get; set; } = new byte[8]; 
                public uint Unknown5 { get; set; } // nRequiredTIer?
                public long Unknown6 { get; set; } // tPremium?
                public byte Unknown7 { get; set; } // tAlternative?
                public List<UnknownStructure0> Unknown8 { get; set; } = new List<UnknownStructure0>(); // tPrices
                public List<AccountItemData> Unknown9 { get; set; } = new List<AccountItemData>(); // Item

                public void Write(GamePacketWriter writer)
                {
                    writer.Write(Unknown3);
                    writer.WriteStringWide(OfferName);
                    writer.WriteStringWide(OfferDescription);

                    writer.WriteBytes(PriceArray);

                    writer.Write(Unknown5);
                    writer.Write(Unknown6);
                    writer.Write(Unknown7);

                    writer.Write(Unknown8.Count);
                    Unknown8.ForEach(e => e.Write(writer));

                    writer.Write(Unknown9.Count);
                    Unknown9.ForEach(e => e.Write(writer));
                }
            }

            public uint Unknown0 { get; set; } // nId
            public uint Unknown1 { get; set; }
            public string OfferGroupName { get; set; } // strName
            public string OfferGroupDescription { get; set; } // strDescription
            public ushort Unknown2 { get; set; } // 14 nNumVariants?
            public List<Offer> Offers { get; set; } = new List<Offer>(); // Or is Offers count = nNumVariants
            public uint ArraySize { get; set; }
            public byte[] Unknown16 { get; set; } // Array1 // nDisplayInfoOverride
            public byte[] Unknown17 { get; set; } // Array2 // nFlags


            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown0);
                writer.Write(Unknown1);
                writer.WriteStringWide(OfferGroupName);
                writer.WriteStringWide(OfferGroupDescription);
                writer.Write(Unknown2, 14);

                writer.Write(Offers.Count);
                Offers.ForEach(e => e.Write(writer));

                writer.Write(ArraySize);
                for (uint i = 0u; i < Unknown16.Length; i++)
                    writer.Write(Unknown16[i]);
                for (uint i = 0u; i < Unknown17.Length; i++)
                    writer.Write(Unknown17[i]);
            }
        }

        public List<OfferGroup> OfferGroups { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(OfferGroups.Count);
            OfferGroups.ForEach(e => e.Write(writer));
        }
    }
}
