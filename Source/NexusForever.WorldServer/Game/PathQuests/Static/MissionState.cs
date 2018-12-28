using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.PathQuests.Static
{
    [Flags]
    public enum MissionState
    {
        None      = 0,
        Unlocked  = 1,
        Complete  = 2
    }
}
