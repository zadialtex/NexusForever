using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using RankModel = NexusForever.WorldServer.Database.Character.Model.GuildRank;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Rank
    {
        public ulong GuildId { get; }
        public byte Index { get; private set; }
        public string Name { get; private set; }
        public GuildRankPermission GuildPermission { get; private set; }
        public ulong BankWithdrawalPermissions { get; set; }
        public long MoneyWithdrawalLimit { get; private set; }
        public long RepairLimit { get; private set; }

        private RankSaveMask saveMask;

        public Rank(string name, ulong guildId, byte index, GuildRankPermission guildRankPermission, ulong bankWithdrawalPermissions, long moneyWithdrawalLimit, long repairLimit)
        {
            GuildId = guildId;
            Name = name;
            Index = index;
            GuildPermission = guildRankPermission;
            BankWithdrawalPermissions = bankWithdrawalPermissions;
            MoneyWithdrawalLimit = moneyWithdrawalLimit;
            RepairLimit = repairLimit;

            saveMask = RankSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != RankSaveMask.None)
            {
                if ((saveMask & RankSaveMask.Create) != 0)
                {
                    context.Add(new RankModel
                    {
                        Id = GuildId,
                        Index = Index,
                        Name = Name,
                        Permission = (int)GuildPermission,
                        BankWithdrawalPermission = BankWithdrawalPermissions,
                        MoneyWithdrawalLimit = MoneyWithdrawalLimit,
                        RepairLimit = RepairLimit
                    });
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new RankModel
                    {
                        Id = GuildId,
                        Index = Index
                    };

                    // could probably clean this up with reflection, works for the time being
                    //EntityEntry <GuildModel> entity = context.Attach(model);
                    //if ((saveMask & GuildSaveMask.Name) != 0)
                    //{
                    //    model.Name = Name;
                    //    entity.Property(p => p.Name).IsModified = true;
                    //}
                }

                saveMask = RankSaveMask.None;
            }
        }

        public void ChangeName(string name)
        {
            Name = name;
        }

        public void AddPermission(GuildRankPermission guildRankPermission)
        {
            GuildPermission |= guildRankPermission;
        }

        public void RemovePermission(GuildRankPermission guildRankPermission)
        {
            if((GuildPermission & guildRankPermission) == 0)
            {
                GuildPermission |= guildRankPermission;
            }
        }

        public GuildData.Rank GetGuildDataRank()
        {
            return new GuildData.Rank
            {
                RankName = Name,
                PermissionMask = GuildPermission,
                BankWithdrawalPermissions = BankWithdrawalPermissions,
                MoneyWithdrawalLimit = MoneyWithdrawalLimit,
                RepairLimit = RepairLimit
            };
        }

    }
}
