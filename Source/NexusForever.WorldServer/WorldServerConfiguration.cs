using NexusForever.Shared.Configuration;
using System;

namespace NexusForever.WorldServer
{
    public class WorldServerConfiguration
    {
        public struct MapConfig
        {
            public string MapPath { get; set; }
        }

        public NetworkConfig Network { get; set; }
        public DatabaseConfig Database { get; set; }
        public MapConfig Map { get; set; }
        public bool UseCache { get; set; } = false;
        public ushort RealmId { get; set; }
        public uint LengthOfInGameDay { get; set; }
        public uint ShadesEveEffigyCount { get; set; }
        public DateTime ShadesEveEffigyBuilt { get; set; }
    }
}
