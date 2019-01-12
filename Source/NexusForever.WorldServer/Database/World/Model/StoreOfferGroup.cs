using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferGroup
    {
        public StoreOfferGroup()
        {
            StoreOfferItem = new HashSet<StoreOfferItem>();
            StoreOfferGroupCategory = new HashSet<StoreOfferGroupCategory>();
        }

        public uint Id { get; set; }
        public uint DisplayFlags { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ushort Field2 { get; set; }
        public bool Visible { get; set; }

        public virtual ICollection<StoreOfferItem> StoreOfferItem { get; set; }
        public virtual ICollection<StoreOfferGroupCategory> StoreOfferGroupCategory{ get; set; }
    }
}
