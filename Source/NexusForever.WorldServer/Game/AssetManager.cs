﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Database.World;
using NexusForever.WorldServer.Database.World.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NLog;

namespace NexusForever.WorldServer.Game
{
    public static class AssetManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

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
        private static ImmutableDictionary<ushort, Location> bindPointLocations;
        private static ImmutableDictionary<uint /*item2CategoryId*/, float /*modifier*/> itemArmorModifiers;
        private static ImmutableDictionary<ItemSlot, ImmutableDictionary<Property, float>> innatePropertiesLevelScaling;
        private static ImmutableDictionary<ItemSlot, ImmutableDictionary<Property, float>> innatePropertiesFlat;

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
            CacheItemArmorModifiers();
            CacheItemInnateProperties();
            CacheTutorials();
            CacheBindPointPositions();
        }

        private static void CacheCharacterCustomisations()
        {
            var entries = ImmutableDictionary.CreateBuilder<uint, List<CharacterCustomizationEntry>>();
            foreach (CharacterCustomizationEntry entry in GameTableManager.CharacterCustomization.Entries)
            {
                uint primaryKey;
                if (entry.CharacterCustomizationLabelId00 == 0 && entry.CharacterCustomizationLabelId01 > 0)
                    primaryKey = (entry.Value01 << 24) | (entry.CharacterCustomizationLabelId01 << 16) | (entry.Gender << 8) | entry.RaceId;
                else
                    primaryKey = (entry.Value00 << 24) | (entry.CharacterCustomizationLabelId00 << 16) | (entry.Gender << 8) | entry.RaceId;

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

            List<PropertyBase> classPropertyBases = CharacterDatabase.GetProperties(1);
            foreach (ClassEntry classEntry in classList)
            {
                Class @class = (Class)classEntry.Id;

                if (entries.ContainsKey(@class))
                    continue;

                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                foreach (PropertyBase propertyModel in classPropertyBases)
                {
                    if (propertyModel.Subtype != (uint)@class)
                        continue;

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
            var entries = ImmutableDictionary.CreateBuilder<ItemSlot, List<EquippedItem>>();
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
            var entries = ImmutableDictionary.CreateBuilder<InventoryLocation, uint>();
            foreach (FieldInfo field in typeof(InventoryLocation).GetFields())
            {
                foreach (InventoryLocationAttribute attribute in field.GetCustomAttributes<InventoryLocationAttribute>())
                {
                    InventoryLocation location = (InventoryLocation)field.GetValue(null);
                    entries.Add(location, attribute.DefaultCapacity);
                }
            }

            InventoryLocationCapacities = entries.ToImmutable();
        }

        private static void CacheItemDisplaySourceEntries()
        {
            var entries = ImmutableDictionary.CreateBuilder<uint, List<ItemDisplaySourceEntryEntry>>();
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
        
        private static void CacheBindPointPositions()
        {
            var entries = ImmutableDictionary.CreateBuilder<ushort, Location>();
            foreach(BindPointEntry entry in GameTableManager.BindPoint.Entries)
            {
                ushort entryId = (ushort)entry.Id;
                Creature2Entry creatureEntity = GameTableManager.Creature2.Entries.SingleOrDefault(x => x.BindPointId == entryId);
                if (creatureEntity == null)
                    continue;

                var entityEntry = WorldDatabase.GetEntity(creatureEntity.Id);
                if (entityEntry == null)
                    continue;

                WorldEntry worldEntry = GameTableManager.World.GetEntry(entityEntry.World);
                if (worldEntry == null)
                    continue;

                Location bindPointLocation = new Location(worldEntry, new Vector3(entityEntry.X, entityEntry.Y, entityEntry.Z), new Vector3(entityEntry.Rx, entityEntry.Ry, entityEntry.Rz));

                if (!entries.ContainsKey(entryId))
                    entries.Add(entryId, bindPointLocation);
            }

            bindPointLocations = entries.ToImmutable();
        }

        private static void CacheItemArmorModifiers()
        {
            var armorMods = ImmutableDictionary.CreateBuilder<uint, float>();
            foreach (Item2CategoryEntry entry in GameTableManager.Item2Category.Entries.Where(i => i.Item2FamilyId == 1))
                armorMods.Add(entry.Id, entry.ArmorModifier);

            itemArmorModifiers = armorMods.ToImmutable();
        }

        private static void CacheItemInnateProperties()
        {
            ImmutableDictionary<ItemSlot, ImmutableDictionary<Property, float>>.Builder propFlat = ImmutableDictionary.CreateBuilder<ItemSlot, ImmutableDictionary<Property, float>>();
            ImmutableDictionary<ItemSlot, ImmutableDictionary<Property, float>>.Builder propScaling = ImmutableDictionary.CreateBuilder<ItemSlot, ImmutableDictionary<Property, float>>();

            foreach (var slot in CharacterDatabase.GetProperties(2).GroupBy(x => x.Subtype).Select(i => i.First()))
            {
                ImmutableDictionary<Property, float>.Builder subtypePropFlat = ImmutableDictionary.CreateBuilder<Property, float>();
                ImmutableDictionary<Property, float>.Builder subtypePropScaling = ImmutableDictionary.CreateBuilder<Property, float>();
                foreach (PropertyBase propertyBase in CharacterDatabase.GetProperties(2).Where(i => i.Subtype == slot.Subtype))
                {
                    switch (propertyBase.ModType)
                    {
                        case 0:
                            subtypePropFlat.Add((Property)propertyBase.Property, propertyBase.Value);
                            break;
                        case 1:
                            subtypePropScaling.Add((Property)propertyBase.Property, propertyBase.Value);
                            break;
                    }
                }

                propFlat.Add((ItemSlot)slot.Subtype, subtypePropFlat.ToImmutable());
                propScaling.Add((ItemSlot)slot.Subtype, subtypePropScaling.ToImmutable());
            }

            innatePropertiesFlat = propFlat.ToImmutable();
            innatePropertiesLevelScaling = propScaling.ToImmutable();
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
        /// Returns matching <see cref="CharacterCustomizationEntry"/> given input parameters
        /// </summary>
        public static IEnumerable<CharacterCustomizationEntry> GetCharacterCustomisation(Dictionary<uint, uint> customisations, uint race, uint sex, uint primaryLabel, uint primaryValue)
        {
            log.Info($"Race: {(Race)race}, Sex: {(Sex)sex}, {primaryLabel}, {primaryValue}");

            ImmutableList<CharacterCustomizationEntry> entries = GetPrimaryCharacterCustomisation(race, sex, primaryLabel, primaryValue);
            if (entries == null)
                return Enumerable.Empty<CharacterCustomizationEntry>();

            List<CharacterCustomizationEntry> customizationEntries = new List<CharacterCustomizationEntry>();

            // Customisation has multiple results, filter with a non-zero secondary KvP.
            List<CharacterCustomizationEntry> primaryEntries = entries.Where(e => e.CharacterCustomizationLabelId01 != 0).ToList();
            if (primaryEntries.Count > 0)
            {
                // This will check all entries where there is a primary AND secondary KvP.
                foreach (CharacterCustomizationEntry customizationEntry in primaryEntries)
                {
                    // Missing primary KvP in table, skipping.
                    if (customizationEntry.CharacterCustomizationLabelId00 == 0)
                        continue;

                    // Secondary KvP not found in customisation list, skipping.
                    if (!customisations.ContainsKey(customizationEntry.CharacterCustomizationLabelId01))
                        continue;

                    // Returning match found for primary KvP and secondary KvP
                    if (customisations[customizationEntry.CharacterCustomizationLabelId01] == customizationEntry.Value01)
                        customizationEntries.Add(customizationEntry);
                }

                // Return the matching value when the primary KvP matching the table's secondary KvP
                CharacterCustomizationEntry entry = entries.FirstOrDefault(e => e.CharacterCustomizationLabelId01 == primaryLabel && e.Value01 == primaryValue);
                if (entry != null)
                    customizationEntries.Add(entry);
            }
            else
            {
                // Return the matching value when the primary KvP matches the table's primary KvP, and no secondary KvP is present.
                CharacterCustomizationEntry entry = entries.FirstOrDefault(e => e.CharacterCustomizationLabelId00 == primaryLabel && e.Value00 == primaryValue);
                if (entry != null)
                    customizationEntries.Add(entry);
                else
                {
                    entry = entries.Single(e => e.CharacterCustomizationLabelId01 == 0 && e.Value01 == 0);
                    if (entry != null)
                        customizationEntries.Add(entry);
                }
            }

            return customizationEntries;
        }
        
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
        /// Returns a <see cref="Dictionary{TKey, TValue}"/> containing <see cref="Property"/> and associated values for given Item.
        /// </summary>
        public static Dictionary<Property, float> GetInnateProperties(ItemSlot itemSlot, uint effectiveLevel, uint categoryId, float supportPowerPercentage)
        {
            Dictionary<Property, float> innateProperties = new Dictionary<Property, float>();

            var innatePropScaling = innatePropertiesLevelScaling.ContainsKey(itemSlot) ? innatePropertiesLevelScaling[itemSlot] : new Dictionary<Property, float>().ToImmutableDictionary();
            var innatePropFlat = innatePropertiesFlat.ContainsKey(itemSlot) ? innatePropertiesFlat[itemSlot] : new Dictionary<Property, float>().ToImmutableDictionary();

            // TODO: Shield reboot, max % and tick % are all the same right now. Investigate how these stats are calculated and add to method.
            foreach (KeyValuePair<Property, float> entry in innatePropFlat)
                innateProperties.TryAdd(entry.Key, entry.Value);

            foreach (KeyValuePair<Property, float> entry in innatePropScaling)
            {
                var value = entry.Value;

                if (entry.Key == Property.AssaultRating)
                {
                    if (supportPowerPercentage == 1f)
                        value = 0f;
                    else if (supportPowerPercentage == 0.5f)
                        value *= 0.3333f;
                }

                if (entry.Key == Property.SupportRating)
                {
                    if (supportPowerPercentage == -1f)
                        value = 0f;
                    else if (supportPowerPercentage == -0.5f)
                        value *= 0.3333f;
                }

                // TODO: Ensure correct values after 50 effective level. There are diminishing returns after 50 effective level to Armor.
                if (entry.Key == Property.Armor)
                    if (itemArmorModifiers.TryGetValue(categoryId, out float armorMod))
                        value *= armorMod;

                if (innateProperties.ContainsKey(entry.Key))
                    innateProperties[entry.Key] = innateProperties[entry.Key] + (uint)Math.Floor(value * effectiveLevel);
                else
                    innateProperties.TryAdd(entry.Key, (uint)Math.Floor(value * effectiveLevel));
            }

            return innateProperties;
        }

        /// <summary>
        /// Returns a Tutorial ID if it's found in the Zone Tutorials cache
        /// </summary>
        public static uint GetTutorialIdForZone(uint zoneId)
        {
            return zoneTutorials.TryGetValue(zoneId, out uint tutorialId) ? tutorialId : 0;
        }

        /// <summary>
        /// Returns a <see cref="Location"/> for a <see cref="BindPoint"/>
        /// </summary>
        public static Location GetBindPoint(ushort bindpointId)
        {
            return bindPointLocations.TryGetValue(bindpointId, out Location bindPoint) ? bindPoint : null;
        }
    }
}
