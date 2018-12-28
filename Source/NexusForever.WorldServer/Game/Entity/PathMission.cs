using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.PathQuests.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PathMission : ISaveCharacter
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public PathMissionEntry Entry { get; }
        public uint Id { get; }
        public ulong CharacterId { get; }

        public bool IsUnlocked => (state & MissionState.Unlocked) != 0;
        public bool IsComplete => (state & MissionState.Complete) != 0;

        public uint Progress
        {
            get => progress;
            set
            {
                if (value != progress)
                {
                    progress = value;
                    saveMask |= PathMissionSaveMask.Progress;
                }
            }
        }
        private uint progress;

        public MissionState State
        {
            get => state;
            set
            {
                if (value != state)
                {
                    state = value;
                    saveMask |= PathMissionSaveMask.State;
                }
            }
        }
        private MissionState state;

        private PathMissionSaveMask saveMask;

        /// <summary>
        /// Create a new <see cref="PathMission"/> from an existing database model.
        /// </summary>
        public PathMission(CharacterPathMission model)
        {
            CharacterId = model.Id;
            Id = model.MissionId;
            Entry = GameTableManager.PathMission.GetEntry(model.MissionId);
            progress = model.Progress;
            state = (MissionState)model.State;

            saveMask = PathMissionSaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="PathMission"/> from an <see cref="PathMissionEntry"/> template.
        /// </summary>
        public PathMission(ulong owner, PathMissionEntry entry, bool completed = false, uint progress = 0, MissionState state = MissionState.None)
        {
            Id = entry.Id;
            CharacterId = owner;
            Entry = entry;
            this.progress = progress;
            this.state = state;

            saveMask = PathMissionSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != PathMissionSaveMask.None)
            {
                if ((saveMask & PathMissionSaveMask.Create) != 0)
                {
                    // Currency doesn't exist in database, all infomation must be saved
                    context.Add(new CharacterPathMission
                    {
                        Id = CharacterId,
                        EpisodeId = Entry.PathEpisodeId,
                        MissionId = Entry.Id,
                        Progress = Progress,
                        State = (uint)State
                    });
                }
                else
                {
                    // Currency already exists in database, save only data that has been modified
                    var model = new CharacterPathMission
                    {
                        Id = CharacterId,
                        EpisodeId = Entry.PathEpisodeId,
                        MissionId = Entry.Id
                    };

                    // could probably clean this up with reflection, works for the time being
                    EntityEntry<CharacterPathMission> entity = context.Attach(model);
                    if ((saveMask & PathMissionSaveMask.State) != 0)
                    {
                        model.State = (uint)state;
                        entity.Property(p => p.State).IsModified = true;
                    }
                }
            }

            saveMask = PathMissionSaveMask.None;
        }

        public void UnlockMission()
        {
            State |= MissionState.Unlocked;
        }

        public void Complete()
        {
            State |= MissionState.Complete;
        }
    }
}
