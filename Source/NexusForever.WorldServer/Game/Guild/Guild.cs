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
using GuildDataModel = NexusForever.WorldServer.Database.Character.Model.GuildData;
using GuildRankModel = NexusForever.WorldServer.Database.Character.Model.GuildRank;
using GuildMemberModel = NexusForever.WorldServer.Database.Character.Model.GuildMember;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Guild : GuildBase
    {
        public uint Taxes { get; private set; }
        public GuildStandard GuildStandard { get; private set; }

        private GuildSaveMask saveMask;

        /// <summary>
        /// Create a new <see cref="Guild"/>
        /// </summary>
        public Guild(WorldSession leaderSession, string guildName, string leaderRankName, string councilRankName, string memberRankName, GuildStandard guildStandard)
            : base(GuildType.Guild)
        {
            Name = guildName;
            LeaderId = leaderSession.Player.CharacterId;
            Taxes = 0;

            Ranks.Add(0, new Rank(leaderRankName, Id, 0, GuildRankPermission.Leader, ulong.MaxValue, long.MaxValue, long.MaxValue));
            Ranks.Add(1, new Rank(councilRankName, Id, 1, (GuildRankPermission.CouncilChat | GuildRankPermission.MemberChat | GuildRankPermission.Kick | GuildRankPermission.Invite | GuildRankPermission.ChangeMemberRank | GuildRankPermission.Vote), ulong.MaxValue, long.MaxValue, long.MaxValue));
            Ranks.Add(2, new Rank(memberRankName, Id, 2, GuildRankPermission.MemberChat, 0, 0, 0));

            GuildStandard = guildStandard;

            Player player = leaderSession.Player;
            Member Leader = new Member(Id, player.CharacterId, Ranks[0], "");
            Members.Add(Leader.CharacterId, Leader);
            OnlineMembers.Add(Leader.CharacterId, leaderSession);

            saveMask = GuildSaveMask.Create;
            base.saveMask = GuildBaseSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != GuildSaveMask.None)
            {
                base.Save(context);

                if ((saveMask & GuildSaveMask.Create) != 0)
                {
                    context.Add(new GuildDataModel
                    {
                        Id = Id,
                        Taxes = Taxes,
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
                    //EntityEntry <GuildModel> entity = context.Attach(model);
                    //if ((saveMask & GuildSaveMask.Name) != 0)
                    //{
                    //    model.Name = Name;
                    //    entity.Property(p => p.Name).IsModified = true;
                    //}
                }

                saveMask = GuildSaveMask.None;
            }
        }

        public GuildData BuildServerGuildData()
        {
            return new GuildData
            {
                GuildId = Id,
                GuildName = Name,
                Taxes = Taxes,
                Type = Type,
                Ranks = GetRanks().ToList(),
                GuildStandard = GuildStandard,
                TotalMembers = (uint)Members.Count,
                UsersOnline = (uint)OnlineMembers.Count,
                Unknown6 = 1,
                Unknown7 = 1,
                GuildInfo =
                {
                    AgeInDays = (float)DateTime.Now.Subtract(CreateTime).TotalHours * -1f
                }
            };
        }
    }
}
