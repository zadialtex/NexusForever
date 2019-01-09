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
                            78, 1, 0, 0
                        }
                    }
            });

            // 0x098B - Store catalogue items
            session.EnqueueMessageEncrypted(new Server098B
            {
                StoreCategories = new List<Server098B.StoreCategory>
                    {
                        new Server098B.StoreCategory
                        {
                            Unknown0 = 1550,
                            SubCategoryName = "Costume Slot Unlock",
                            SubCategoryDescription = "Unlocks an additional costume slot.",
                            AccountItemList = new List<Server098B.StoreCategory.StoreItem>
                        {
                            new Server098B.StoreCategory.StoreItem
                            {
                                Unknown3 = 1550,
                                ItemName = "Costume Slot Unlock",
                                ItemDescription = "Unlocks an additional costume slot.",
                                Unknown8 = new List<Server098B.StoreCategory.StoreItem.UnknownStructure0>
                                {
                                    new Server098B.StoreCategory.StoreItem.UnknownStructure0
                                    {
                                     Unknown10 = 6
                                    }
                                },
                                Unknown9 = new List<Server098B.StoreCategory.StoreItem.AccountItemData>
                                {
                                    new Server098B.StoreCategory.StoreItem.AccountItemData
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

            //// 0x0987 - Store catalogue finalised message
            /ession.EnqueueMessageEncrypted(new Server0987());
        }
    }
}
