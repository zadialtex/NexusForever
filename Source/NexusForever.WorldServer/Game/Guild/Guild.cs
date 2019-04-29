using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using GuildBaseModel = NexusForever.WorldServer.Database.Character.Model.Guild;
using GuildDataModel = NexusForever.WorldServer.Database.Character.Model.GuildData;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Guild : GuildBase
    {
        public uint Taxes { get; private set; }
        public GuildStandard GuildStandard { get; private set; }

        public string MessageOfTheDay
        {
            get => messageOfTheDay;
            set
            {
                messageOfTheDay = value;
                saveMask |= GuildSaveMask.MessageOfTheDay;
            }
        }
        private string messageOfTheDay;
        public string AdditionalInfo
        {
            get => additionalInfo;
            set
            {
                additionalInfo = value;
                saveMask |= GuildSaveMask.AdditionalInfo;
            }
        }
        private string additionalInfo;

        private GuildSaveMask saveMask;

        public Guild(GuildDataModel model, GuildBaseModel baseModel) 
            : base (GuildType.Guild, baseModel)
        {
            Taxes = model.Taxes;
            GuildStandard = new GuildStandard
            {
                BackgroundIcon = new GuildStandard.GuildStandardPart
                {
                    GuildStandardPartId = model.BackgroundIconPartId
                },
                ForegroundIcon = new GuildStandard.GuildStandardPart
                {
                    GuildStandardPartId = model.ForegroundIconPartId
                },
                ScanLines = new GuildStandard.GuildStandardPart
                {
                    GuildStandardPartId = model.ScanLinesPartId
                }
            };
            MessageOfTheDay = model.MessageOfTheDay;
            AdditionalInfo = model.AdditionalInfo;

            saveMask = GuildSaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="Guild"/>
        /// </summary>
        public Guild(WorldSession leaderSession, string guildName, string leaderRankName, string councilRankName, string memberRankName, GuildStandard guildStandard)
            : base(GuildType.Guild)
        {
            Name = guildName;
            LeaderId = leaderSession.Player.CharacterId;
            Taxes = 0;

            // Add Default Ranks & Assign Default Permissions for Guild
            AddRank(new Rank(leaderRankName, Id, 0, GuildRankPermission.Leader, ulong.MaxValue, long.MaxValue, long.MaxValue));
            AddRank(new Rank(councilRankName, Id, 1, (GuildRankPermission.CouncilChat | GuildRankPermission.MemberChat | GuildRankPermission.Kick | GuildRankPermission.Invite | GuildRankPermission.ChangeMemberRank | GuildRankPermission.Vote), ulong.MaxValue, long.MaxValue, long.MaxValue));
            AddRank(new Rank(memberRankName, Id, 2, GuildRankPermission.MemberChat, 0, 0, 0));

            GuildStandard = guildStandard;

            Player player = leaderSession.Player;
            Member Leader = new Member(Id, player.CharacterId, GetRank(0), "", this);
            AddMember(Leader);
            OnlineMembers.Add(Leader.CharacterId, leaderSession);

            saveMask = GuildSaveMask.Create;
            base.saveMask = GuildBaseSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            base.Save(context);

            if (saveMask != GuildSaveMask.None)
            {
                if ((saveMask & GuildSaveMask.Create) != 0)
                {
                    context.Add(new GuildDataModel
                    {
                        Id = Id,
                        Taxes = Taxes,
                        AdditionalInfo = AdditionalInfo,
                        MessageOfTheDay = MessageOfTheDay,
                        BackgroundIconPartId = GuildStandard.BackgroundIcon.GuildStandardPartId,
                        ForegroundIconPartId = GuildStandard.ForegroundIcon.GuildStandardPartId,
                        ScanLinesPartId = GuildStandard.ScanLines.GuildStandardPartId
                    });
                }
                else
                {
                    // Guild already exists in database, save only data that has been modified
                    var model = new GuildDataModel
                    {
                        Id = Id
                    };

                    // could probably clean this up with reflection, works for the time being
                    EntityEntry<GuildDataModel> entity = context.Attach(model);
                    if ((saveMask & GuildSaveMask.MessageOfTheDay) != 0)
                    {
                        model.MessageOfTheDay = MessageOfTheDay;
                        entity.Property(p => p.MessageOfTheDay).IsModified = true;
                    }
                    if ((saveMask & GuildSaveMask.Taxes) != 0)
                    {
                        model.Taxes = Taxes;
                        entity.Property(p => p.Taxes).IsModified = true;
                    }
                    if ((saveMask & GuildSaveMask.AdditionalInfo) != 0)
                    {
                        model.AdditionalInfo = AdditionalInfo;
                        entity.Property(p => p.AdditionalInfo).IsModified = true;
                    }
                }

                saveMask = GuildSaveMask.None;
            }
        }

        public override GuildData BuildGuildDataPacket()
        {
            return new GuildData
            {
                GuildId = Id,
                GuildName = Name,
                Taxes = Taxes,
                Type = Type,
                Ranks = GetGuildRanksPackets().ToList(),
                GuildStandard = GuildStandard,
                TotalMembers = (uint)members.Count,
                UsersOnline = (uint)OnlineMembers.Count,
                GuildInfo =
                {
                    MessageOfTheDay = MessageOfTheDay,
                    AdditionalInfo = AdditionalInfo,
                    AgeInDays = (float)DateTime.Now.Subtract(CreateTime).TotalDays * -1f
                }
            };
        }

        public void SetTaxes(bool taxesEnabled)
        {
            Taxes = Convert.ToUInt32(taxesEnabled);
            saveMask |= GuildSaveMask.Taxes;
        }
    }
}
