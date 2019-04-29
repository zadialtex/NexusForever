using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using GuildRankModel = NexusForever.WorldServer.Database.Character.Model.GuildRank;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Rank
    {
        public ulong GuildId { get; }
        public byte Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;

                }       
            }
        }
        private byte index;
        public string Name { get; private set; }
        public GuildRankPermission GuildPermission { get; private set; }
        public ulong BankWithdrawalPermissions { get; set; }
        public long MoneyWithdrawalLimit { get; private set; }
        public long RepairLimit { get; private set; }

        public bool PendingDelete => saveMask == RankSaveMask.Delete;

        private RankSaveMask saveMask;

        public Rank(GuildRankModel model)
        {
            GuildId = model.Id;
            Name = model.Name;
            Index = model.Index;
            GuildPermission = (GuildRankPermission)model.Permission;
            BankWithdrawalPermissions = model.BankWithdrawalPermission;
            MoneyWithdrawalLimit = model.MoneyWithdrawalLimit;
            RepairLimit = model.RepairLimit;

            saveMask = RankSaveMask.None;
        }

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
                    context.Add(new GuildRankModel
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
                else if ((saveMask & RankSaveMask.Delete) != 0)
                {
                    var model = new GuildRankModel
                    {
                        Id = GuildId,
                        Index = Index
                    };

                    context.Entry(model).State = EntityState.Deleted;
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new GuildRankModel
                    {
                        Id = GuildId,
                        Index = Index
                    };

                    // could probably clean this up with reflection, works for the time being
                    EntityEntry<GuildRankModel> entity = context.Attach(model);
                    if ((saveMask & RankSaveMask.Rename) != 0)
                    {
                        model.Name = Name;
                        entity.Property(p => p.Name).IsModified = true;
                    }
                    if ((saveMask & RankSaveMask.Permissions) != 0)
                    {
                        model.Permission = (int)GuildPermission;
                        entity.Property(p => p.Permission).IsModified = true;
                    }
                }

                saveMask = RankSaveMask.None;
            }
        }

        public void Rename(string name)
        {
            Name = name;
            saveMask |= RankSaveMask.Rename;
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

        public void SetPermission(GuildRankPermission guildRankPermission)
        {
            GuildPermission = guildRankPermission;

            saveMask |= RankSaveMask.Permissions;
        }

        public void Delete()
        {
            // Entity won't exist if create flag exists, so we set to None and let GC get rid of it.
            if ((saveMask & RankSaveMask.Create) == 0)
                saveMask = RankSaveMask.Delete;
            else
                saveMask = RankSaveMask.None;
        }

        public GuildRank BuildGuildRankPacket()
        {
            return new GuildRank
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
