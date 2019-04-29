using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Guild.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GuildRank : IWritable
    {
        public string RankName { get; set; } = "";
        public GuildRankPermission PermissionMask { get; set; } = GuildRankPermission.NoRank;
        public ulong BankWithdrawalPermissions { get; set; } = 0;
        public long MoneyWithdrawalLimit { get; set; } = 0;
        public long RepairLimit { get; set; } = 0;

        public void Write(GamePacketWriter writer)
        {
            writer.WriteStringWide(RankName);
            writer.Write((int)PermissionMask);
            writer.Write(BankWithdrawalPermissions);
            writer.Write(MoneyWithdrawalLimit);
            writer.Write(RepairLimit);
        }
    }
}
