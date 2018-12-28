using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.Character.Model
{
    public partial class CharacterPathEpisode
    {
        public CharacterPathEpisode()
        {
            CharacterPathMission = new HashSet<CharacterPathMission>();
        }

        public ulong Id { get; set; }
        public uint EpisodeId { get; set; }
        public byte RewardReceived { get; set; }

        public virtual Character IdNavigation { get; set; }
        public virtual ICollection<CharacterPathMission> CharacterPathMission { get; set; }
    }
}
