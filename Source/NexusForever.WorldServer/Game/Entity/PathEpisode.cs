using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PathEpisode : ISaveCharacter, IEnumerable<PathMission>
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public PathEpisodeEntry Entry { get; }
        public uint Id { get; }
        public ulong CharacterId { get; }
        public Player Player { get; }

        public bool RewardReceived
        {
            get => rewardReceived;
            set
            {
                if(value != rewardReceived)
                {
                    rewardReceived = value;
                    saveMask |= PathEpisodeSaveMask.RewardChange;
                }
            }
        }

        private bool rewardReceived;

        private Dictionary<uint, PathMission> missions = new Dictionary<uint, PathMission>();

        private PathEpisodeSaveMask saveMask;

        /// <summary>
        /// Create a new <see cref="PathEpisode"/> from an existing database model.
        /// </summary>
        public PathEpisode(CharacterPathEpisode model, PathMissionManager pathMissionManager)
        {
            CharacterId = model.Id;
            Id = model.EpisodeId;
            Entry = GameTableManager.PathEpisode.GetEntry(model.EpisodeId);
            RewardReceived = Convert.ToBoolean(model.RewardReceived);

            foreach (CharacterPathMission pathMissionModel in model.CharacterPathMission)
                missions.Add(pathMissionModel.MissionId, new PathMission(pathMissionModel));

            AddMissionsToLookup(pathMissionManager);

            saveMask = PathEpisodeSaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="PathEpisode"/> from an <see cref="PathEpisodeEntry"/> template.
        /// </summary>
        public PathEpisode(ulong owner, PathEpisodeEntry entry, IEnumerable<PathMissionEntry> pathMissions, PathMissionManager pathMissionManager, bool rewardReceived = false)
        {
            Id = entry.Id;
            CharacterId = owner;
            Entry = entry;
            RewardReceived = rewardReceived;

            foreach (PathMissionEntry pathMissionModel in pathMissions)
                missions.Add(pathMissionModel.Id, new PathMission(CharacterId, pathMissionModel));

            AddMissionsToLookup(pathMissionManager);

            saveMask = PathEpisodeSaveMask.Create;
        }

        private void AddMissionsToLookup(PathMissionManager pathMissionManager)
        {
            foreach (PathMission pathMission in missions.Values)
                pathMissionManager.AddMissionLookup(pathMission.Id, this);
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != PathEpisodeSaveMask.None)
            {
                if ((saveMask & PathEpisodeSaveMask.Create) != 0)
                {
                    // Currency doesn't exist in database, all infomation must be saved
                    context.Add(new CharacterPathEpisode
                    {
                        Id = CharacterId,
                        EpisodeId = Entry.Id,
                        RewardReceived = Convert.ToByte(RewardReceived),
                    });
                }
                else
                {
                    var model = new CharacterPathEpisode
                    {
                        Id = CharacterId,
                        EpisodeId = Entry.Id
                    };
                }
            }

            foreach (PathMission pathMission in missions.Values)
                pathMission.Save(context);

            saveMask = PathEpisodeSaveMask.None;
        }

        /// <summary>
        /// Returns a matching <see cref="PathMission"/> if it exists within this episode
        /// </summary>
        public PathMission GetMission(uint missionId)
        {
            return missions.TryGetValue(missionId, out PathMission pathMission) ? pathMission : null;
        }

        public IEnumerator<PathMission> GetEnumerator()
        {
            return missions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
