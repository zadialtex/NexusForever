using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerAccountSetCurrency, MessageDirection.Server)]
    public class ServerAccountSetCurrency : IWritable
    {
        public class AccountCurrency : IWritable
        {
            public byte CurrencyId { get; set; }
            public ulong Amount { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(CurrencyId, 5);
                writer.Write(Amount);
            }
        }
        
        public List<AccountCurrency> AccountCurrencies { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountCurrencies.Count, 32);
            AccountCurrencies.ForEach(e => e.Write(writer));
        }
    }
}
