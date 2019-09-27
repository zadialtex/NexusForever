﻿using System;

namespace NexusForever.WorldServer.Game.Entity.Static
{
    /// <summary>
    /// Determines which fields need saving for <see cref="Player"/> when being saved to the database.
    /// </summary>
    [Flags]
    public enum PlayerSaveMask
    {
        None        = 0x0000,
        Location    = 0x0001,
        Path        = 0x0002,
        Costume     = 0x0004,
        InputKeySet = 0x0008,
        Xp          = 0x0010,
        Affiliation = 0x0020,
        Holomark    = 0x0040,
        Innate      = 0x0080,
        BindPoint   = 0x0100,
        Appearance  = 0x0200,
    }
}
