using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Database.World;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Game
{
    public static class AssetManager
    {
        public static ImmutableDictionary<InventoryLocation, uint> InventoryLocationCapacities { get; private set; }

        /// <summary>
        /// Id to be assigned to the next created character.
        /// </summary>
        public static ulong NextCharacterId => nextCharacterId++;

        /// <summary>
        /// Id to be assigned to the next created item.
        /// </summary>
        public static ulong NextItemId => nextItemId++;

        /// <summary>
        /// Id to be assigned to the next created mail.
        /// </summary>
        public static ulong NextMailId => nextMailId++;

        private static ulong nextCharacterId;
        private static ulong nextItemId;
        private static ulong nextMailId;

        private static ImmutableDictionary<uint, ImmutableList<CharacterCustomizationEntry>> characterCustomisations;
        private static ImmutableList<PropertyValue> characterBaseProperties;
        private static ImmutableDictionary<Class, ImmutableList<PropertyValue>> characterClassBaseProperties;

        private static ImmutableDictionary<ItemSlot, ImmutableList<EquippedItem>> equippedItems;
        private static ImmutableDictionary<uint, ImmutableList<ItemDisplaySourceEntryEntry>> itemDisplaySourcesEntry;

        private static ImmutableDictionary</*zoneId*/uint, /*tutorialId*/uint> zoneTutorials;

        public static void Initialise()
        {
            nextCharacterId = CharacterDatabase.GetNextCharacterId() + 1ul;
            nextItemId      = CharacterDatabase.GetNextItemId() + 1ul;
            nextMailId      = CharacterDatabase.GetNextMailId() + 1ul;

            CacheCharacterCustomisations();
            CacheCharacterBaseProperties();
            CacheCharacterClassBaseProperties();
            CacheInventoryEquipSlots();
            CacheInventoryBagCapacities();
            CacheItemDisplaySourceEntries();
            CacheTutorials();
        }

        private static void CacheCharacterCustomisations()
        {
            var entries = new Dictionary<uint, List<CharacterCustomizationEntry>>();
            foreach (CharacterCustomizationEntry entry in GameTableManager.CharacterCustomization.Entries)
            {
                uint primaryKey = (entry.Value00 << 24) | (entry.CharacterCustomizationLabelId00 << 16) | (entry.Gender << 8) | entry.RaceId;
                if (!entries.ContainsKey(primaryKey))
                    entries.Add(primaryKey, new List<CharacterCustomizationEntry>());

                entries[primaryKey].Add(entry);
            }

            characterCustomisations = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private static void CacheCharacterBaseProperties()
        {
            var entries = ImmutableList.CreateBuilder<PropertyValue>();
            foreach (PropertyBase propertyModel in CharacterDatabase.GetProperties(0))
            {
                var newPropValue = new PropertyValue((Property)propertyModel.Property, propertyModel.Value, propertyModel.Value);
                entries.Add(newPropValue);
            }

            characterBaseProperties = entries.ToImmutable();
        }

        private static void CacheCharacterClassBaseProperties()
        {
            ImmutableDictionary<Class, ImmutableList<PropertyValue>>.Builder entries = ImmutableDictionary.CreateBuilder<Class, ImmutableList<PropertyValue>>();
            var classList = GameTableManager.Class.Entries;

            foreach(ClassEntry classEntry in classList)
            {
                Class @class = (Class)classEntry.Id;

                if (entries.ContainsKey(@class))
                    continue;

                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                foreach (PropertyBase propertyModel in CharacterDatabase.GetProperties((uint)@class))
                {
                    var newPropValue = new PropertyValue((Property)propertyModel.Property, propertyModel.Value, propertyModel.Value);
                    propertyList.Add(newPropValue);
                }
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();

                entries.Add(@class, classProperties);
            }
            
            characterClassBaseProperties = entries.ToImmutable();
        }

        private static void CacheInventoryEquipSlots()
        {
            var entries = new Dictionary<ItemSlot, List<EquippedItem>>();
            foreach (FieldInfo field in typeof(ItemSlot).GetFields())
            {
                foreach (EquippedItemAttribute attribute in field.GetCustomAttributes<EquippedItemAttribute>())
                {
                    ItemSlot slot = (ItemSlot)field.GetValue(null);
                    if (!entries.ContainsKey(slot))
                        entries.Add(slot, new List<EquippedItem>());

                    entries[slot].Add(attribute.Slot);
                }
            }
            equippedItems = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        public static void CacheInventoryBagCapacities()
        {
            var entries = new Dictionary<InventoryLocation, uint>();
            foreach (FieldInfo field in typeof(InventoryLocation).GetFields())
            {
                foreach (InventoryLocationAttribute attribute in field.GetCustomAttributes<InventoryLocationAttribute>())
                {
                    InventoryLocation location = (InventoryLocation)field.GetValue(null);
                    entries.Add(location, attribute.DefaultCapacity);
                }
            }

            InventoryLocationCapacities = entries.ToImmutableDictionary();
        }

        private static void CacheItemDisplaySourceEntries()
        {
            var entries = new Dictionary<uint, List<ItemDisplaySourceEntryEntry>>();
            foreach (ItemDisplaySourceEntryEntry entry in GameTableManager.ItemDisplaySourceEntry.Entries)
            {
                if (!entries.ContainsKey(entry.ItemSourceId))
                    entries.Add(entry.ItemSourceId, new List<ItemDisplaySourceEntryEntry>());

                entries[entry.ItemSourceId].Add(entry);
            }

            itemDisplaySourcesEntry = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private static void CacheTutorials()
        {
            var zoneEntries =  ImmutableDictionary.CreateBuilder<uint, uint>();
            foreach (Tutorial tutorial in WorldDatabase.GetTutorialTriggers())
            {
                if (tutorial.TriggerId == 0) // Don't add Tutorials with no trigger ID
                    continue;

                if (tutorial.Type == 29 && !zoneEntries.ContainsKey(tutorial.TriggerId))
                    zoneEntries.Add(tutorial.TriggerId, tutorial.Id);
            }

            zoneTutorials = zoneEntries.ToImmutable();
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="CharacterCustomizationEntry"/>'s for the supplied race, sex, label and value.
        /// </summary>
        public static ImmutableList<CharacterCustomizationEntry> GetPrimaryCharacterCustomisation(uint race, uint sex, uint label, uint value)
        {
            uint key = (value << 24) | (label << 16) | (sex << 8) | race;
            return characterCustomisations.TryGetValue(key, out ImmutableList<CharacterCustomizationEntry> entries) ? entries : null;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList[T]"/> containing all base <see cref="PropertyValue"/> for any character
        /// </summary>
        public static ImmutableList<PropertyValue> GetCharacterBaseProperties()
        {
            return characterBaseProperties;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList[T]"/> containing all base <see cref="PropertyValue"/> for a character class
        /// </summary>
        public static ImmutableList<PropertyValue> GetCharacterClassBaseProperties(Class @class)
        {
            return characterClassBaseProperties.TryGetValue(@class, out ImmutableList<PropertyValue> propertyValues) ? propertyValues : null;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="EquippedItem"/>'s for supplied <see cref="ItemSlot"/>.
        /// </summary>
        public static ImmutableList<EquippedItem> GetEquippedBagIndexes(ItemSlot slot)
        {
            return equippedItems.TryGetValue(slot, out ImmutableList<EquippedItem> entries) ? entries : null;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="ItemDisplaySourceEntryEntry"/>'s for the supplied itemSource.
        /// </summary>
        public static ImmutableList<ItemDisplaySourceEntryEntry> GetItemDisplaySource(uint itemSource)
        {
            return itemDisplaySourcesEntry.TryGetValue(itemSource, out ImmutableList<ItemDisplaySourceEntryEntry> entries) ? entries : null;
        }

        /// <summary>
        /// Returns a Tutorial ID if it's found in the Zone Tutorials cache
        /// </summary>
        public static uint GetTutorialIdForZone(uint zoneId)
        {
            return zoneTutorials.TryGetValue(zoneId, out uint tutorialId) ? tutorialId : 0;
        }
    }
}
