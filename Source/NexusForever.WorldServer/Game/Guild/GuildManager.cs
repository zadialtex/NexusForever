using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GuildModel = NexusForever.WorldServer.Database.Character.Model.Guild;

namespace NexusForever.WorldServer.Game.Guild
{
    public static partial class GuildManager
    {
        private static ILogger log { get; } = LogManager.GetCurrentClassLogger();

        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        /// <summary>
        /// Set up guild operation handlers
        /// </summary>
        private static readonly Dictionary<GuildOperation, GuildOperationHandler> guildOperationHandlers
            = new Dictionary<GuildOperation, GuildOperationHandler>();
        private delegate void GuildOperationHandler(WorldSession session, ClientGuildOperation operation);

        /// <summary>
        /// Id to be assigned to the next created residence.
        /// </summary>
        public static ulong NextGuildId => nextGuildId++;
        private static ulong nextGuildId;

        private static readonly ConcurrentDictionary</*guildId*/ ulong, GuildBase> guilds = new ConcurrentDictionary<ulong, GuildBase>();
        private static readonly Dictionary<GuildType, uint> maxGuildSize = new Dictionary<GuildType, uint>
        {
            { GuildType.Guild, 40u },
            { GuildType.Circle, 20u }
        };

        private static double timeToSave = SaveDuration;

        public static void Initialise()
        {
            nextGuildId = 1712381ul;

            foreach (GuildModel guild in CharacterDatabase.GetGuilds())
            {
                switch ((GuildType)guild.Type)
                {
                    case GuildType.Guild:
                        guilds.TryAdd(guild.Id, new Guild(guild.GuildData, guild));
                        break;
                }
            }

            log.Info($"Initialized {guilds.Count} Guilds.");

            InitialiseGuildOperationHandlers();
        }

        private static void InitialiseGuildOperationHandlers()
        {
            IEnumerable<MethodInfo> methods = Assembly.GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static));

            foreach (MethodInfo method in methods)
            {
                IEnumerable<GuildOperationHandlerAttribute> attributes = method.GetCustomAttributes<GuildOperationHandlerAttribute>();
                foreach (GuildOperationHandlerAttribute attribute in attributes)
                {
                    #region Debug
                    ParameterInfo[] parameterInfo = method.GetParameters();
                    Debug.Assert(parameterInfo.Length == 2);
                    Debug.Assert(typeof(WorldSession) == parameterInfo[0].ParameterType);
                    Debug.Assert(typeof(ClientGuildOperation) == parameterInfo[1].ParameterType);
                    #endregion

                    GuildOperationHandler @delegate = (GuildOperationHandler)Delegate.CreateDelegate(typeof(GuildOperationHandler), method);
                    guildOperationHandlers.Add(attribute.Operation, @delegate);
                }
            }
        }

        public static void HandleGuildOperation(WorldSession session, ClientGuildOperation operation)
        {
            if (guildOperationHandlers.ContainsKey(operation.Operation))
            {
                if (HasPermission(operation.GuildId, session.Player.CharacterId, operation.Operation))
                    guildOperationHandlers[operation.Operation](session, operation);
                else
                    session.EnqueueMessageEncrypted(new ServerGuildResult
                    {
                        RealmId = WorldServer.RealmId,
                        GuildId = session.Player.CharacterId,
                        Result = GuildResult.RankLacksSufficientPermissions
                    });
            }
            else
            {
                log.Info($"GuildOperation {operation.Operation} has no handler implemented.");

                session.EnqueueMessageEncrypted(new ServerChat
                {
                    Channel = Social.ChatChannel.Debug,
                    Name = "GuildManager",
                    Text = $"{operation.Operation} currently not implemented",
                });
            }
        }

        public static void Update(double lastTick)
        {
            timeToSave -= lastTick;
            if (timeToSave <= 0d)
            {
                var tasks = new List<Task>();
                foreach (var guild in guilds.Values)
                {
                    if(guild.Type == GuildType.Guild)
                        tasks.Add(CharacterDatabase.SaveGuild((Guild)guild));

                    tasks.Add(guild.ClearDeleted());
                }

                Task.WaitAll(tasks.ToArray());

                timeToSave = SaveDuration;
            }
        }

        public static void RegisterGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            GuildResult result = GuildResult.Success;

            if (clientGuildRegister.GuildType == GuildType.Guild && session.Player.GuildId > 0)
                result = GuildResult.AtMaxGuildCount;

            if (clientGuildRegister.GuildType == GuildType.Guild && session.Player.GuildMemberships.Where(i => guilds[i].GetType().Name == "Circle").Count() >= 5)
                result = GuildResult.AtMaxCircleCount;

            if (result == GuildResult.Success)
            {
                switch (clientGuildRegister.GuildType)
                {
                    case GuildType.Guild:
                        Guild newGuild = CreateGuild(session, clientGuildRegister);
                        if (newGuild != null)
                        {
                            session.Player.GuildId = newGuild.Id;
                            SendPacketsAfterJoin(session, newGuild, GuildResult.YouCreated);
                        }
                        break;
                    case GuildType.Circle:
                        SendGuildResult(session, GuildResult.AtMaxCircleCount);
                        break;
                    case GuildType.ArenaTeam2v2:
                    case GuildType.ArenaTeam3v3:
                    case GuildType.ArenaTeam5v5:
                        break;
                    case GuildType.WarParty:
                        break;
                    case GuildType.Community:
                        break;
                }
            }
            else
                SendGuildResult(session, result);
        }

        private static Guild CreateGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new Guild(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle, clientGuildRegister.GuildStandard);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        public static bool HasPermission(ulong guildId, ulong characterId, GuildOperation operation)
        {
            return true;
        }

        public static IEnumerable<GuildBase> GetMatchingGuilds(ulong playerId)
        {
            return guilds.Values.Where(i => i.GetMember(playerId) != null).OrderBy(x => x.Type).ThenBy(x => x.Id); // Ordering for GuildOperations which operate on index
        }

        public static void OnPlayerLogin(WorldSession session, Player player)
        {
            var matchingGuilds = GetMatchingGuilds(player.CharacterId);
            foreach (var guild in matchingGuilds)
            {
                if (guild.Type == GuildType.Guild)
                    player.GuildId = guild.Id;

                player.GuildMemberships.Add(guild.Id);
                guild.OnPlayerLogin(session, player);
            }
        }

        public static void OnPlayerLogout(WorldSession session, Player player)
        {
            foreach (ulong guildId in session.Player.GuildMemberships)
            {
                guilds[guildId].OnPlayerLogout(session, player);
            }
        }

        public static void SendInitialPackets(WorldSession session)
        {
            List<GuildData> playerGuilds = new List<GuildData>();
            List<GuildMember> playerMemberInfo = new List<GuildMember>();
            List<GuildUnknown> playerUnknowns = new List<GuildUnknown>();
            foreach(ulong guildId in session.Player.GuildMemberships)
            {
                playerGuilds.Add(guilds[guildId].BuildGuildDataPacket());
                playerMemberInfo.Add(guilds[guildId].GetMember(session.Player.CharacterId).BuildGuildMemberPacket());
                playerUnknowns.Add(new GuildUnknown());
            }
            
            ServerGuildInit serverGuildInit = new ServerGuildInit
            {
                Guilds = playerGuilds,
                PlayerMemberInfo = playerMemberInfo,
                GuildUnknownList = playerUnknowns
            };
            session.EnqueueMessageEncrypted(serverGuildInit);
        }

        public static async Task SendPacketsAfterAddToMap(WorldSession session)
        {
            // Delay allows the roster to load after the player has entered the game. This triggers chat channel joined & motd messages in the client.
            // TODO: Remove Delay and figure out which client packet or event needs to be fired before it's ready to accept this packet.
            await Task.Delay(5000);

            foreach (ulong guildId in session.Player.GuildMemberships)
                SendGuildRoster(session, guilds[guildId].GetGuildMembersPackets().ToList(), guildId);

            SendPendingInvite(session);
        }

        private static void SendPacketsAfterJoin(WorldSession session, GuildBase newGuild, GuildResult result)
        {
            session.Player.GuildMemberships.Add(newGuild.Id);

            SendGuildJoin(session, newGuild.BuildGuildDataPacket(), newGuild.GetMember(session.Player.GuildId).BuildGuildMemberPacket(), new GuildUnknown());
            SendGuildResult(session, result, newGuild, referenceText: newGuild.Name);
            SendGuildRoster(session, newGuild.GetGuildMembersPackets().ToList(), newGuild.Id);
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

        private static void SendGuildResult(WorldSession session, GuildResult guildResult, GuildBase guild = null, uint referenceId = 0, string referenceText = "")
        {
            ServerGuildResult serverGuildResult = new ServerGuildResult
            {
                Result = guildResult
            };

            if (guild != null)
            {
                serverGuildResult.RealmId = WorldServer.RealmId;
                serverGuildResult.GuildId = guild.Id;
                serverGuildResult.ReferenceId = referenceId;
                serverGuildResult.ReferenceText = referenceText;
            }

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

        private static void SendPendingInvite(WorldSession session)
        {
            var guild = guilds[session.Player.PendingGuildInvite.GuildId];
            uint taxes = 0;
            if (guild.GetType().Name == "Guild")
            {
                Guild guildInstance = (Guild)guilds[session.Player.PendingGuildInvite.GuildId];
                taxes = guildInstance.Taxes;
            }

            ServerGuildInvite serverGuildInvite = new ServerGuildInvite
            {
                GuildName = guild.Name,
                GuildType = guild.Type,
                PlayerName = session.Player.Name,
                Taxes = taxes
            };

            session.EnqueueMessageEncrypted(serverGuildInvite);
        }
    }
}
