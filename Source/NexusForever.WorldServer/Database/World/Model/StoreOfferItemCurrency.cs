namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferItemCurrency
    {
        public uint OfferId { get; set; }
        public byte CurrencyId { get; set; }
        public float Price { get; set; }
        public byte Field12 { get; set; }
        public float DiscountPercent { get; set; }
        public double Field14 { get; set; }
        public double Expiry { get; set; }
    }
}
