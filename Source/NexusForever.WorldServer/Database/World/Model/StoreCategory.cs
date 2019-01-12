namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreCategory
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint ParentCategoryId { get; set; }
        public uint Index { get; set; }
        public bool Visible { get; set; }
    }
}
