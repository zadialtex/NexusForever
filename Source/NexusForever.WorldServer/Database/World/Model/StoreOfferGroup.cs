namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferGroup
    {
        public uint Id { get; set; }
        public uint Field1 { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ushort Field2 { get; set; }
        public uint CategoryId1 { get; set; }
        public uint CategoryId2 { get; set; }
        public uint CategoryId3 { get; set; }
        public byte Category1Index { get; set; }
        public byte Category2Index { get; set; }
        public byte Category3Index { get; set; }
        public bool Visible { get; set; }
    }
}
