namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferItemData
    {
        public uint Id { get; set; }
        public uint OfferId { get; set; }
        public uint Type { get; set; }
        public ushort ItemId { get; set; }
        public uint Amount { get; set; }
    }
}
