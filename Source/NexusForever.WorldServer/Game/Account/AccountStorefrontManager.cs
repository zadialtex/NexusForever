using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Account.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Mail.Static;
using NexusForever.WorldServer.Game.Storefront;
using NexusForever.WorldServer.Game.Storefront.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccountModel = NexusForever.Shared.Database.Auth.Model.Account;

namespace NexusForever.WorldServer.Game.Account
{
    public class AccountStorefrontManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly WorldSession session;
        private readonly Dictionary<string, Transaction> Transactions = new Dictionary<string, Transaction>();
        private readonly List<Transaction> pendingTransactions = new List<Transaction>();

        public AccountStorefrontManager(WorldSession session, AccountModel model)
        {
            this.session = session;
        }

        public void HandleCharacterPurchase(uint offerId, byte currencyId, uint expectedPrice, TargetPlayerIdentity playerIdentity, Player player)
        {
            OfferItem offerItem = GlobalStorefrontManager.GetStoreOfferItem(offerId);
            OfferItemPrice offerItemPrice;
            List<OfferItemData> accountItems = new List<OfferItemData>();

            StoreError GetResult()
            {
                if (player == null || playerIdentity.CharacterId != player.CharacterId)
                    return StoreError.GenericFail;

                if (offerItem == null)
                    return StoreError.InvalidOffer;

                offerItemPrice = offerItem.GetPriceDataForCurrency(currencyId);
                if (offerItemPrice == null)
                    return StoreError.InvalidPrice;

                accountItems = offerItem.GetOfferItems().ToList();
                foreach (OfferItemData itemData in offerItem.GetOfferItems())
                {
                    if (itemData.Entry == null)
                        return StoreError.GenericFail;

                    if (itemData.Entry.Item2Id == 0 && itemData.Entry.AccountCurrencyEnum == 0)
                        return StoreError.InvalidOffer;

                    //if (itemData.Entry.EntitlementIdPurchase > 0 && !session.AccountEntitlements.HasEntitlement(itemData.Entry.EntitlementIdPurchase))
                    //    return StoreError.MissingEntitlement;
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
                    if (accountItems[i].Entry.Item2Id > 0)
                        HandleItemPurchase(accountItems[i]);

                    if (accountItems[i].Entry.AccountCurrencyAmount > 0)
                        HandleCurrencyPurchase(accountItems[i]);
                }
            }
            else
                session.EnqueueMessageEncrypted(new ServerStoreError
                {
                    Error = storeError
                });
        }

        public void GenerateTransaction(ulong transactionKey, TransactionEvent transactionEvent, AccountCurrencyType accountCurrencyType = AccountCurrencyType.None, ulong startingValue = 0, ulong finishedCurrency = 0)
        {
            Transaction newTransaction = new Transaction(transactionKey, transactionEvent, session.Player != null ? session.Player.CharacterId : 0ul, accountCurrencyType, startingValue, finishedCurrency);

            pendingTransactions.Add(newTransaction);
        }

        private void HandleItemPurchase(OfferItemData offerItemData)
        {
            if (session.Player.Inventory.IsInventoryFull())
            {
                uint[] itemToSend = new uint[] { offerItemData.Entry.Item2Id };
                session.Player.MailManager.SendMail(26454, DeliveryTime.Instant, 461265, 461266, itemToSend);
            }
            else
                session.Player.Inventory.ItemCreate(offerItemData.Entry.Item2Id, offerItemData.Amount, 44, 1);
        }

        private void HandleCurrencyPurchase(OfferItemData offerItemData)
        {
            session.AccountCurrencyManager.CurrencyAddAmount((AccountCurrencyType)offerItemData.Entry.AccountCurrencyEnum, offerItemData.Entry.AccountCurrencyAmount * offerItemData.Amount, 1);
        }
    }
}
