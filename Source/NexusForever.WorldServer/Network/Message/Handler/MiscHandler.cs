using System;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Contact.Static;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class MiscHandler
    {
        [MessageHandler(GameMessageOpcode.ClientPing)]
        public static void HandlePing(WorldSession session, ClientPing ping)
        {
            session.Heartbeat.OnHeartbeat();
        }

        /// <summary>
        /// Handled responses to Player Info Requests.
        /// TODO: Put this in the right place, this is used by Mail & Contacts, at minimum. Probably used by Guilds, Circles, etc. too.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        [MessageHandler(GameMessageOpcode.ClientPlayerInfoRequest)]
        public static void HandlePlayerInfoRequest(WorldSession session, ClientPlayerInfoRequest request)
        {
            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterById(request.Identity.CharacterId),
                character =>
            {
                if (character == null)
                    throw new InvalidPacketValueException();

                if (request.Type == ContactType.Ignore) // Ignored user data request
                    session.EnqueueMessageEncrypted(new ServerPlayerInfoBasicResponse
                    {
                        ResultCode = 0,
                        Identity = new TargetPlayerIdentity
                        {
                            RealmId = WorldServer.RealmId,
                            CharacterId = character.Id
                        },
                        Name = character.Name,
                        Faction = (Faction)character.FactionId,
                    });
                else
                    session.EnqueueMessageEncrypted(new ServerPlayerInfoFullResponse
                    {
                        BaseData = new ServerPlayerInfoBasicResponse
                        {
                            ResultCode = 0,
                            Identity = new TargetPlayerIdentity
                            {
                                RealmId = WorldServer.RealmId,
                                CharacterId = character.Id
                            },
                            Name = character.Name,
                            Faction = (Faction)character.FactionId
                        },
                        IsClassPathSet = true,
                        Path = (Path)character.ActivePath,
                        Class = (Class)character.Class,
                        Level = character.Level,
                        IsLastLoggedOnInDaysSet = true,
                        LastLoggedInDays = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id) != null ? 0 : -30f // TODO: Get Last Online from DB & Calculate Time Offline (Hard coded for 30 days currently)
                    });
            }));
        }
        
        [MessageHandler(GameMessageOpcode.ClientReplayLevelRequest)]
        public static void HandleReplayLevel(WorldSession session, ClientReplayLevelUp request)
        {
            session.Player.PlayLevelUpEffect((byte)request.Level);
        }

        [MessageHandler(GameMessageOpcode.ClientToggleWeapons)]
        public static void HandleWeaponToggle(WorldSession session, ClientToggleWeapons toggleWeapons)
        {
            session.Player.Sheathed = toggleWeapons.ToggleState;
        }

        [MessageHandler(GameMessageOpcode.ClientRandomRollRequest)]
        public static void HandleRandomRoll(WorldSession session, ClientRandomRollRequest randomRoll)
        {
            if ( randomRoll.MinRandom > randomRoll.MaxRandom)
                throw new InvalidPacketValueException();

            if (randomRoll.MaxRandom > 1000000u)
                throw new InvalidPacketValueException();

            session.EnqueueMessageEncrypted(new ServerRandomRollResponse
            {
                TargetPlayerIdentity = new TargetPlayerIdentity
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = session.Player.CharacterId
                },
                MinRandom = randomRoll.MinRandom,
                MaxRandom = randomRoll.MaxRandom,
                RandomRollResult = new Random().Next((int)randomRoll.MinRandom, (int)randomRoll.MaxRandom)
            });
        }
    }
}
