using NexusForever.Shared.Network;
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
        private delegate void GuildOperationHandler(WorldSession session, ClientGuildOperation operation, GuildBase guildBase);

        /// <summary>
        /// Id to be assigned to the next created residence.
        /// </summary>
        public static ulong NextGuildId => nextGuildId++;
        private static ulong nextGuildId;

        private static readonly ConcurrentDictionary</*guildId*/ ulong, GuildBase> guilds = new ConcurrentDictionary<ulong, GuildBase>();
        private static readonly HashSet<GuildBase> deletedGuilds = new HashSet<GuildBase>();
        private static readonly Dictionary<GuildType, uint> maxGuildSize = new Dictionary<GuildType, uint>
        {
            { GuildType.Guild, 40u },
            { GuildType.Circle, 20u },
            { GuildType.ArenaTeam5v5, 9u },
            { GuildType.ArenaTeam3v3, 5u },
            { GuildType.ArenaTeam2v2, 3u },
            { GuildType.WarParty, 80u},
            { GuildType.Community, 5u }
        };

        private static double timeToSave = SaveDuration;

        /// <summary>
        /// Initialise the <see cref="GuildManager"/>, anmd build cache of all existing guilds
        /// </summary>
        public static void Initialise()
        {
            nextGuildId = CharacterDatabase.GetNextGuildId() + 1ul;

            foreach (GuildModel guild in CharacterDatabase.GetGuilds())
            {
                switch ((GuildType)guild.Type)
                {
                    case GuildType.Guild:
                        guilds.TryAdd(guild.Id, new Guild(guild.GuildData, guild));
                        break;
                    case GuildType.Circle:
                        guilds.TryAdd(guild.Id, new Circle(guild));
                        break;
                    case GuildType.ArenaTeam2v2:
                    case GuildType.ArenaTeam3v3:
                    case GuildType.ArenaTeam5v5:
                        guilds.TryAdd(guild.Id, new ArenaTeam(guild));
                        break;
                    case GuildType.WarParty:
                        guilds.TryAdd(guild.Id, new WarParty(guild));
                        break;
                    case GuildType.Community:
                        guilds.TryAdd(guild.Id, new Community(guild));
                        break;
                    default:
                        log.Warn($"Guild Type not recognised {(GuildType)guild.Type}");
                        break;
                }
            }

            log.Info($"Initialized {guilds.Count} Guilds.");

            InitialiseGuildOperationHandlers();
        }

        /// <summary>
        /// Initialise all <see cref="GuildOperationHandler"/> that will handle <see cref="GuildOperation"/>
        /// </summary>
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
                    Debug.Assert(parameterInfo.Length == 3);
                    Debug.Assert(typeof(WorldSession) == parameterInfo[0].ParameterType);
                    Debug.Assert(typeof(ClientGuildOperation) == parameterInfo[1].ParameterType);
                    Debug.Assert(typeof(GuildBase) == parameterInfo[2].ParameterType);
                    #endregion

                    GuildOperationHandler @delegate = (GuildOperationHandler)Delegate.CreateDelegate(typeof(GuildOperationHandler), method);
                    guildOperationHandlers.Add(attribute.Operation, @delegate);
                }
            }
        }

        /// <summary>
        /// Executes associated <see cref="GuildOperationHandler"/> if one exists to handle <see cref="GuildOperation"/>
        /// </summary>
        public static void HandleGuildOperation(WorldSession session, ClientGuildOperation operation)
        {
            if (guildOperationHandlers.ContainsKey(operation.Operation))
            {
                GetGuild(operation.GuildId, out GuildBase guild);
                GuildResult canOperate = HasGuildPermission(guild, session.Player.CharacterId);

                if (canOperate == GuildResult.Success)
                    guildOperationHandlers[operation.Operation](session, operation, guild);
                else
                    SendGuildResult(session, canOperate, guild);
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

        /// <summary>
        /// Checks the Character ID is a member of <see cref="GuildBase"/> and that the <see cref="GuildBase"/> exists
        /// </summary>
        /// <param name="guild"></param>
        /// <param name="characterId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static GuildResult HasGuildPermission(GuildBase guild, ulong characterId)
        {
            if (guild == null)
                return GuildResult.NotAGuild;

            if (guild.GetMember(characterId) == null)
                return GuildResult.NotInThatGuild;

            return GuildResult.Success;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occured.
        /// </summary>
        public static void Update(double lastTick)
        {
            timeToSave -= lastTick;
            if (timeToSave <= 0d)
            {
                var tasks = new List<Task>();

                // Save deleted guilds first
                foreach (var guild in deletedGuilds)
                {
                    tasks.Add(GetSaveTask(guild));
                    tasks.Add(guild.ClearDeleted());
                }

                foreach (var guild in guilds.Values)
                {
                    tasks.Add(GetSaveTask(guild));
                    tasks.Add(guild.ClearDeleted());
                }

                Task.WaitAll(tasks.ToArray());
                deletedGuilds.Clear();

                timeToSave = SaveDuration;
            }
        }

        /// <summary>
        /// Returns appropriate <see cref="Task"/> to handle saving that <see cref="IGuild"/>
        /// </summary>
        private static Task GetSaveTask(GuildBase guild)
        {
            switch (guild.Type)
            {
                case GuildType.Guild:
                    return CharacterDatabase.SaveGuild((Guild)guild);
                case GuildType.Circle:
                    return CharacterDatabase.SaveGuild((Circle)guild);
                case GuildType.ArenaTeam2v2:
                case GuildType.ArenaTeam3v3:
                case GuildType.ArenaTeam5v5:
                    return CharacterDatabase.SaveGuild((ArenaTeam)guild);
                case GuildType.WarParty:
                    return CharacterDatabase.SaveGuild((WarParty)guild);
                case GuildType.Community:
                    return CharacterDatabase.SaveGuild((Community)guild);
            }

            return null;
        }

        /// <summary>
        /// Returns <see cref="GuildBase"/> if one exists with the passed guild ID
        /// </summary>
        public static GuildBase GetGuild(ulong guildId)
        {
            guilds.TryGetValue(guildId, out GuildBase guild);
            return guild;
        }

        /// <summary>
        /// Returns <see cref="GuildBase"/> if one exists with the passed guild ID, as an <see cref="out"/> parameter
        /// </summary>
        public static void GetGuild(ulong guildId, out GuildBase guild)
        {
            guilds.TryGetValue(guildId, out GuildBase guildBase);
            guild = guildBase;
        }

        /// <summary>
        /// Entry method to registering any <see cref="GuildBase"/>. Should only be called from the guild handler.
        /// </summary>
        public static void RegisterGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            GuildResult result = GuildResult.Success;

            if (clientGuildRegister.GuildType == GuildType.Guild && session.Player.GuildId > 0)
                result = GuildResult.AtMaxGuildCount;

            if (clientGuildRegister.GuildType == GuildType.Circle && session.Player.GuildMemberships.Where(i => guilds[i].Type == GuildType.Circle).Count() >= 5)
                result = GuildResult.AtMaxCircleCount;

            if (result == GuildResult.Success)
            {
                // TODO: Deduct cost of creation; 10g = Guild, 50p/600 ServiceTokens = Community

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
                        GuildBase newCircle = CreateCircle(session, clientGuildRegister);
                        if (newCircle != null)
                            SendPacketsAfterJoin(session, newCircle, GuildResult.YouCreated);
                        break;
                    case GuildType.ArenaTeam2v2:
                    case GuildType.ArenaTeam3v3:
                    case GuildType.ArenaTeam5v5:
                        GuildBase newArenaTeam = CreateArenaTeam(session, clientGuildRegister);
                        if (newArenaTeam != null)
                            SendPacketsAfterJoin(session, newArenaTeam, GuildResult.YouCreated);
                        break;
                    case GuildType.WarParty:
                        GuildBase newWarParty = CreateWarParty(session, clientGuildRegister);
                        if (newWarParty != null)
                            SendPacketsAfterJoin(session, newWarParty, GuildResult.YouCreated);
                        break;
                    case GuildType.Community:
                        GuildBase newCommunity = CreateCommunity(session, clientGuildRegister);
                        if (newCommunity != null)
                            SendPacketsAfterJoin(session, newCommunity, GuildResult.YouCreated);
                        break;
                }
            }
            else
                SendGuildResult(session, result);
        }

        /// <summary>
        /// Returns newly created <see cref="Guild"/> for use when registering
        /// </summary>
        private static Guild CreateGuild(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new Guild(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle, clientGuildRegister.GuildStandard);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        /// <summary>
        /// Returns newly created <see cref="Circle"/> for use when registering
        /// </summary>
        private static Circle CreateCircle(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new Circle(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        /// <summary>
        /// Returns newly created <see cref="WarParty"/> for use when registering
        /// </summary>
        private static WarParty CreateWarParty(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new WarParty(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        /// <summary>
        /// Returns newly created <see cref="ArenaTeam"/> for use when registering
        /// </summary>
        private static ArenaTeam CreateArenaTeam(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new ArenaTeam(session, clientGuildRegister.GuildType, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        /// <summary>
        /// Returns newly created <see cref="Community"/> for use when registering
        /// </summary>
        private static Community CreateCommunity(WorldSession session, ClientGuildRegister clientGuildRegister)
        {
            var guild = new Community(session, clientGuildRegister.GuildName, clientGuildRegister.MasterTitle, clientGuildRegister.CouncilTitle, clientGuildRegister.MemberTitle);
            guilds.TryAdd(guild.Id, guild);
            return guild;
        }

        /// <summary>
        /// Handles joining a <see cref="Player"/> to a <see cref="GuildBase"/>, based on a <see cref="GuildInvite"/>. Should only be called from the guild handler.
        /// </summary>
        public static void JoinGuild(WorldSession session, GuildInvite guildInvite)
        {
            GuildResult result = GuildResult.Success;

            if (guildInvite == null)
                throw new ArgumentNullException("Guild invite is null.");

            guilds.TryGetValue(guildInvite.GuildId, out GuildBase guild);
            if (guild == null)
                result = GuildResult.NotAGuild;
            else if (guild.GetMemberCount() >= maxGuildSize[guild.Type])
                result = GuildResult.CannotInviteGuildFull;

            if (result == GuildResult.Success)
            {
                if (guild.Type == GuildType.Guild)
                    session.Player.GuildId = guild.Id;
                session.Player.GuildMemberships.Add(guild.Id);

                // Update guild with result and new player
                guild.AddMember(new Member(guild.Id, session.Player.CharacterId, guild.GetRank(9), "", guild));
                guild.AnnounceGuildResult(GuildResult.InviteAccepted, referenceText: session.Player.Name);
                guild.OnlineMembers.Add(session.Player.CharacterId, session);

                // Send Join event to player responding
                SendPacketsAfterJoin(session, guild, GuildResult.YouJoined);
                guild.AnnounceGuildMemberChange(session.Player.CharacterId);
            }
            else
                SendGuildResult(session, result, referenceText: guild.Name);
        }

        /// <summary>
        /// Deletes <see cref="GuildBase"/> with the passed guild ID
        /// </summary>
        /// <param name="guildId"></param>
        public static void DeleteGuild(ulong guildId)
        {
            guilds.TryGetValue(guildId, out GuildBase guild);
            if (guild == null)
                throw new ArgumentNullException($"Guild not found with ID {guildId}");

            guild.Delete();
            guilds.Remove(guildId, out GuildBase guildBase);
            deletedGuilds.Add(guildBase);
        }

        /// <summary>
        /// Returns all <see cref="GuildBase"/> that a player is associated with
        /// </summary>
        public static IEnumerable<GuildBase> GetMatchingGuilds(ulong characterId)
        {
            return guilds.Values.Where(i => i.GetMember(characterId) != null).OrderBy(x => x.Type).ThenBy(x => x.Id); // Ordering for GuildOperations which operate on index
        }

        /// <summary>
        /// Used to trigger login events for <see cref="Player"/>, and forward them to appropriate <see cref="GuildBase"/>
        /// </summary>
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

        /// <summary>
        /// Used to trigger logout events for <see cref="Player"/>, and forward them to appropriate <see cref="GuildBase"/>
        /// </summary>
        public static void OnPlayerLogout(WorldSession session, Player player)
        {
            foreach (ulong guildId in session.Player.GuildMemberships)
            {
                guilds.TryGetValue(guildId, out GuildBase guild);
                if (guild != null)
                    guild.OnPlayerLogout(session, player);
            }
        }

        /// <summary>
        /// Used to send initial packets to the <see cref="Player"/> containing associated guilds
        /// </summary>
        public static void SendInitialPackets(WorldSession session)
        {
            List<GuildData> playerGuilds = new List<GuildData>();
            List<GuildMember> playerMemberInfo = new List<GuildMember>();
            List<GuildPlayerLimits> playerUnknowns = new List<GuildPlayerLimits>();
            foreach(ulong guildId in session.Player.GuildMemberships)
            {
                playerGuilds.Add(guilds[guildId].BuildGuildDataPacket());
                playerMemberInfo.Add(guilds[guildId].GetMember(session.Player.CharacterId).BuildGuildMemberPacket());
                playerUnknowns.Add(new GuildPlayerLimits());
            }

            int index = playerGuilds.FindIndex(a => a.GuildId == session.Player.GuildAffiliation);
            ServerGuildInit serverGuildInit = new ServerGuildInit
            {
                NameplateIndex = (uint)index,
                Guilds = playerGuilds,
                Self = playerMemberInfo,
                SelfPrivate = playerUnknowns
            };
            session.EnqueueMessageEncrypted(serverGuildInit);
        }

        /// <summary>
        /// Sends all packets required to instruct the <see cref="Player"/> that they have joined the <see cref="GuildBase"/>. Should only be called by <see cref="JoinGuild(WorldSession, GuildInvite)"/>
        /// </summary>
        private static void SendPacketsAfterJoin(WorldSession session, GuildBase newGuild, GuildResult result)
        {
            session.Player.GuildMemberships.Add(newGuild.Id);
            if (session.Player.GuildAffiliation == 0)
                session.Player.GuildAffiliation = newGuild.Id;

            SendGuildJoin(session, newGuild.BuildGuildDataPacket(), newGuild.GetMember(session.Player.CharacterId).BuildGuildMemberPacket(), new GuildPlayerLimits());
            SendGuildResult(session, result, newGuild, referenceText: newGuild.Name);
            SendGuildAffiliation(session);
            SendGuildRoster(session, newGuild.GetGuildMembersPackets().ToList(), newGuild.Id);
        }

        /// <summary>
        /// Sends <see cref="ServerGuildJoin"/> packet to the <see cref="Player"/> with appropriate data
        /// </summary>
        private static void SendGuildJoin(WorldSession session, GuildData guildData, GuildMember guildMember, GuildPlayerLimits guildUnknown)
        {
            ServerGuildJoin serverGuildJoin = new ServerGuildJoin
            {
                GuildData = guildData,
                Self = guildMember,
                SelfPrivate = guildUnknown
            };

            session.EnqueueMessageEncrypted(serverGuildJoin);
        }

        /// <summary>
        /// Sends <see cref="ServerGuildResult"/> packet to the <see cref="Player"/> with appropriate data
        /// </summary>
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

        /// <summary>
        /// Sends <see cref="ServerGuildRoster"/> packet to the <see cref="Player"/> with appropriate data
        /// </summary>
        private static void SendGuildRoster(WorldSession session, List<GuildMember> guildMembers, ulong guildId)
        {
            ServerGuildRoster serverGuildMembers = new ServerGuildRoster
            {
                GuildRealm = WorldServer.RealmId,
                GuildId = guildId,
                GuildMembers = guildMembers,
                Done = true
            };

            session.EnqueueMessageEncrypted(serverGuildMembers);
        }

        /// <summary>
        /// Sends <see cref="ServerEntityGuildAffiliation"/> packet to the <see cref="Player"/> and all surrounding <see cref="Entity"/> with appropriate data
        /// </summary>
        private static void SendGuildAffiliation(WorldSession session)
        {
            if (session.Player.GuildAffiliation == 0)
                return;

            GuildBase guild = GetGuild(session.Player.GuildAffiliation);
            if (guild == null)
                return;

            if (guild.GetMember(session.Player.CharacterId) == null)
                return;
            
            session.Player.EnqueueToVisible(new ServerEntityGuildAffiliation
            {
                UnitId = session.Player.Guid,
                GuildName = guild.Name,
                GuildType = guild.Type
            }, true);
        }

        /// <summary>
        /// Sends <see cref="ServerGuildInvite"/> packet to the <see cref="Player"/> with appropriate data
        /// </summary>
        private static void SendPendingInvite(WorldSession session)
        {
            if (session.Player.PendingGuildInvite == null)
                return;

            var guild = GetGuild(session.Player.PendingGuildInvite.GuildId);
            uint taxes = 0;
            if (guild.Type == GuildType.Guild)
            {
                Guild guildInstance = (Guild)guilds[session.Player.PendingGuildInvite.GuildId];
                taxes = guildInstance.Flags;
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

        /// <summary>
        /// Handles removing a <see cref="Player"/> from a <see cref="GuildBase"/> and updating the server and client data appropriately
        /// </summary>
        /// <param name="session"></param>
        /// <param name="result"></param>
        /// <param name="guild"></param>
        /// <param name="referenceText"></param>
        private static void HandlePlayerRemove(WorldSession session, GuildResult result, GuildBase guild, string referenceText = "")
        {
            SendGuildResult(session,result, referenceText: referenceText.Length > 0 ? referenceText : session.Player.Name);

            if (session.Player.GuildId == guild.Id)
                session.Player.GuildId = 0;
            session.Player.GuildMemberships.Remove(guild.Id);
            session.EnqueueMessageEncrypted(new ServerGuildRemove
            {
                RealmId = WorldServer.RealmId,
                GuildId = guild.Id
            });

            session.Player.GuildAffiliation = 0;
            session.Player.EnqueueToVisible(new ServerEntityGuildAffiliation
            {
                UnitId = session.Player.Guid,
            }, true);
        }
    }
}
