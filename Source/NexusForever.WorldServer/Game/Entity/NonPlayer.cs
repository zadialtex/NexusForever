using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
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

            
            CalculateProperties();
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new NonPlayerEntityModel
            {
                CreatureId = CreatureId,
                QuestChecklistIdx = 0
            };
        }

        private void CalculateProperties()
        {
            Creature2Entry creatureEntry = GameTableManager.Creature2.GetEntry(CreatureId);

            // TODO: research this some more
            float[] values = new float[200];

            System.Random random = new System.Random();
            ulong level = 1;

            if (creatureEntry != null)
                level = (ulong)random.Next((int)creatureEntry.MinLevel, (int)creatureEntry.MaxLevel);
            else if (GetStatInteger(Stat.Level) > 0)
                level = (ulong)GetStatInteger(Stat.Level);

            CreatureLevelEntry levelEntry = GameTableManager.CreatureLevel.GetEntry(level);
            for (uint i = 0u; i < levelEntry.UnitPropertyValue.Length; i++)
                values[i] = levelEntry.UnitPropertyValue[i];

            if (creatureEntry != null)
            {
                Creature2ArcheTypeEntry archeTypeEntry = GameTableManager.Creature2ArcheType.GetEntry(creatureEntry.Creature2ArcheTypeId);
                if (archeTypeEntry != null)
                    for (uint i = 0u; i < archeTypeEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];

                Creature2DifficultyEntry difficultyEntry = GameTableManager.Creature2Difficulty.GetEntry(creatureEntry.Creature2DifficultyId);
                if (difficultyEntry != null)
                    for (uint i = 0u; i < difficultyEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];

                Creature2TierEntry tierEntry = GameTableManager.Creature2Tier.GetEntry(creatureEntry.Creature2TierId);
                if (tierEntry != null)
                    for (uint i = 0u; i < tierEntry.UnitPropertyMultiplier.Length; i++)
                        values[i] *= archeTypeEntry.UnitPropertyMultiplier[i];
            }

            for (uint i = 0u; i < levelEntry.UnitPropertyValue.Length; i++)
                SetProperty((Property)i, values[i], values[i]);

            if (stats.Count == 0)
            {
                SetStat(Stat.Health, (uint)(GetPropertyValue(Property.BaseHealth) ?? 1000));
                SetStat(Stat.Level, (uint)level);
            }
        }
    }
}
