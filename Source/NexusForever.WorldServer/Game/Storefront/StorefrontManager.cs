using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NexusForever.WorldServer.Database.World;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Storefront
{
    public static class StorefrontManager
    {
        private static ImmutableList<StoreCategory> StoreCategoryList { get; set; }

        public static void Initialise()
        {
            InitialiseStoreInfo();
        }

        public static void InitialiseStoreInfo()
        {
            //ImmutableDictionary<uint, ImmutableList<StoreCategory>> storeCategories = WorldDatabase.GetStoreCategories()
            //    .GroupBy(i => i.Id)
            //    .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());

            StoreCategoryList = WorldDatabase.GetStoreCategories()
                .OrderBy(i => i.Id)
                .ToImmutableList();
            StoreCategoryList.Remove(StoreCategoryList.FirstOrDefault(x => x.Id == 26));
        }

        public static void SendStoreCategories(WorldSession session)
        {
            var ServerStoreCategoriesList = StoreCategoryList
              .Select(x => new ServerStoreCategories.StoreCategory()
              {
                  CategoryId = x.Id,
                  CategoryName = x.Name,
                  CategoryDesc = x.Description,
                  ParentCategoryId = x.ParentCategoryId,
                  Index = x.Index,
                  Visible = x.Visible
              })
              .ToList();

            session.EnqueueMessageEncrypted(new ServerStoreCategories
            {
                StoreCategories = ServerStoreCategoriesList,
                Unknown4 = 4
            });
            session.EnqueueMessageEncrypted(new ServerStoreFinalise());
        }
    }
}
