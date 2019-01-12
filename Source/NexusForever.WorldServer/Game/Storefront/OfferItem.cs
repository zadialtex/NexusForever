using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Game.Storefront.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NexusForever.WorldServer.Game.Storefront
{
    public class OfferItem
    {
        public uint Id { get; }
        public string Name { get; }
        public string Description { get; }
        public DisplayFlag DisplayFlags { get; }
        public long Field6 { get; }
        public byte Field7 { get; }
        public bool Visible { get; }

        private List<OfferItemData> itemDataList { get; set; } = new List<OfferItemData>();
        private Dictionary<byte, OfferItemPrice> prices { get; set; } = new Dictionary<byte, OfferItemPrice>();

        public OfferItem(StoreOfferItem model)
        {
            Id = model.Id;
            Name = model.Name;
            Description = model.Description;
            DisplayFlags = (DisplayFlag)model.DisplayFlags;
            Field6 = model.Field6;
            Field7 = model.Field7;
            Visible = model.Visible;

            foreach (StoreOfferItemData itemData in model.StoreOfferItemData)
                itemDataList.Add(new OfferItemData(itemData));
                
            foreach (StoreOfferItemPrice price in model.StoreOfferItemPrice)
            {
                if (!StorefrontManager.CurrencyProtobucksEnabled && price.CurrencyId == 11)
                    continue;
                if (!StorefrontManager.CurrencyOmnibitsEnabled && price.CurrencyId == 6)
                    continue;

                OfferItemPrice itemPrice = new OfferItemPrice(price);
                prices.Add(itemPrice.CurrencyId, itemPrice);
            }
        }

        private IEnumerable<ServerStoreOffers.OfferGroup.Offer.OfferItemData> GetItemNetworkPackets()
        {
            foreach (OfferItemData item in itemDataList)
                yield return item.BuildNetworkPacket();
        }

        private IEnumerable<ServerStoreOffers.OfferGroup.Offer.OfferCurrencyData> GetPricingNetworkPackets()
        {
            foreach (OfferItemPrice price in prices.Values)
                yield return price.BuildNetworkPacket();
        }

        public ServerStoreOffers.OfferGroup.Offer BuildNetworkPacket()
        {
            float priceProtobucks = 0;
            float priceOmnibits = 0;

            if (prices.TryGetValue(11, out OfferItemPrice protobucksItemPrice))
                priceProtobucks = protobucksItemPrice.GetCurrencyValue();
            if (prices.TryGetValue(6, out OfferItemPrice omnibitsItemPrice))
                priceOmnibits = omnibitsItemPrice.GetCurrencyValue();

            return new ServerStoreOffers.OfferGroup.Offer
            {
                Id = Id,
                OfferName = Name,
                OfferDescription = Description,
                DisplayFlags = DisplayFlags,
                PriceProtobucks = priceProtobucks,
                PriceOmnibits = priceOmnibits,
                Unknown6 = Field6,
                Unknown7 = Field7,
                ItemData = GetItemNetworkPackets().ToList(),
                CurrencyData = GetPricingNetworkPackets().ToList()
            };
        }
    }
}
