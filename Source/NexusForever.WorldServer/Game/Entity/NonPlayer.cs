using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.CSI;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Spell;
using EntityModel = NexusForever.WorldServer.Database.World.Model.Entity;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.NonPlayer)]
    public class NonPlayer : UnitEntity
    {
        public VendorInfo VendorInfo { get; private set; }

        public NonPlayer()
            : base(EntityType.NonPlayer)
        {
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);

            if (model.EntityVendor != null)
            {
                CreateFlags |= EntityCreateFlag.Vendor;
                VendorInfo = new VendorInfo(model);
            }

            if (model.EntityStats.Count == 0)
                CalculateProperties();

            //if (MaxHealth == 0)
            //    MaxHealth = 1000u;

            if(Health == 0u)
                Health = 1000u;
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new NonPlayerEntityModel
            {
                CreatureId = CreatureId,
                QuestChecklistIdx = 0
            };
        }

        public override void OnActivateCast(Player activator, uint interactionId)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);

            // TODO: Handle casting activate spells at correct times. Additionally, ensure Prerequisites are met to cast.
            // Creature2Entry can contain up to 4 spells to activate and prerequisite spells to trigger.
            uint spell4Id = 116;
            if (entry.Spell4IdActivate00 > 0)
                spell4Id = entry.Spell4IdActivate00;

            SpellParameters parameters = new SpellParameters
            {
                PrimaryTargetId = Guid,
                ClientSideInteraction = new ClientSideInteraction(activator, this, interactionId),
                CastTimeOverride = entry.ActivateSpellCastTime,
            };
            activator.CastSpell(spell4Id, parameters);
        }

        private void CalculateProperties()
        {
            Creature2Entry creatureEntry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);

            // TODO: research this some more
            float[] values = new float[200];

            System.Random random = new System.Random();
            ulong level = 1;

            if (creatureEntry != null)
                level = (ulong)random.Next((int)creatureEntry.MinLevel, (int)creatureEntry.MaxLevel);
            else if (GetStatInteger(Stat.Level) > 0)
                level = (ulong)GetStatInteger(Stat.Level);

            CreatureLevelEntry levelEntry = GameTableManager.Instance.CreatureLevel.GetEntry(level);
            for (uint i = 0u; i < levelEntry.UnitPropertyValue.Length; i++)
                values[i] = levelEntry.UnitPropertyValue[i];

            if (creatureEntry != null)
            {
                Creature2ArcheTypeEntry archeTypeEntry = GameTableManager.Instance.Creature2ArcheType.GetEntry(creatureEntry.Creature2ArcheTypeId);
                if (archeTypeEntry != null)
                    for (uint i = 0u; i < archeTypeEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];

                Creature2DifficultyEntry difficultyEntry = GameTableManager.Instance.Creature2Difficulty.GetEntry(creatureEntry.Creature2DifficultyId);
                if (difficultyEntry != null)
                    for (uint i = 0u; i < difficultyEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];

                Creature2TierEntry tierEntry = GameTableManager.Instance.Creature2Tier.GetEntry(creatureEntry.Creature2TierId);
                if (tierEntry != null)
                    for (uint i = 0u; i < tierEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];
            }

            for (uint i = 0u; i < levelEntry.UnitPropertyValue.Length; i++)
                SetBaseProperty((Property)i, values[i]);

            Level = (uint)level;
        }
    }
}
