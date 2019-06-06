using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Storefront
{
    public class OfferItemData
    {
        public uint Id { get; set; }
        public uint OfferId { get; set; }
        public uint Type { get; set; }
        public ushort ItemId { get; set; }
        public uint Amount { get; set; }

        public AccountItemEntry Entry { get; private set; }

        public OfferItemData(StoreOfferItemData model)
        {
            Id = model.Id;
            OfferId = model.OfferId;
            Type = model.Type;
            ItemId = model.ItemId;
            Amount = model.Amount;

            Entry = GameTableManager.AccountItem.GetEntry(ItemId);
            if (Entry == null)
                throw new ArgumentNullException("Item was not found in AccountItem table.");
        }

        public ServerStoreOffers.OfferGroup.Offer.OfferItemData BuildNetworkPacket()
        {
            return new ServerStoreOffers.OfferGroup.Offer.OfferItemData
            {
                Type = Type,
                AccountItemId = ItemId,
                Amount = Amount
            };
        }
    }
}
