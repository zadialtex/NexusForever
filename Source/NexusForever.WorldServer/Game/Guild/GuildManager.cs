using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Guild
{
    public static class GuildManager
    {
        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        /// <summary>
        /// Id to be assigned to the next created residence.
        /// </summary>
        public static ulong NextGuildId => nextGuildId++;
        private static ulong nextGuildId;

        private static readonly ConcurrentDictionary</*guildId*/ ulong, Guild> guilds = new ConcurrentDictionary<ulong, Guild>();

        private static double timeToSave = SaveDuration;

        public static void Initialise()
        {
            nextGuildId = 1ul;
        }

        public static void Update(double lastTick)
        {
            timeToSave -= lastTick;
            if (timeToSave <= 0d)
            {
                var tasks = new List<Task>();
                foreach (Guild guild in guilds.Values)
                    tasks.Add(CharacterDatabase.SaveGuild(guild));

                Task.WaitAll(tasks.ToArray());

                timeToSave = SaveDuration;
            }
        }

        public static void RegisterGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            GuildResult result = GuildResult.Success;

            if (result == GuildResult.Success)
            {
                if(clientGuildRegister.GuildType == GuildType.Guild)
                {
                    Guild newGuild = CreateGuild(session, clientGuildRegister);
                    if (newGuild != null)
                    {
                        result = GuildResult.YouCreated;
                        SendGuildJoin(session, newGuild.BuildServerGuildData(), newGuild.GetGuildMember(session.Player.CharacterId), new GuildUnknown());
                        SendGuildResult(session, result, newGuild.Name);
                        SendGuildRoster(session, newGuild.GetMembers().ToList(), newGuild.Id);
                    }
                }
            }
        }

        private static Guild CreateGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new Guild(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle, clientGuildRegister.GuildStandard);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        private static void SendGuildJoin(WorldSession session, GuildData guildData, GuildMember guildMember, GuildUnknown guildUnknown)
        {
            ServerGuildJoin serverGuildJoin = new ServerGuildJoin
            {
                GuildData = guildData,
                PlayerMembership = guildMember,
                GuildUnknown = guildUnknown
            };

            session.EnqueueMessageEncrypted(serverGuildJoin);
        }

        private static void SendGuildResult(WorldSession session, GuildResult guildResult, string guildName)
        {
            ServerGuildResult serverGuildResult = new ServerGuildResult
            {
                Realm = WorldServer.RealmId,
                CharacterId = session.Player.CharacterId,
                GuildName = guildName,
                Result = guildResult
            };

            session.EnqueueMessageEncrypted(serverGuildResult);
        }

        private static void SendGuildRoster(WorldSession session, List<GuildMember> guildMembers, ulong guildId)
        {
            ServerGuildMembers serverGuildMembers = new ServerGuildMembers
            {
                GuildRealm = WorldServer.RealmId,
                GuildId = guildId,
                GuildMembers = guildMembers,
                Unknown0 = true
            };

            session.EnqueueMessageEncrypted(serverGuildMembers);
        }
    }
}
