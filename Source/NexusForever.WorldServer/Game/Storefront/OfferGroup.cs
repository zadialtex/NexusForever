using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Game.Storefront.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NexusForever.WorldServer.Game.Storefront
{
    public class OfferGroup
    {
        public uint Id { get; }
        public DisplayFlag DisplayFlags { get; }
        public string Name { get; }
        public string Description { get; }
        public ushort Field2 { get; }
        public bool Visible { get; }
        public uint[] CategoryArray { get; }
        public uint[] CategoryIndexArray { get; }

        public List<OfferItem> offerItems = new List<OfferItem>();

        public OfferGroup(StoreOfferGroup model)
        {
            Id = model.Id;
            DisplayFlags = (DisplayFlag)model.DisplayFlags;
            Name = model.Name;
            Description = model.Description;
            Field2 = model.Field2;
            Visible = model.Visible;

            CategoryArray = CalculateCategoryArray(model.StoreOfferGroupCategory);
            CategoryIndexArray = CalculateCategoryArray(model.StoreOfferGroupCategory, true);

            foreach (StoreOfferItem offerItem in model.StoreOfferItem)
                offerItems.Add(new OfferItem(offerItem));
        }

        private uint[] CalculateCategoryArray(IEnumerable<StoreOfferGroupCategory> offerGroupCategories, bool indexCheck = false)
        {
            List<uint> CategoryArray = new List<uint>();

            foreach (StoreOfferGroupCategory category in offerGroupCategories)
            {
                if (category.Visible)
                    CategoryArray.Add(!indexCheck ? category.CategoryId : category.Index);
            }

            return CategoryArray.ToArray();
        }

        private IEnumerable<ServerStoreOffers.OfferGroup.Offer> GetOfferNetworkPackets()
        {
            foreach (OfferItem offer in offerItems)
                yield return offer.BuildNetworkPacket();
        }

        public ServerStoreOffers.OfferGroup BuildNetworkPacket()
        {
            return new ServerStoreOffers.OfferGroup
            {
                Id = Id,
                DisplayFlags = DisplayFlags,
                OfferGroupName = Name,
                OfferGroupDescription = Description,
                Unknown2 = Field2,
                ArraySize = (uint)CategoryArray.Length,
                CategoryArray = CategoryArray,
                CategoryIndexArray = CategoryIndexArray,
                Offers = GetOfferNetworkPackets().ToList()
            };
        }
    }
}
