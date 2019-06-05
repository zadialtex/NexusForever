using NexusForever.WorldServer.Network;
using System.Collections.Generic;
using AccountModel = NexusForever.Shared.Database.Auth.Model.Account;
using AccountCurrencyModel = NexusForever.Shared.Database.Auth.Model.AccountCurrency;
using NexusForever.Shared.Database.Auth.Model;
using NexusForever.WorldServer.Network.Message.Model;
using ServerAccountCurrency = NexusForever.WorldServer.Network.Message.Model.Shared.AccountCurrency;
using System.Linq;
using NLog;
using System;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Account.Static;

namespace NexusForever.WorldServer.Game.Account
{
    public class AccountCurrencyManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly WorldSession session;
        private readonly Dictionary<uint, AccountCurrency> currencies = new Dictionary<uint, AccountCurrency>();

        public AccountCurrencyManager(WorldSession session, AccountModel model)
        {
            this.session = session;

            foreach (AccountCurrencyModel currencyModel in model.AccountCurrency)
                currencies.Add(currencyModel.CurrencyId, new AccountCurrency(currencyModel));
        }

        public void Save(AuthContext context)
        {
            foreach (AccountCurrency accountCurrency in currencies.Values)
                accountCurrency.Save(context);
        }

        /// <summary>
        /// Create a new <see cref="CharacterCurrency"/>.
        /// </summary>
        private AccountCurrency CreateAccountCurrency(byte currencyId, ulong amount = 0)
        {
            AccountCurrencyTypeEntry currencyEntry = GameTableManager.AccountCurrencyType.GetEntry(currencyId);
            if (currencyEntry == null)
                throw new ArgumentNullException($"AccountCurrencyTypeEntry not found for currencyId {currencyId}");

            if (currencies.TryAdd(currencyId, new AccountCurrency(session.Account.Id, (AccountCurrencyType)currencyId, amount)))
                return currencies[currencyId];
            else
                return null;
        }

        /// <summary>
        /// Returns whether the Account has enough of the currency to afford the amount
        /// </summary>
        public bool CanAfford(AccountCurrencyType currencyType, ulong amount)
        {
            return CanAfford((byte)currencyType, amount);
        }

        /// <summary>
        /// Returns whether the Account has enough of the currency to afford the amount
        /// </summary>
        public bool CanAfford(byte currencyId, ulong amount)
        {
            if (!currencies.TryGetValue(currencyId, out AccountCurrency accountCurrency))
                return false;

            return accountCurrency.CanAfford(amount);
        }

        /// <summary>
        /// Add a supplied amount to an <see cref="AccountCurrency"/>.
        /// </summary>
        public void CurrencyAddAmount(AccountCurrencyType currencyType, ulong amount, ulong reason = 0)
        {
            CurrencyAddAmount((byte)currencyType, amount);
        }

        /// <summary>
        /// Add a supplied amount to an <see cref="AccountCurrency"/>.
        /// </summary>
        public void CurrencyAddAmount(byte currencyId, ulong amount, ulong reason = 0)
        {
            if (!currencies.TryGetValue(currencyId, out AccountCurrency accountCurrency))
            {
                accountCurrency = CreateAccountCurrency(currencyId, 0);
            }

            if (accountCurrency == null)
                throw new ArgumentException($"Account Currency entry not found for currencyId {(AccountCurrencyType)currencyId}.");

            if (accountCurrency.AddAmount(amount))
                SendAccountCurrencyUpdate(accountCurrency, reason);
        }

        /// <summary>
        /// Subtract a supplied amount to an <see cref="AccountCurrency"/>.
        /// </summary>
        public void CurrencySubtractAmount(AccountCurrencyType currencyType, ulong amount, ulong reason = 0)
        {
            CurrencySubtractAmount((byte)currencyType, amount);
        }

        /// <summary>
        /// Subtract a supplied amount to an <see cref="AccountCurrency"/>.
        /// </summary>
        public void CurrencySubtractAmount(byte currencyId, ulong amount, ulong reason = 0)
        {
            if (!currencies.TryGetValue(currencyId, out AccountCurrency accountCurrency))
            {
                accountCurrency = CreateAccountCurrency(currencyId, 0);
            }

            if (accountCurrency == null)
                throw new ArgumentException($"Account Currency entry not found for currencyId {(AccountCurrencyType)currencyId}.");

            if (!accountCurrency.CanAfford(amount))
                throw new ArgumentException($"Trying to remove more currency {accountCurrency.CurrencyId} than the player has!");

            // TODO: Ensure that we're not at cap - is there a cap?
            if(accountCurrency.SubtractAmount(amount))
                SendAccountCurrencyUpdate(accountCurrency, reason);
        }

        private IEnumerable<ServerAccountCurrency> GetAccountCurrencyPackets()
        {
            foreach (AccountCurrency accountCurrency in currencies.Values)
                yield return accountCurrency.BuildServerPacket();
        }

        /// <summary>
        /// Sends information about all the player's <see cref="AccountCurrency"/> during Character Select
        /// </summary>
        public void SendCharacterListPacket()
        {
            session.EnqueueMessageEncrypted(new ServerAccountCurrencySet
            {
                AccountCurrencies = GetAccountCurrencyPackets().ToList()
            });
        }

        /// <summary>
        /// Sends information about all the player's <see cref="AccountCurrency"/> when entering world
        /// </summary>
        public void SendInitialPackets()
        {
            foreach (AccountCurrency accountCurrency in currencies.Values)
                SendAccountCurrencyUpdate(accountCurrency);
        }

        /// <summary>
        /// Sends information about a player's <see cref="AccountCurrency"/>
        /// </summary>
        private void SendAccountCurrencyUpdate(AccountCurrency accountCurrency, ulong reason = 0)
        {
            session.EnqueueMessageEncrypted(new ServerAccountCurrencyGrant
            {
                AccountCurrency = accountCurrency.BuildServerPacket(),
                Unknown0 = reason
            });
        }
    }
}
