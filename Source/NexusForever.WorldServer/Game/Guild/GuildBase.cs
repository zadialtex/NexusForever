using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using GuildModel = NexusForever.WorldServer.Database.Character.Model.Guild;
using GuildRankModel = NexusForever.WorldServer.Database.Character.Model.GuildRank;
using GuildMemberModel = NexusForever.WorldServer.Database.Character.Model.GuildMember;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NexusForever.WorldServer.Game.Guild
{
    public abstract class GuildBase
    {
        public ulong Id { get; }
        public GuildType Type { get; }
        public string Name { get; protected set; }
        public ulong LeaderId { get; protected set; }
        public GuildMember Leader { get; protected set; }
        public DateTime CreateTime { get; }

        public GuildBaseSaveMask saveMask { get; protected set; }

        public Dictionary</*index*/byte, Rank> Ranks { get; private set; } = new Dictionary<byte, Rank>();
        public Dictionary</*id*/ulong, Member> Members { get; private set; } = new Dictionary<ulong, Member>();
        public Dictionary</*id*/ulong, WorldSession> OnlineMembers { get; private set; } = new Dictionary<ulong, WorldSession>();

        protected GuildBase(GuildType guildType)
        {
            Id = 1;
            Type = guildType;
            CreateTime = DateTime.Now;
        }

        protected void Save(CharacterContext context)
        {
            if (saveMask != GuildBaseSaveMask.None)
            {
                if ((saveMask & GuildBaseSaveMask.Create) != 0)
                {
                    context.Add(new GuildModel
                    {
                        Id = Id,
                        Type = (byte)Type,
                        Name = Name,
                        LeaderId = LeaderId,
                        CreateTime = CreateTime
                    });
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new GuildModel
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

                saveMask = GuildBaseSaveMask.None;
            }

            foreach (Rank rank in Ranks.Values)
                rank.Save(context);

            foreach (Member member in Members.Values)
                member.Save(context);
        }

        protected IEnumerable<GuildData.Rank> GetRanks()
        {
            foreach (Rank rank in Ranks.Values)
            {
                yield return rank.GetGuildDataRank();
            }
        }

        public GuildMember GetGuildMember(ulong characterId)
        {
            return Members.Values.FirstOrDefault(i => i.CharacterId == characterId).GetGuildMember();
        }

        public IEnumerable<GuildMember> GetMembers()
        {
            foreach(Member member in Members.Values)
                yield return member.GetGuildMember();
        }
    }
}
