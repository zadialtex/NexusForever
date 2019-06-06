using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Storefront;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class AccountHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientStorefrontRequestCatalog)]
        public static void HandleStorefrontRequestCatalogRealm(WorldSession session, ClientStorefrontRequestCatalog storefrontRequest)
        {
            // Packet order below, for reference and implementation

            // 0x096D - Account inventory
            session.EnqueueMessageEncrypted(new ServerAccountItems
            {
                AccountItems = new System.Collections.Generic.List<Model.Shared.AccountItem>
                {
                    new Model.Shared.AccountItem
                    {
                        Id = 1,
                        ItemId = 793,
                        //Unknown0 = 0,
                        //Unknown1 = false,
                        //TargetPlayerIdentity = new Model.Shared.TargetPlayerIdentity
                        //{
                        //    RealmId = 1,
                        //    CharacterId = 2
                        //}
                    },
                    new Model.Shared.AccountItem
                    {
                        Id = 12757397,
                        ItemId = 29,
                        //Unknown0 = 3,
                        //Unknown1 = true
                    },
                    //new Model.Shared.AccountItem
                    //{
                    //    Id = 12757398,
                    //    ItemId = 29,
                    //    Unknown0 = 1234,
                    //},
                    new Model.Shared.AccountItem
                    {
                        Id = 234459813,
                        ItemId = 19
                    },
                    new Model.Shared.AccountItem
                    {
                        Id = 59824862,
                        ItemId = 2212,
                        Unknown0 = 7,
                        Unknown1 = true,
                        TargetPlayerIdentity = new Model.Shared.TargetPlayerIdentity
                        {
                            RealmId = 1,
                            CharacterId = 1
                        }
                    },
                    new Model.Shared.AccountItem
                    {
                        Id = 59824863,
                        ItemId = 2132,
                        Unknown0 = 0,
                        Unknown1 = false,
                        TargetPlayerIdentity = new Model.Shared.TargetPlayerIdentity
                        {
                            RealmId = 1,
                            CharacterId = 1
                        }
                    }
                }
            });

            session.EnqueueMessageEncrypted(new ServerAccountItemCooldownSet
            {
                AccountItemCooldownGroup = 3,
                CooldownInSeconds = 18000
            });
            // 0x0968 - Entitlements

            // 0x097F - Account Tier (Basic/Signature)

            // 0x0966 - SetAccountCurrencyAmounts
            session.AccountCurrencyManager.SendCharacterListPacket();

            // 0x096F - Weekly Omnibit progress

            // 0x096E - Daily Rewards packet
            // 0x078F - Claim Reward Button

            // 0x0981 - Unknown

            // Store packets
            // 0x0988 - Store catalogue categories 
            // 0x098B - Store catalogue offer grouips + offers
            // 0x0987 - Store catalogue finalised message
            GlobalStorefrontManager.HandleCatalogRequest(session);
        }

        [MessageHandler(GameMessageOpcode.ClientStorefrontPurchaseCharacter)]
        public static void HandlePurchaseCharacter(WorldSession session, ClientStorefrontPurchaseCharacter purchase)
        {
            log.Info($"OfferId: {purchase.StorePurchaseRequest.OfferId}, Unknown1: {purchase.StorePurchaseRequest.CurrencyId}, Unknown2: {purchase.StorePurchaseRequest.Price}, Unknown3: {purchase.StorePurchaseRequest.Unknown3}, Unknown4: {purchase.StorePurchaseRequest.CategoryId}, TargetPlayer: {purchase.StorePurchaseRequest.PlayerIdentity.RealmId} : {purchase.StorePurchaseRequest.PlayerIdentity.CharacterId}, Unknown5: {purchase.StorePurchaseRequest.Unknown5}");

            session.AccountStorefrontManager.HandleCharacterPurchase(purchase.StorePurchaseRequest.OfferId, purchase.StorePurchaseRequest.CurrencyId, (uint)purchase.StorePurchaseRequest.Price, purchase.StorePurchaseRequest.PlayerIdentity, session.Player);
        }

        [MessageHandler(GameMessageOpcode.ClientStorefrontPurchaseAccount)]
        public static void HandlePurchaseAccount(WorldSession session, ClientStorefrontPurchaseAccount purchase)
        {
            log.Info($"OfferId: {purchase.StorePurchaseRequest.OfferId}, Unknown1: {purchase.StorePurchaseRequest.CurrencyId}, Unknown2: {purchase.StorePurchaseRequest.Price}, Unknown3: {purchase.StorePurchaseRequest.Unknown3}, Unknown4: {purchase.StorePurchaseRequest.CategoryId}, TargetPlayer: {purchase.StorePurchaseRequest.PlayerIdentity.RealmId} : {purchase.StorePurchaseRequest.PlayerIdentity.CharacterId}, Unknown5: {purchase.StorePurchaseRequest.Unknown5}, base.Unknown0: {purchase.Unknown0}, TargetPlayer: {purchase.TargetPlayerIdentity.RealmId} : {purchase.TargetPlayerIdentity.CharacterId}, Message: {purchase.Message}");

            session.EnqueueMessageEncrypted(new ServerStoreError
            {
                Error = Game.Storefront.Static.StoreError.MissingEntitlement
            });
        }
    }
}