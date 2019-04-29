using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using GuildMemberModel = NexusForever.WorldServer.Database.Character.Model.GuildMember;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Member
    {
        public ulong GuildId { get; }
        public ulong CharacterId { get; }
        public Rank Rank { get; private set; }
        public string Note { get; private set; }
        
        private GuildBase Guild { get; } 
        private MemberSaveMask saveMask;

        public Member(GuildMemberModel model, Rank rank, GuildBase @base)
        {
            GuildId = model.Id;
            CharacterId = model.CharacterId;
            Rank = rank;
            Note = model.Note;

            Guild = @base;

            saveMask = MemberSaveMask.None;
        }

        public Member(ulong guildId, ulong characterId, Rank rank, string note, GuildBase @base)
        {
            GuildId = guildId;
            CharacterId = characterId;
            Rank = rank;
            Note = note;

            Guild = @base;

            saveMask = MemberSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != MemberSaveMask.None)
            {
                if ((saveMask & MemberSaveMask.Create) != 0)
                {
                    context.Add(new GuildMemberModel
                    {
                        Id = GuildId,
                        CharacterId = CharacterId,
                        Rank = Rank.Index,
                        Note = Note
                    });
                }
                else if ((saveMask & MemberSaveMask.Delete) != 0)
                {
                    var model = new GuildMemberModel
                    {
                        Id = GuildId,
                        CharacterId = CharacterId
                    };

                    context.Entry(model).State = EntityState.Deleted;
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new GuildMemberModel
                    {
                        Id = GuildId,
                        CharacterId = CharacterId,
                    };

                    // could probably clean this up with reflection, works for the time being
                    EntityEntry<GuildMemberModel> entity = context.Attach(model);
                    if ((saveMask & MemberSaveMask.Rank) != 0)
                    {
                        model.Rank = Rank.Index;
                        entity.Property(p => p.Rank).IsModified = true;
                    }
                }

                saveMask = MemberSaveMask.None;
            }
        }

        public void UpdateOnRankChange()
        {
            saveMask |= MemberSaveMask.Rank;
        }

        public void Delete()
        {
            // Entity won't exist if create flag exists, so we set to None and let GC get rid of it.
            if ((saveMask & MemberSaveMask.Create) == 0)
                saveMask = MemberSaveMask.Delete;
            else
                saveMask = MemberSaveMask.None;
        }

        public GuildMember BuildGuildMemberPacket()
        {
            ICharacter characterInfo = CharacterManager.GetCharacterInfo(CharacterId);
            return new GuildMember
            {
                Realm = WorldServer.RealmId,
                CharacterId = CharacterId,
                Rank = Rank.Index,
                Name = characterInfo.Name,
                Sex = characterInfo.Sex,
                Class = characterInfo.Class,
                Path = characterInfo.Path,
                Level = characterInfo.Level,
                Note = Note,
                LastOnline = characterInfo.GetOnlineStatus()
            };
        }
    }
}
