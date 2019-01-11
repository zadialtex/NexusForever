using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
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
                        },
                        new Server096D.AccountItem
                        {
                            Unknown0 = 3849507,
                            Unknown1 = 29,
                            Unknown2 = 0,
                            Unknown3 = false,
                            RealmId = 0,
                            CharacterId = 0
                        }
                    }

            });

            // 0x0968 - Entitlements?
            session.EnqueueMessageEncrypted(new ServerAccountEntitlements
            {
                Entitlements =
                {
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.BaseCharacterProgressionCaps,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.BaseCharacterSlots,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.EconomyParticipation,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.GuildsAccess,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.FullSocialParticipation,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.CREDDUsage,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.InGameCSAccess,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = (Entitlement)20,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.Signature,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.ChuaWarriorUnlock,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.AurinEngineerUnlock,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.CanPurchasePromotionToken,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.SharedRealmBankUnlock,
                        Count       = 1
                    },
                    new ServerAccountEntitlements.AccountEntitlementInfo
                    {
                        Entitlement = Entitlement.AdditionalCostumeUnlocks,
                        Count = 1
                    }
                }
            });

            // 0x097F
            session.EnqueueMessageEncrypted(new Server097F
            {
                Unknown0 = 1
            });

            // 0x0966 - SetAccountCurrencyAmounts
            session.EnqueueMessageEncrypted(new ServerAccountSetCurrency
            {
                AccountCurrencies = new List<ServerAccountSetCurrency.AccountCurrency>
                {
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 6,
                        Amount = 13603   
                    },
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 7,
                        Amount = 0
                    },
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 5,
                        Amount = 2
                    },
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 9,
                        Amount = 441
                    },
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 4,
                        Amount = 14
                    },
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 11,
                        Amount = 1337
                    }, 
                    new ServerAccountSetCurrency.AccountCurrency
                    {
                        CurrencyId = 8,
                        Amount = 0
                    }
                }
            });

            // 0x096F
            session.EnqueueMessageEncrypted(new Server096F
            {
                Unknown0 = 1800
            });

            // 0x096E
            session.EnqueueMessageEncrypted(new ServerAccountDailyRewards
            {
                DaysAvailable = 55,
                DaysClaimed = 53,
            });
                // 0x078F - Claim Reward Button

            // 0x0981 - Looks to be Entries from AccountItem Table
            session.EnqueueMessageEncrypted(new Server0981
            {
                AccountItemList = new byte[]
                {
                    4, 0, 0, 0, 49, 0, 0, 0, 76, 0, 0, 0, 78, 0, 0, 0, 86, 0, 0, 0, 89, 0, 0, 0, 90, 0, 0, 0, 92, 0, 0, 0, 103, 0, 0, 0, 109, 0, 0, 0, 110, 0, 0, 0, 111, 0, 0, 0, 122, 0, 0, 0, 139, 0, 0, 0, 142, 0, 0, 0, 148, 0, 0, 0, 149, 0, 0, 0, 157, 0, 0, 0, 160, 0, 0, 0, 161, 0, 0, 0, 162, 0, 0, 0, 166, 0, 0, 0, 172, 0, 0, 0, 176, 0, 0, 0, 187, 0, 0, 0, 192, 0, 0, 0, 207, 0, 0, 0
                }
            });

            // 0x0988 - Store catalogue categories
            session.EnqueueMessageEncrypted(new ServerStoreCategories
            {
                StoreCategories = new List<ServerStoreCategories.StoreCategory>
                {
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Holo-Wardrobe",
                        CategoryDesc = "Costumes, weapons, and dyes keep you safe and looking sharp.",
                        CategoryId = 27,
                        ParentCategoryId = 26,
                        Index = 7,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Costumes",
                        CategoryDesc = "Look like a quadrillion space-bucks when you update your style with new costume pieces.",
                        CategoryId = 28,
                        ParentCategoryId = 27,
                        Index = 1,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Weapons",
                        CategoryDesc = "Need a new weapon? Find deadly tools of combat to suit every class here.",
                        CategoryId = 29,
                        ParentCategoryId = 27,
                        Index = 3,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Mounts",
                        CategoryDesc = "Because walking everywhere is for suckers.",
                        CategoryId = 31,
                        ParentCategoryId = 26,
                        Index = 9,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Hoverboards",
                        CategoryDesc = "Go anywhere and shred everywhere with gravity-defying hoverboards.",
                        CategoryId = 32,
                        ParentCategoryId = 31,
                        Index = 2,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Ground Mounts",
                        CategoryDesc = "From sleek beasts to overpowered hoverbikes, you'll cover more ground than ever with these mounts.",
                        CategoryId = 33,
                        ParentCategoryId = 31,
                        Index = 1,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Consumables",
                        CategoryDesc = "Eat, drink, and be merry with these premium consumables.",
                        CategoryId = 34,
                        ParentCategoryId = 26,
                        Index = 12,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory {
                        CategoryName = "Unlocks",
                        CategoryDesc = "Expand your horizons (and your options) with character and account unlocks.",
                        CategoryId = 35,
                        ParentCategoryId = 26,
                        Index = 13,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Account Unlocks",
                        CategoryDesc = "Get new character slots, increase your décor limit, access additional costumes, and more.",
                        CategoryId = 36,
                        ParentCategoryId = 35,
                        Index = 1,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Character Unlocks",
                        CategoryDesc = "Enhance a character's personal life with options like extra bank slots and improved riding skills.",
                        CategoryId = 37,
                        ParentCategoryId = 35,
                        Index = 2,
                        Visible = true
                    },
                    new ServerStoreCategories.StoreCategory
                    {
                        CategoryName = "Beginner Basics",
                        CategoryDesc = "New to WildStar? Check out our shiny new deals for bright beginnings!",
                        CategoryId = 212,
                        ParentCategoryId = 26,
                        Index = 6,
                        Visible = true
                    },
                },
                Unknown4 = 4
            });

            // 0x098B - Store catalogue subcategories + items
            session.EnqueueMessageEncrypted(new ServerStoreCatalog
            {
                OfferGroups = new List<ServerStoreCatalog.OfferGroup>
                    {
                        new ServerStoreCatalog.OfferGroup
                        {
                            Unknown0 = 1550,
                            OfferGroupName = "Costume Slot Unlock",
                            OfferGroupDescription = "Unlocks an additional costume slot. ",
                            Offers = new List<ServerStoreCatalog.OfferGroup.Offer>
                            {
                                new ServerStoreCatalog.OfferGroup.Offer
                                {
                                    Unknown3 = 1550,
                                    OfferName = "Costume Slot Unlock",
                                    OfferDescription = "Unlocks an additional costume slot. ",
                                    PriceArray = new byte[]{
                                        0, 0, 22, 67, 0, 0, 150, 66
                                    },
                                    Unknown6 = -1016071787,
                                    Unknown8 = new List<ServerStoreCatalog.OfferGroup.Offer.UnknownStructure0>
                                    {
                                        new ServerStoreCatalog.OfferGroup.Offer.UnknownStructure0
                                        {
                                            CurrencyId = 6,
                                            Unknown15 = 2995404795
                                        },
                                        new ServerStoreCatalog.OfferGroup.Offer.UnknownStructure0
                                        {
                                            CurrencyId = 11,
                                            Unknown15 = 2995405795
                                        }
                                    },
                                    Unknown9 = new List<ServerStoreCatalog.OfferGroup.Offer.AccountItemData>
                                    {
                                        new ServerStoreCatalog.OfferGroup.Offer.AccountItemData
                                        {
                                            Type = 0,
                                            AccountItemId = 78,
                                            Unknown16 = 2
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

            // 0x98C
            session.EnqueueMessageEncrypted(new Server098C
            {
                Unknown0 = true,
                Unknown1 = 22
            });
        }
    }
}
