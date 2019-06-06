using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Mail.Static;
using NexusForever.WorldServer.Game.Storefront;
using NexusForever.WorldServer.Game.Storefront.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using AccountModel = NexusForever.Shared.Database.Auth.Model.Account;

namespace NexusForever.WorldServer.Game.Account
{
    public class AccountStorefrontManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly WorldSession session;
        private readonly Dictionary<uint, string> Transactions = new Dictionary<uint, string>();

        public AccountStorefrontManager(WorldSession session, AccountModel model)
        {
            this.session = session;
        }

        public void HandleCharacterPurchase(uint offerId, byte currencyId, uint expectedPrice, TargetPlayerIdentity playerIdentity, Player player)
        {
            OfferItem offerItem = GlobalStorefrontManager.GetStoreOfferItem(offerId);
            OfferItemPrice offerItemPrice;
            List<AccountItemEntry> accountItems = new List<AccountItemEntry>();

            StoreError GetResult()
            {
                if (player == null || playerIdentity.CharacterId != player.CharacterId)
                    return StoreError.GenericFail;

                if (offerItem == null)
                    return StoreError.InvalidOffer;

                offerItemPrice = offerItem.GetPriceDataForCurrency(currencyId);
                if (offerItemPrice == null)
                    return StoreError.InvalidPrice;

                foreach(OfferItemData itemData in offerItem.GetOfferItems())
                {
                    AccountItemEntry accountItem = GameTableManager.AccountItem.GetEntry(itemData.ItemId);
                    if (accountItem == null)
                        return StoreError.GenericFail;

                    accountItems.Add(accountItem);
                }

                if (expectedPrice != offerItemPrice.GetCurrencyValue())
                    return StoreError.PgWs_CartFraudFailure;

                if (!session.AccountCurrencyManager.CanAfford(currencyId, (ulong)offerItemPrice.Price))
                    return StoreError.InvalidPrice;
                
                return StoreError.PurchasePending;
            }

            StoreError storeError = GetResult();
            if (storeError == StoreError.PurchasePending)
            {
                session.EnqueueMessageEncrypted(new ServerStorePurchaseResult
                {
                    Success = true,
                    Unknown0 = 22
                });

                session.AccountCurrencyManager.CurrencySubtractAmount(currencyId, expectedPrice);

                //session.EnqueueMessageEncrypted(new ServerAccountTransaction
                //{
                //    TransacationId = Guid.NewGuid().ToString(),
                //    Redeemed = true
                //});

                //session.EnqueueMessageEncrypted(new ServerAccountOperationResult
                //{
                //    Result = Game.Account.Static.AccountOperationResult.NotEnoughCurrency,
                //    Operation = Game.Account.Static.AccountOperation.MTXPurchase
                //});

                //session.EnqueueMessageEncrypted(new ServerAccountOperationResult
                //{
                //    Result = Game.Account.Static.AccountOperationResult.Ok,
                //    Operation = Game.Account.Static.AccountOperation.TakeItem
                //});
                for (int i = 0; i < accountItems.Count; i++)
                {
                    if (session.Player.Inventory.IsInventoryFull())
                    {
                        uint[] itemToSend = new uint[] { accountItems[i].Item2Id };
                        session.Player.MailManager.SendMail(26454, DeliveryTime.Instant, 461265, 461266, itemToSend);
                    }
                    else
                        session.Player.Inventory.ItemCreate(accountItems[i].Item2Id, 1, 44, 1);
                }
            }
            else
                session.EnqueueMessageEncrypted(new ServerStoreError
                {
                    Error = storeError
                });
        }
    }
}
