using NexusForever.WorldServer.Game.Entity.Static;
using System;
using CharacterModel = NexusForever.WorldServer.Database.Character.Model.Character;

namespace NexusForever.WorldServer.Game.CharacterCache
{
    public class CharacterInfo : ICharacter
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Path Path { get; set; }
        public uint Level { get; set; }
        public Faction Faction { get; set; }
        public DateTime LastOnline { get; set; } = DateTime.Now;

        public CharacterInfo() { }

        public CharacterInfo(CharacterModel model)
        {
            Name = model.Name;
            Sex = (Sex)model.Sex;
            Race = (Race)model.Race;
            Class = (Class)model.Class;
            Path = (Path)model.ActivePath;
            Level = model.Level;
            Faction = (Faction)model.FactionId;
            LastOnline = model.LastOnline;
        }

        public CharacterInfo(ICharacter model)
        {
            Name = model.Name;
            Sex = model.Sex;
            Race = model.Race;
            Class = model.Class;
            Path = model.Path;
            Level = model.Level;
            Faction = model.Faction;
        }

        public float GetOnlineStatus()
        {
            return (float)DateTime.Now.Subtract(LastOnline).TotalDays * -1f;
        }
    }
}
