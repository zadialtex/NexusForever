using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Storefront.Static
{
    public enum StoreError
    {
        CatalogUnavailable           = 0x0000,
        StoreDisabled                = 0x0001,
        InvalidOffer                 = 0x0002,
        InvalidPrice                 = 0x0003,
        GenericFail                  = 0x0004,
        PurchasePending              = 0x0005,
        PgWs_CartFraudFailure        = 0x0006,
        PgWs_CartPaymentFailure      = 0x0007,
        PgWs_InvalidCCExpirationDate = 0x0008,
        PgWs_InvalidCreditCardNumber = 0x0009,
        PgWs_CreditCardExpired       = 0x000A,
        PgWs_CreditCardDeclined      = 0x000B,
        PgWs_CreditFloorExceeded     = 0x000C,
        PgWs_InventoryStatusFailure  = 0x000D,
        PgWs_PaymentPostAuthFailure  = 0x000E,
        PgWs_SubmitCartFailed        = 0x000F,
        PurchaseVelocityLimit        = 0x0010,
        MissingItemEntitlement       = 0x0011,
        IneligibleGiftRecipient      = 0x0012,
        CannotUseOffer               = 0x0013,
        MissingEntitlement           = 0x0014,
        CannotGiftOffer              = 0x0015
    }
}
