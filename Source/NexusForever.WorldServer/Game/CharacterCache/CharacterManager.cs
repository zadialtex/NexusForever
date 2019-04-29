using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Game.Entity;
using NLog;
using System;
using System.Collections.Generic;
using CharacterModel = NexusForever.WorldServer.Database.Character.Model.Character;

namespace NexusForever.WorldServer.Game.CharacterCache
{
    public static class CharacterManager
    {
        private static ILogger log { get; } = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<ulong, ICharacter> characters = new Dictionary<ulong, ICharacter>();
        private static readonly Dictionary<string, ulong> characterNameToId = new Dictionary<string, ulong>();

        public static void Initialise()
        {
            BuildCharacterInfoFromDB();
        }

        private static async void BuildCharacterInfoFromDB()
        {
            List<CharacterModel> allCharactersInDb = await CharacterDatabase.GetAllCharactersAsync();
            foreach (CharacterModel character in allCharactersInDb)
                AddPlayer(character.Id, new CharacterInfo(character));

            log.Info($"Stored {characters.Count} characters in Character Cache");
        }

        private static void AddPlayer(ulong characterId, ICharacter character)
        {
            characters.TryAdd(characterId, character);
            characterNameToId.Add(character.Name.ToLower(), characterId);
        }

        public static void RegisterPlayer(Player player)
        {
            if (characters.ContainsKey(player.CharacterId))
                characters[player.CharacterId] = player;
            else
                AddPlayer(player.CharacterId, player);    
        }

        public static void DeregisterPlayer(Player player)
        {
            if (!characters.ContainsKey(player.CharacterId))
                throw new Exception($"{player.CharacterId} should exist in characters dictionary.");

            characters[player.CharacterId] = new CharacterInfo(player);
        }

        public static bool IsCharacter(string name)
        {
            characterNameToId.TryGetValue(name.ToLower(), out ulong value);
            return value > 0;
        }

        public static ICharacter GetCharacterInfo(ulong characterId)
        {
            characters.TryGetValue(characterId, out ICharacter characterInfo);
            return characterInfo;
        }

        public static ICharacter GetCharacterInfo(string name)
        {
            characterNameToId.TryGetValue(name, out ulong characterId);
            if (characterId > 0)
            {
                characters.TryGetValue(characterId, out ICharacter characterInfo);
                return characterInfo;
            }
            
            return null;    
        }
    }
}
