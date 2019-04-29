using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using GuildBaseModel = NexusForever.WorldServer.Database.Character.Model.Guild;
using GuildRankModel = NexusForever.WorldServer.Database.Character.Model.GuildRank;
using GuildMemberModel = NexusForever.WorldServer.Database.Character.Model.GuildMember;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model;
using System.Threading.Tasks;
using NexusForever.WorldServer.Game.CharacterCache;

namespace NexusForever.WorldServer.Game.Guild
{
    public abstract class GuildBase
    {
        public ulong Id { get; }
        public GuildType Type { get; }
        public string Name { get; protected set; }
        public ulong LeaderId { get; protected set; }
        public Member Leader { get; protected set; }
        public DateTime CreateTime { get; }

        public GuildBaseSaveMask saveMask { get; protected set; }

        protected Dictionary</*index*/byte, Rank> ranks { get; set; } = new Dictionary<byte, Rank>();
        private HashSet<Rank> deletedRanks { get; } = new HashSet<Rank>();
        protected Dictionary</*characterId*/ulong, Member> members { get; set; } = new Dictionary<ulong, Member>();
        private HashSet<Member> deletedMembers { get; } = new HashSet<Member>();
        public Dictionary</*id*/ulong, WorldSession> OnlineMembers { get; private set; } = new Dictionary<ulong, WorldSession>();

        protected GuildBase(GuildType guildType, GuildBaseModel model)
        {
            Id = model.Id;
            Type = (GuildType)model.Type;
            Name = model.Name;
            LeaderId = model.LeaderId;
            CreateTime = model.CreateTime;

            foreach (GuildRankModel guildRankModel in model.GuildRank)
                ranks.Add(guildRankModel.Index, new Rank(guildRankModel));

            foreach (GuildMemberModel guildMemberModel in model.GuildMember)
                members.Add(guildMemberModel.CharacterId, new Member(guildMemberModel, ranks[guildMemberModel.Rank], this));

            Leader = members[LeaderId];

            saveMask = GuildBaseSaveMask.None;
        }

        protected GuildBase(GuildType guildType)
        {
            Id = GuildManager.NextGuildId;
            Type = guildType;
            CreateTime = DateTime.Now;

            saveMask = GuildBaseSaveMask.Create;
        }

        protected void Save(CharacterContext context)
        {
            if (saveMask != GuildBaseSaveMask.None)
            {
                if ((saveMask & GuildBaseSaveMask.Create) != 0)
                {
                    context.Add(new GuildBaseModel
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
                    var model = new GuildBaseModel
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

            // Saving of deleted ranks must occur before saving of new or existing ranks so that the primary key is available
            foreach (Rank rank in deletedRanks)
                rank.Save(context);

            foreach (Rank rank in ranks.Values)
                rank.Save(context);

            foreach (Member member in deletedMembers)
                member.Save(context);

            foreach (Member member in members.Values)
                member.Save(context);
        }

        public void OnPlayerLogin(WorldSession session, Player player)
        {
            // TODO: Announce to guild?
            AnnounceGuildMemberChange(player.CharacterId);
            AnnounceGuildResult(GuildResult.MemberOnline, referenceText: player.Name);

            OnlineMembers.TryAdd(player.CharacterId, session);
        }

        public void OnPlayerLogout(WorldSession session, Player player)
        {
            OnlineMembers.Remove(session.Player.CharacterId);

            // TODO: Announce to guild?
            AnnounceGuildMemberChange(player.CharacterId);
            AnnounceGuildResult(GuildResult.MemberOffline, referenceText: player.Name);
        }

        public void AddRank(Rank rank)
        {
            if (ranks.ContainsKey(rank.Index))
                throw new ArgumentOutOfRangeException("There is already a rank that exists with this index.");
            if (rank.Index > 9 || rank.Index < 0)
                throw new ArgumentOutOfRangeException("Rank Index invalid.");

            ranks.Add(rank.Index, rank);
        }

        public void RemoveRank(byte rankIndex)
        {
            if (rankIndex > 9)
                throw new ArgumentOutOfRangeException("Rank Index cannot be higher than the maximum rank count of 10.");
            if (!ranks.ContainsKey(rankIndex))
                throw new ArgumentNullException("Rank does not exist by that rank index");

            RemoveRank(ranks[rankIndex]);
        }

        private void RemoveRank(Rank rank)
        {
            rank.Delete();
            deletedRanks.Add(rank);
            ranks.Remove(rank.Index);
        }

        public bool RankExists(string name)
        {
            return ranks.Values.FirstOrDefault(i => i.Name == name) != null;
        }

        public Rank GetRank(byte index)
        {
            ranks.TryGetValue(index, out Rank rank);
            return rank;
        }

        public IEnumerable<GuildRank> GetGuildRanksPackets()
        {
            for (byte i = 0; i < 10; i++)
            {
                Rank rank = GetRank(i);
                if (rank != null)
                    yield return rank.BuildGuildRankPacket();
                else
                    yield return new GuildRank();
            }
        }

        public void AddMember(Member member)
        {
            if (members.ContainsKey(member.CharacterId))
                throw new ArgumentOutOfRangeException("That character already exists in the guild.");

            members.Add(member.CharacterId, member);
        }

        public void RemoveMember(ulong characterId, out WorldSession memberSession)
        {
            if (!members.ContainsKey(characterId))
                throw new ArgumentNullException("That character does not exist in the guild.");

            // Make sure the session is returned if it exists before removing from OnlineMembers
            OnlineMembers.TryGetValue(characterId, out WorldSession targetSession);
            memberSession = targetSession;

            RemoveMember(members[characterId]);
        }

        private void RemoveMember(Member member)
        {
            member.Delete();
            deletedMembers.Add(member);
            members.Remove(member.CharacterId);
        }

        public Member GetMember(string characterName)
        {
            return members.Values.FirstOrDefault(i => CharacterManager.GetCharacterInfo(i.CharacterId).Name == characterName);
        }

        public Member GetMember(ulong characterId)
        {
            members.TryGetValue(characterId, out Member member);
            return member;
        }

        public IEnumerable<Member> GetMembersOfRank(byte index)
        {
            return members.Values.Where(i => i.Rank.Index == index);
        }

        public IEnumerable<GuildMember> GetGuildMembersPackets()
        {
            foreach(Member member in members.Values)
                yield return member.BuildGuildMemberPacket();
        }

        public abstract GuildData BuildGuildDataPacket();

        public void SendToOnlineUsers(IWritable writable)
        {
            foreach (WorldSession targetSession in OnlineMembers.Values)
                targetSession.EnqueueMessageEncrypted(writable);
        }

        public void AnnounceGuildResult(GuildResult guildResult, uint referenceId = 0, string referenceText = "")
        {
            ServerGuildResult serverGuildResult = new ServerGuildResult
            {
                Result = guildResult
            };
            serverGuildResult.RealmId = WorldServer.RealmId;
            serverGuildResult.GuildId = Id;
            serverGuildResult.ReferenceId = referenceId;
            serverGuildResult.ReferenceText = referenceText;

            SendToOnlineUsers(serverGuildResult);
        }

        public void AnnounceGuildMemberChange(ulong characterId, ushort unknown0 = 0, ushort unknown1 = 1)
        {
            ServerGuildMemberChange serverGuildMemberChange = new ServerGuildMemberChange
            {
                RealmId = WorldServer.RealmId,
                GuildId = Id,
                GuildMember = members[characterId].BuildGuildMemberPacket(),
                Unknown0 = unknown0,
                Unknown1 = unknown1
            };

            SendToOnlineUsers(serverGuildMemberChange);
        }

        public async Task ClearDeleted()
        {
            deletedRanks.Clear();
            deletedMembers.Clear();
        }
    }
}
