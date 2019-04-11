using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Storefront.Static;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerStoreOffers)]
    public class ServerStoreOffers : IWritable
    {
        public class OfferGroup : IWritable
        {
            public class Offer : IWritable
            {
                public class OfferCurrencyData : IWritable
                {
                    public byte CurrencyId { get; set; } // 5
                    public uint Unknown11 { get; set; }
                    public byte Unknown12 { get; set; } // 2
                    public uint Unknown13 { get; set; }
                    public double Unknown14 { get; set; }
                    public double ExpiryTimestamp { get; set; }

                    public void Write(GamePacketWriter writer)
                    {
                        writer.Write(CurrencyId, 5);
                        writer.Write(Unknown11);
                        writer.Write(Unknown12, 2);
                        writer.Write(Unknown13);
                        writer.Write(Unknown14);
                        writer.Write(ExpiryTimestamp);
                    }
                }

                public class OfferItemData : IWritable
                {
                    public uint Type { get; set; } // Should be 0
                    public ushort AccountItemId { get; set; } // 15
                    public uint Amount { get; set; }

                    public void Write(GamePacketWriter writer)
                    {
                        writer.Write(Type);
                        if (Type == 0)
                        {
                            writer.Write(AccountItemId, 15);
                            writer.Write(Amount);
                        }
                    }
                }

                public uint Id { get; set; }
                public string OfferName { get; set; }
                public string OfferDescription { get; set; }
                public byte[] PriceArray { get; set; } = new byte[8];
                public DisplayFlag DisplayFlags { get; set; } // Probably PremiumTier level
                public double Unknown6 { get; set; } 
                public byte Unknown7 { get; set; } 
                public List<OfferCurrencyData> CurrencyData { get; set; } = new List<OfferCurrencyData>();
                public List<OfferItemData> ItemData { get; set; } = new List<OfferItemData>();

                public void Write(GamePacketWriter writer)
                {
                    writer.Write(Id);
                    writer.WriteStringWide(OfferName);
                    writer.WriteStringWide(OfferDescription);

                    writer.WriteBytes(PriceArray);

                    writer.Write(DisplayFlags, 32u);
                    writer.Write(Unknown6);
                    writer.Write(Unknown7);

                    writer.Write(CurrencyData.Count);
                    CurrencyData.ForEach(e => e.Write(writer));

                    writer.Write(ItemData.Count);
                    ItemData.ForEach(e => e.Write(writer));
                }
            }

            public uint Id { get; set; }
            public DisplayFlag DisplayFlags { get; set; } // Probably PremiumTier level
            public string OfferGroupName { get; set; }
            public string OfferGroupDescription { get; set; }
            public ushort Unknown2 { get; set; }
            public List<Offer> Offers { get; set; } = new List<Offer>();
            public uint ArraySize { get; set; }
            public byte[] CategoryArray { get; set; }
            public byte[] CategoryIndexArray { get; set; }


            public void Write(GamePacketWriter writer)
            {
                writer.Write(Id);
                writer.Write(DisplayFlags, 32u);
                writer.WriteStringWide(OfferGroupName);
                writer.WriteStringWide(OfferGroupDescription);
                writer.Write(Unknown2, 14u);

                writer.Write(Offers.Count);
                Offers.ForEach(e => e.Write(writer));

                writer.Write(ArraySize);
                writer.WriteBytes(CategoryArray);
                writer.WriteBytes(CategoryIndexArray);
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