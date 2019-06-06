using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Account.Static
{
    public enum TransactionEvent
    {
        PurchaseRequest,
        PurchaseFail,
        PurchaseSucceeded,
        ClaimRequested,
        ClaimPending,
        ClaimDelivered,
        GiftRequest,
        GiftSent,
        GiftReceived
    }
}
