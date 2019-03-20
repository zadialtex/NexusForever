using NexusForever.Shared.Network;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Character = NexusForever.WorldServer.Database.Character.Model.Character;
using CharacterContext = NexusForever.WorldServer.Database.Character.Model.CharacterContext;
using MemberModel = NexusForever.WorldServer.Database.Character.Model.GuildMember;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Member
    {
        public ulong GuildId { get; }
        public ulong CharacterId { get; }
        public Rank Rank { get; private set; }
        public string Note { get; private set; }
        private Character Character;
        private Player Player;

        private MemberSaveMask saveMask;

        public Member(ulong guildId, ulong characterId, Rank rank, string note, Player player = null, Character character = null)
        {
            GuildId = guildId;
            CharacterId = characterId;
            Rank = rank;
            Note = note;

            if (player != null)
                Player = player;

            if (character != null)
                Character = character;
            else
                SetCharacter();

            saveMask = MemberSaveMask.Create;
        }

        public async void SetCharacter()
        {
            Character = await CharacterDatabase.GetCharacterById(CharacterId);
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != MemberSaveMask.None)
            {
                if ((saveMask & MemberSaveMask.Create) != 0)
                {
                    context.Add(new MemberModel
                    {
                        Id = GuildId,
                        CharacterId = CharacterId,
                        Rank = Rank.Index,
                        Note = Note
                    });
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new MemberModel
                    {
                        Id = GuildId,
                        CharacterId = CharacterId,
                    };

                    // could probably clean this up with reflection, works for the time being
                    //EntityEntry <GuildModel> entity = context.Attach(model);
                    //if ((saveMask & GuildSaveMask.Name) != 0)
                    //{
                    //    model.Name = Name;
                    //    entity.Property(p => p.Name).IsModified = true;
                    //}
                }

                saveMask = MemberSaveMask.None;
            }
        }
        
        public GuildMember GetGuildMember()
        {
            if (Character != null || Player != null)
                return new GuildMember
                {
                    Realm = WorldServer.RealmId,
                    CharacterId = CharacterId,
                    Rank = Rank.Index,
                    Name = Character.Name ?? Player.Name,
                    Sex = (Sex)Character.Sex,
                    Class = (Class)Character.Class,
                    Path = (Path)Character.ActivePath,
                    Level = Convert.ToUInt32(Character.Level) ?? Player.Level,
                    Note = Note,
                    LastOnline = NetworkManager<WorldSession>.GetSession(x => x.Player?.CharacterId == CharacterId) != null ? -1f : -0.0007f
                };
            else
                return null;
        }
    }
}
