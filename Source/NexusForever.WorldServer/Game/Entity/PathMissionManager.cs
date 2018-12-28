using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.PathQuests.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PathMissionManager : ISaveCharacter
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly Player player;
        private readonly Dictionary</*episodeId*/uint, PathEpisode> episodes = new Dictionary<uint, PathEpisode>();
        private readonly Dictionary</*missionId*/uint, /*episodeId*/uint> missionLookup = new Dictionary<uint, uint>();

        /// <summary>
        /// Create a new <see cref="PathMissionManager"/> from <see cref="Player"/> database model.
        /// </summary>
        public PathMissionManager(Player owner, Character model)
        {
            player = owner;

            foreach (var characterPathEpisode in model.CharacterPathEpisode)
                episodes.Add(characterPathEpisode.EpisodeId, new PathEpisode(characterPathEpisode, this));

            //TODO: Check that all missions for each episode are accounted for and add missing ones. Need to do this in case DB crashes mid-write.
        }

        /// <summary>
        /// Create a new <see cref="CharacterPathEpisode"/>.
        /// </summary>
        public PathEpisode PathEpisodeCreate(uint episodeId)
        {
            PathEpisodeEntry pathEpisodeEntry = GameTableManager.PathEpisode.GetEntry(episodeId);
            if (pathEpisodeEntry == null)
                return null;

            return PathEpisodeCreate(pathEpisodeEntry);
        }

        /// <summary>
        /// Create a new <see cref="CharacterPathEpisode"/>.
        /// </summary>
        public PathEpisode PathEpisodeCreate(PathEpisodeEntry pathEpisodeEntry)
        {
            if (pathEpisodeEntry == null)
                return null;

            if (episodes.ContainsKey(pathEpisodeEntry.Id))
                throw new ArgumentException($"Path Episode {pathEpisodeEntry.Id} is already added to the player!");

            PathEpisode pathEpisode = new PathEpisode(
                player.CharacterId,
                pathEpisodeEntry,
                GetEpisodeMissions(pathEpisodeEntry.Id),
                this,
                false
            );
            episodes.Add(pathEpisodeEntry.Id, pathEpisode);
            return pathEpisode;
        }

        /// <summary>
        /// Returns all <see cref="PathMissionEntry"/> for a given <see cref="PathEpisodeEntry"/>
        /// </summary>
        /// <param name="pathEpisodeId"></param>
        /// <returns></returns>
        public IEnumerable<PathMissionEntry> GetEpisodeMissions(uint pathEpisodeId)
        {
            if (pathEpisodeId <= 0)
                return null;

            return GameTableManager.PathMission.Entries.Where(x => x.PathEpisodeId == pathEpisodeId);
        }

        /// <summary>
        /// Adds <see cref="PathEpisode"/> to <see cref="PathMission"/> lookup
        /// </summary>
        public void AddMissionLookup(uint missionId, PathEpisode pathEpisode)
        {
            missionLookup.TryAdd(missionId, pathEpisode.Id);
        }

        /// <summary>
        /// Initiates necessary methods for when a player loads into the game. Must be called after entity has been created.
        /// </summary>
        public void SendInitialPackets()
        {
            SetEpisodeProgress();
        }

        /// <summary>
        /// Sets the episode progress and sends to the player
        /// </summary>
        public void SetEpisodeProgress()
        {
            foreach (PathEpisode pathEpisode in episodes.Values)
                SendServerPathEpisodeProgress(pathEpisode.Id, pathEpisode.ToList());
        }

        /// <summary>
        /// Sets the current episode based on zone and sends to the player
        /// </summary>
        public void SetCurrentZoneEpisode()
        {
            PathEpisodeEntry currentMapEpisode = GetEpisodeForMap();
            if (currentMapEpisode != null)
            {
                SendServerPathCurrentEpisode((ushort)currentMapEpisode.WorldZoneId, (ushort)currentMapEpisode.Id);

                if (!episodes.TryGetValue(currentMapEpisode.Id, out PathEpisode pathEpisode))
                    PathEpisodeCreate(currentMapEpisode.Id);
            }
        }

        /// <summary>
        /// Get the matching <see cref="PathEpisodeEntry"/> for map the player's currently on
        /// </summary>
        /// <returns></returns>
        private PathEpisodeEntry GetEpisodeForMap()
        {
            // TODO: Use Zone ID & World ID when we can track zone
            uint worldId = player.Map.Entry.Id;
            PathEpisodeEntry matchedEpisode = GameTableManager.PathEpisode.Entries.FirstOrDefault(x => x.WorldId == worldId && x.PathTypeEnum == (uint)player.Path);

            return matchedEpisode;
        }

        private bool GetMission(uint missionId, out PathMission mission)
        {
            mission = null;

            PathMissionEntry pathMissionEntry = GameTableManager.PathMission.GetEntry(missionId);
            if (pathMissionEntry == null)
                throw new ArgumentOutOfRangeException($"PathMissionEntry not found for ID {missionId}"); // TODO: Use another custom Exception to reflect an entry missing the TBL

            if (episodes.TryGetValue(pathMissionEntry.PathEpisodeId, out PathEpisode pathEpisode))
            {
                mission = pathEpisode.GetMission(missionId);
            }

            return mission != null;
        }

        public void UnlockMission(uint missionId)
        {
            if (missionId == 0)
                throw new ArgumentException("Mission ID must be greater than 0");

            UnlockMissions(new List<uint>
            {
                missionId
            });
        }

        public void UnlockMissions(IEnumerable<uint> missionIds)
        {
            List<PathMission> missionsToSend = new List<PathMission>();

            foreach(uint missionId in missionIds)
            {
                PathMissionEntry pathMissionEntry = GameTableManager.PathMission.GetEntry(missionId);
                if (pathMissionEntry == null)
                    throw new ArgumentException($"Mission ID {missionId} did not match any PathMissionEntry");
                
                missionsToSend.Add(UnlockMission(pathMissionEntry));
            }

            SendServerPathMissionActivate(missionsToSend.ToArray());
        }

        /// <summary>
        /// Unlocks a <see cref="PathMissionEntry"/> for the Player
        /// </summary>
        /// <param name="pathMissionEntry"></param>
        private PathMission UnlockMission(PathMissionEntry pathMissionEntry)
        {
            if (GetMission(pathMissionEntry.Id, out PathMission matchingMission))
            {
                matchingMission.UnlockMission();

                return matchingMission;
            }

            return null;
        }

        public void UpdateMission(uint missionId, uint progress, MissionState state)
        {
            PathMissionEntry pathMissionEntry = GameTableManager.PathMission.GetEntry(missionId);
            if (pathMissionEntry == null)
                throw new ArgumentException($"Mission ID {missionId} did not match any PathMissionEntry");

            UpdateMission(pathMissionEntry, progress, state);
        }

        public void UpdateMission(PathMissionEntry pathMissionEntry, uint progress, MissionState state)
        {
            if (GetMission(pathMissionEntry.Id, out PathMission matchingMission))
            {
                matchingMission.Progress = Math.Clamp(matchingMission.Progress += progress, 0, 100);
                if (matchingMission.Progress >= 100)
                    matchingMission.Complete();

                matchingMission.State = state;

                SendServerPathMissionUpdate(matchingMission);
            }
        }

        public void Save(CharacterContext context)
        {
            //log.Debug($"PathMissionManager.Save called");
            foreach (PathEpisode pathEpisode in episodes.Values)
                pathEpisode.Save(context);
        }

        private void SendServerPathEpisodeProgress(uint episodeId, IEnumerable<PathMission> pathMissions)
        {
            List<ServerPathEpisodeProgress.Mission> missionProgress = new List<ServerPathEpisodeProgress.Mission>();

            foreach(PathMission pathMission in pathMissions)
            {
                if (!pathMission.IsUnlocked)
                    continue;

                missionProgress.Add(new ServerPathEpisodeProgress.Mission
                {
                    MissionId = pathMission.Id,
                    Completed = pathMission.IsComplete,
                    Userdata = pathMission.Progress,
                });
            }

            player.Session.EnqueueMessageEncrypted(new ServerPathEpisodeProgress
            {
                EpisodeId = (ushort)episodeId,
                Missions = missionProgress
            });
        }

        private void SendServerPathCurrentEpisode(ushort zoneId, ushort episodeId)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathCurrentEpisode
            {
                ZoneId = zoneId,
                EpisodeId = episodeId
            });
        }

        private void SendServerPathMissionActivate(PathMission[] pathMissions, byte reason = 1, uint giver = 0)
        {
            List<ServerPathMissionActivate.Mission> missionList = new List<ServerPathMissionActivate.Mission>();

            foreach (PathMission pathMission in pathMissions)
            {
                //log.Debug($"Activating {pathMission.Id}, {pathMission.Completed}, {pathMission.Progress}, {pathMission.State}");
                missionList.Add(new ServerPathMissionActivate.Mission
                {
                    MissionId = pathMission.Id,
                    Completed = pathMission.IsComplete,
                    Userdata = pathMission.Progress,
                    Reason = reason,
                    Giver = giver
                });
            }

            player.Session.EnqueueMessageEncrypted(new ServerPathMissionActivate
            {
                Missions = missionList       
            });
        }

        private void SendServerPathMissionUpdate(PathMission pathMission)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathMissionUpdate
            {
                MissionId = (ushort)pathMission.Id,
                Completed = pathMission.IsComplete,
                Userdata = pathMission.Progress
            });
        }
    }
}
