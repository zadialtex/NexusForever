using NexusForever.WorldServer.Game.Account.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Account
{
    class Transaction
    {
        public Guid TransactionId { get; private set; }
        public TransactionEvent TransactionEvent { get; set; }
        public ulong TransactionKey { get; set; }
        public DateTime Date { get; set; }
        public ulong CharacterId { get; set; }
        public AccountCurrencyType AccountCurrency { get; set; }
        public ulong StartingCurrency { get; set; }
        public ulong FinishedCurrency { get; set; }

        public Transaction(ulong transactionKey, TransactionEvent transactionEvent, ulong characterId, AccountCurrencyType accountCurrency, ulong startingCurrency, ulong finishedCurrency)
        {
            TransactionId = Guid.NewGuid();
            TransactionKey = transactionKey;
            TransactionEvent = transactionEvent;
            Date = DateTime.UtcNow;
            CharacterId = characterId;
            AccountCurrency = accountCurrency;
            StartingCurrency = startingCurrency;
            FinishedCurrency = finishedCurrency;
        }
    }
}
