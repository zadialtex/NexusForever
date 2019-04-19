using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.Character.Model
{
    public partial class PropertyBase
    {
        public uint Type { get; set; }
        public uint Property { get; set; }
        public float Value { get; set; }
        public string Note { get; set; }
    }
}
