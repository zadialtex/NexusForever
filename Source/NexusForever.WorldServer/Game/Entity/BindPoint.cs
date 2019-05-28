using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System.Linq;
using EntityModel = NexusForever.WorldServer.Database.World.Model.Entity;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.BindPoint)]
    public class BindPoint : UnitEntity
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public uint CreatureId { get; private set; }
        public Creature2Entry Entry { get; private set; } = null;
        public BindPointEntry BindPointEntry { get; private set; } = null;

        public BindPoint()
            : base(EntityType.BindPoint)
        {
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
            CreatureId          = model.Creature;
            Entry               = GameTableManager.Creature2.GetEntry(CreatureId);
            if (Entry.BindPointId != 0u)
                BindPointEntry      = GameTableManager.BindPoint.GetEntry(Entry.BindPointId);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new BindpointEntityModel
            {
                CreatureId      = CreatureId
            };
        }

        public override ServerEntityCreate BuildCreatePacket()
        {
            ServerEntityCreate bindPointEntity = new ServerEntityCreate
            {
                Guid = Guid,
                Type = Type,
                EntityModel = BuildEntityModel(),
                CreateFlags = (byte)CreateFlags,
                Stats = stats.Values.ToList(),
                Commands = MovementManager.ToList(),
                VisibleItems = itemVisuals.Values.ToList(),
                Properties = Properties.Values.ToList(),
                Faction1 = Faction1,
                Faction2 = Faction2,
                DisplayInfo = 22371, // DisplayInfo,
                OutfitInfo = OutfitInfo,
            };

            // Proof of Concept for map props being "used/controlled" by entities
            if (CreatureId == 13559) // Tremor Ridge, Algoric, Transmat Terminal entity
            {
                bindPointEntity.UnknownB0 = new ServerEntityCreate.UnknownStructureB0
                {
                    Type = 1,
                    ActivePropId = 320934, // 320934 = Tremor Ridge, Algoroc, Transmat Terminal Prop
                    Unknown2 = 0
                };
            }

            return bindPointEntity;
        }

        public override void OnActivateFail(Player activator)
        {
            base.OnActivateFail(activator);
        }

        public override void OnActivateSuccess(Player activator)
        {
            if (BindPointEntry == null)
                log.Error($"BindPoint not found: {Entry.BindPointId}");
            else
            {
                // Set bindpoint
                activator.BindPoint = (ushort)BindPointEntry.Id;

                // Send ServerCharacterBindpoint
                activator.Session.EnqueueMessageEncrypted(new ServerCharacterBindpoint
                {
                    BindpointId = (ushort)Entry.BindPointId
                });

                // Grant Recall spell: "8568, Teleporting to Eldan Stone - Recall Shard - Hearthstone - Global - Tier 1"
                if (activator.SpellManager.GetSpell(7575) == null)
                    activator.SpellManager.AddSpell(7575);
            }

            base.OnActivateSuccess(activator);
        }

        public override void OnActivateCast(Player activator, ClientActivateUnitDeferred request)
        {
            if (BindPointEntry == null)
                log.Error($"BindPoint not found: {Entry.BindPointId}");
            else
            {
                // TODO: Confirm that this bindpoint can be used by the Player's faction

                // TODO: cast "8566, Bind to Transmatter Terminal (Previously Eldan Stone) - DO NOT DELETE - SYSTEM SPELL - Tier 1" by 0x07FD
                // This might also be GameFormula Entry #472?
                activator.PendingClientInteractionEvent = new ClientInteractionEvent(activator, this, request.ClientUniqueId, 8566);
            }
        }
    }
}
