using NexusForever.Shared.GameTable.Model;
using WorldEntityModel = NexusForever.WorldServer.Database.World.Model.Entity;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.Plug)]
    public class Plug : WorldEntity
    {
        public HousingPlotInfoEntry PlotEntry { get; }
        public HousingPlugItemEntry PlugEntry { get; }

        public ushort PlugId { get; private set; }

        public Plug() 
            : base (EntityType.Plug)
        {
        }

        public Plug(HousingPlotInfoEntry plotEntry, HousingPlugItemEntry plugEntry)
            : base(EntityType.Plug)
        {
            PlotEntry = plotEntry;
            PlugEntry = plugEntry;
        }

        public override void Initialise(WorldEntityModel model)
        {
            PlugId = (ushort)model.Creature;

            base.Initialise(model);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PlugModel
            {
                SocketId  = (ushort)(PlotEntry?.WorldSocketId ?? WorldSocketId),
                PlugId    = (ushort)(PlugEntry?.WorldIdPlug00 ?? PlugId),
                PlugFlags = 63
            };
        }
    }
}
