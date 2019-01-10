using NexusForever.Shared.Database.Auth;
using NexusForever.Shared.Database.Auth.Model;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class AccountStoreHandler
    {
        [MessageHandler(GameMessageOpcode.ClientStorefrontRequestCatalog)]
        public static void HandleStorefrontRequestCatalogRealm(WorldSession session, ClientStorefrontRequestCatalog storefrontRequest)
        {
            // 0x096D - Account inventory
            session.EnqueueMessageEncrypted(new Server096D
            {
                AccountItemList = new List<Server096D.AccountItem>
                    {
                        new Server096D.AccountItem
                        {
                            Unknown0 = 234459813,
                            Unknown1 = 19,
                            Unknown2 = 0,
                            Unknown3 = false,
                            RealmId = 0,
                            CharacterId = 0
                        }
                    }

            });

            // 0x0968 - Entitlements?

            // 0x097F

            // 0x0966 - SetAccountCurrencyAmounts

            // 0x096F

            // 0x096E

            // 0x0981 - Looks to be Entries from AccountItem Table
            session.EnqueueMessageEncrypted(new Server0981
            {
                AccountItemList = new List<byte[]>
                    {
                        new byte[4]
                        {
                            78, 0, 0, 0
                        }
                    }
            });

            // 0x098B - Store catalogue items
            session.EnqueueMessageEncrypted(new ServerStoreCatalog
            {
                StoreCategories = new List<ServerStoreCatalog.StoreCategory>
                    {
                        new ServerStoreCatalog.StoreCategory
                        {
                            Unknown0 = 1550,
                            Unknown1 = 0,
                            SubCategoryName = "Costume Slot Unlock",
                            SubCategoryDescription = "Unlocks an additional costume slot.",
                            Unknown2 = 0,
                            AccountItemList = new List<ServerStoreCatalog.StoreCategory.StoreItem>
                        {
                            new ServerStoreCatalog.StoreCategory.StoreItem
                            {
                                Unknown3 = 1550,
                                ItemName = "Costume Slot Unlock",
                                ItemDescription = "Unlocks an additional costume slot.",
                                Unknown4 = new byte[]{
                                    0, 0, 22, 67, 0, 0, 150, 66
                                },
                                Unknown5 = 0,
                                Unknown6 = -1016071787,
                                Unknown7 = 0,
                                Unknown8 = new List<ServerStoreCatalog.StoreCategory.StoreItem.UnknownStructure0>
                                {
                                    new ServerStoreCatalog.StoreCategory.StoreItem.UnknownStructure0
                                    {
                                        Unknown10 = 6,
                                        Unknown11 = 1117126656,
                                        Unknown12 = 0,
                                        Unknown13 = 0,
                                        Unknown14 = 433572722,
                                        Unknown15 = 1995404795
                                    },
                                    new ServerStoreCatalog.StoreCategory.StoreItem.UnknownStructure0
                                    {
                                        Unknown10 = 11,
                                        Unknown11 = 1125515264,
                                        Unknown12 = 0,
                                        Unknown13 = 0,
                                        Unknown14 = -235145984,
                                        Unknown15 = 1995405795
                                    }
                                },
                                Unknown9 = new List<ServerStoreCatalog.StoreCategory.StoreItem.AccountItemData>
                                {
                                    new ServerStoreCatalog.StoreCategory.StoreItem.AccountItemData
                                    {
                                        Type = 0,
                                        AccountItemId = 78,
                                        Unknown16 = 1
                                    }
                                }
                            }
                        },
                        ArraySize = 2,
                        Unknown16 = new byte[]
                        {
                            35, 0, 0, 0, 36, 0, 0, 0
                        },
                        Unknown17 = new byte[]
                        {
                            22, 0, 0, 0, 6, 0, 0, 0
                        }
                    }
                }
            });

            // 0x0987 - Store catalogue finalised message
            session.EnqueueMessageEncrypted(new Server0987());
        }
    }
}
