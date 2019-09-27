﻿using NexusForever.WorldServer.Game.Entity.Static;
using System;
using CharacterModel = NexusForever.WorldServer.Database.Character.Model.Character;

namespace NexusForever.WorldServer.Game.CharacterCache
{
    public interface ICharacter
    {
        string Name { get; }
        Sex Sex { get; }
        Race Race { get; }
        Class Class { get; }
        Path Path { get; }
        uint Level { get; }
        Faction Faction { get; }

        float GetOnlineStatus();
    }
}
