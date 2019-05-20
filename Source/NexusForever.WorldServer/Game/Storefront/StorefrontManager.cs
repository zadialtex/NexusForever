using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NexusForever.WorldServer.Database.World;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Game.Storefront.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Game.Storefront
{
    public static class StorefrontManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static ImmutableList<ServerStoreCategories.StoreCategory> StoreCategoryList { get; set; }
        private static ImmutableList<ServerStoreOffers.OfferGroup> StoreOfferGroupList { get; set; }

        private static ImmutableList<StoreOfferGroupCategory> StoreOfferGroupCategoryList { get; set; }
        private static ImmutableList<StoreOfferItem> StoreOfferItemList { get; set; }
        private static ImmutableList<StoreOfferItemData> StoreOfferItemDataList { get; set; }
        private static ImmutableList<StoreOfferItemCurrency> StoreOfferItemCurrencyList { get; set; }

        public static void Initialise()
        {
            // Need to initialise in reverse to ensure filtering works
            InitialiseStoreOfferItemCurrencies();
            InitialiseStoreOfferItemData();
            InitialiseStoreOfferItems();
            InitialiseStoreOfferGroupCategories();
            InitialiseStoreOfferGroups();
            InitialiseStoreCategories();
        }

        private static void InitialiseStoreCategories()
        {
            List<StoreCategory> StoreCategories = WorldDatabase.GetStoreCategories()
                .OrderBy(i => i.Id)
                .Where(a => StoreOfferGroupList.Any(x => x.CategoryArray.Contains((byte)a.Id)))
                .ToList();
            StoreCategories.Remove(StoreCategories.Find(x => x.Id == 26));

            StoreCategoryList = StoreCategories
              .Select(x => new ServerStoreCategories.StoreCategory()
              {
                  CategoryId = x.Id,
                  CategoryName = x.Name,
                  CategoryDesc = x.Description,
                  ParentCategoryId = x.ParentCategoryId,
                  Index = x.Index,
                  Visible = x.Visible
              })
              .ToImmutableList();
        }

        private static void InitialiseStoreOfferGroups()
        {
            List<StoreOfferGroup> StoreOfferGroups = WorldDatabase.GetStoreOfferGroups()
                .OrderBy(i => i.Id)
                .Where(a => StoreOfferItemList.Any(x => x.GroupId == a.Id))
                .ToList();
            foreach (var offerGroup in StoreOfferGroups.FindAll(x => x.Visible == false))
                StoreOfferGroups.Remove(offerGroup);


            StoreOfferGroupList = StoreOfferGroups
                .Select(x => new ServerStoreOffers.OfferGroup()
                {
                    Id = x.Id,
                    DisplayFlags = (DisplayFlag)x.DisplayFlags,
                    OfferGroupName = x.Name,
                    OfferGroupDescription = x.Description,
                    Unknown2 = x.Field2,
                    Offers = GetOffersForGroup(x.Id),
                    ArraySize = (uint)(CalculateCategoryArray(StoreOfferGroupCategoryList.Where(i => i.Id == x.Id).ToList()).Length / 4),
                    CategoryArray = CalculateCategoryArray(StoreOfferGroupCategoryList.Where(i => i.Id == x.Id).ToList()),
                    CategoryIndexArray = CalculateCategoryArray(StoreOfferGroupCategoryList.Where(i => i.Id == x.Id).ToList(), true)
                })
                .ToImmutableList();            
        }

        private static void InitialiseStoreOfferGroupCategories()
        {
            List<StoreOfferGroupCategory> StoreOfferGroupCategories = WorldDatabase.GetStoreOfferGroupCategories()
                .OrderBy(i => i.Id)
                .ToList();
                
            foreach (var groupCategory in StoreOfferGroupCategories.FindAll(x => x.Visible == false))
                StoreOfferGroupCategories.Remove(groupCategory);

            StoreOfferGroupCategoryList = StoreOfferGroupCategories.ToImmutableList();
        }

        private static byte[] CalculateCategoryArray(List<StoreOfferGroupCategory> offerGroupCategories, bool indexCheck = false)
        {
            List<byte> CategoryArray = new List<byte>();

            foreach (StoreOfferGroupCategory category in offerGroupCategories)
            {
                if(category.Visible)
                {
                    CategoryArray.Add(!indexCheck ? (byte)category.CategoryId : category.Index);
                    CategoryArray.Add(0);
                    CategoryArray.Add(0);
                    CategoryArray.Add(0);
                }
            }

            return CategoryArray.ToArray();
        }

        private static void InitialiseStoreOfferItems()
        {
            List<StoreOfferItem> StoreOfferItems = WorldDatabase.GetStoreOfferItems()
                .OrderBy(i => i.Id)
                .Where(a => StoreOfferItemDataList.Any(x => x.OfferId == a.Id))
                .ToList();
            foreach (var offerGroup in StoreOfferItems.FindAll(x => x.Visible == false))
                StoreOfferItems.Remove(offerGroup);

            StoreOfferItemList = StoreOfferItems.ToImmutableList();
        }

        private static List<ServerStoreOffers.OfferGroup.Offer> GetOffersForGroup(uint groupId)
        {
            List<ServerStoreOffers.OfferGroup.Offer> StoreItemsToSend = StoreOfferItemList
                .Where(x => x.GroupId == groupId)
                .Select(x => new ServerStoreOffers.OfferGroup.Offer()
                {
                    Id = x.Id,
                    OfferName = x.Name,
                    OfferDescription = x.Description,
                    PriceProtobucks = GetCurrencyValue(x.Id, 11),
                    PriceOmnibits = GetCurrencyValue(x.Id, 6),
                    DisplayFlags = (DisplayFlag)x.DisplayFlags,
                    Unknown6 = x.Field6,
                    Unknown7 = x.Field7,
                    CurrencyData = GetCurrencyDataForOffer(x.Id),
                    ItemData = GetItemDataForOffer(x.Id)
                })
                .ToList();

            return StoreItemsToSend;
        }

        private static void InitialiseStoreOfferItemData()
        {
            List<StoreOfferItemData> StoreOfferItemsData = WorldDatabase.GetStoreOfferItemsData()
                .OrderBy(i => i.Id)
                .ToList();

            StoreOfferItemDataList = StoreOfferItemsData.ToImmutableList();
        }

        private static List<ServerStoreOffers.OfferGroup.Offer.OfferItemData> GetItemDataForOffer(uint offerId)
        {
            List<ServerStoreOffers.OfferGroup.Offer.OfferItemData> StoreItemDataToSend = StoreOfferItemDataList
                .Where(x => x.OfferId == offerId)
                .Select(x => new ServerStoreOffers.OfferGroup.Offer.OfferItemData()
                {
                    Type = x.Type,
                    AccountItemId = x.ItemId,
                    Amount = x.Amount
                })
                .ToList();

            return StoreItemDataToSend;
        }

        private static void InitialiseStoreOfferItemCurrencies()
        {
            List<StoreOfferItemCurrency> StoreOfferItemCurrencies = WorldDatabase.GetStoreOfferItemCurrencies()
                .OrderBy(i => i.OfferId)
                .ToList();

            StoreOfferItemCurrencyList = StoreOfferItemCurrencies.ToImmutableList();
        }

        private static List<ServerStoreOffers.OfferGroup.Offer.OfferCurrencyData> GetCurrencyDataForOffer(uint offerId)
        {
            List<ServerStoreOffers.OfferGroup.Offer.OfferCurrencyData> StoreCurrencyToSend = StoreOfferItemCurrencyList
                .Where(x => x.OfferId == offerId)
                .Select(x => new ServerStoreOffers.OfferGroup.Offer.OfferCurrencyData()
                {
                    CurrencyId = x.CurrencyId,
                    Price = x.Price,
                    Unknown12 = x.Field12,
                    DiscountPercent = x.DiscountPercent,
                    Unknown14 = x.Field14,
                    ExpiryTimestamp = 1995405795 // x.Expiry
                })
                .ToList();

            return StoreCurrencyToSend;
        }

        private static uint GetCurrencyValue(uint offerId, byte currencyId)
        {
            var currencyEntry = StoreOfferItemCurrencyList.FirstOrDefault(x => x.OfferId == offerId && x.CurrencyId == currencyId);
            if (currencyEntry == null)
                return 0;

            if (currencyEntry.DiscountPercent == 0f)
                return (uint)Math.Ceiling(currencyEntry.Price);
            else
            {
                return (uint)Math.Ceiling(currencyEntry.Price); // This is the sale price

                // TODO: Use below formula to send original price to users ineligible for the offer.
                //return (uint)Math.Ceiling(currencyEntry.Price / ((100f - currencyEntry.DiscountPercent) / 100)); // This gives the full price of the item
            }
        }

        public static void SendLoadSequence(WorldSession session)
        {
            SendStoreCategories(session);
            SendStoreOffers(session);
            SendStoreFinalise(session);
        }

        private static void SendStoreCategories(WorldSession session)
        {
            session.EnqueueMessageEncrypted(new ServerStoreCategories
            {
                StoreCategories = StoreCategoryList.ToList(),
                Unknown4 = 4
            });
        }

        private static void SendStoreOffers(WorldSession session)
        {
            List<ServerStoreOffers.OfferGroup> offersToSend = new List<ServerStoreOffers.OfferGroup>();
            uint count = 0;

            foreach(ServerStoreOffers.OfferGroup offerGroup in StoreOfferGroupList)
            {
                count++;
                offersToSend.Add(offerGroup);
                if(count == 20 || StoreOfferGroupList.IndexOf(offerGroup) == StoreOfferGroupList.Count - 1)
                {
                    session.EnqueueMessageEncrypted(new ServerStoreOffers
                    {
                        OfferGroups = offersToSend
                    });
                    offersToSend.RemoveRange(0, (int)count);
                    count = 0;
                }
            }
        }

        private static void SendStoreFinalise(WorldSession session)
        {
            session.EnqueueMessageEncrypted(new ServerStoreFinalise());
        }
    }
}
