namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferItem
    {
        public uint Id { get; set; }
        public uint GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint DisplayFlags { get; set; }
        public double Field6 { get; set; }
        public byte Field7 { get; set; }
        public bool Visible { get; set; }
    }
}
