using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.Character.Model
{
    public partial class CharacterPathMission
    {
        public ulong Id { get; set; }
        public uint EpisodeId { get; set; }
        public uint MissionId { get; set; }
        public uint Progress { get; set; }
        public uint State { get; set; }

        public virtual CharacterPathEpisode CharacterPathEpisode { get; set; }
    }
}
