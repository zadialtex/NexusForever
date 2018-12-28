using System;

namespace NexusForever.WorldServer.Game.Entity.Static
{
    /// <summary>
    /// Determines which fields need saving for <see cref="PathEpisode"/> when being saved to the database.
    /// </summary>
    [Flags]
    public enum PathMissionSaveMask
    {
        None                = 0x0000,
        Create              = 0x0001,
        Delete              = 0x0002,
        Complete            = 0x0004,
        Progress            = 0x0008,
        State               = 0x0016
    }
}
