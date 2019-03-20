using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.Character.Model
{
    public partial class GuildRank
    {
        public ulong Id { get; set; }
        public byte Index { get; set; }
        public string Name { get; set; }
        public int Permission { get; set; }
        public ulong BankWithdrawalPermission { get; set; }
        public long MoneyWithdrawalLimit { get; set; }
        public long RepairLimit { get; set; }

        public virtual Guild IdNavigation { get; set; }
    }
}