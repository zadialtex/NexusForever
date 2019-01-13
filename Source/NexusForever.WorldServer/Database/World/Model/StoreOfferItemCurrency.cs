namespace NexusForever.WorldServer.Database.World.Model
{
    public partial class StoreOfferItemCurrency
    {
        public uint OfferId { get; set; }
        public byte CurrencyId { get; set; }
        public uint Field11 { get; set; }
        public byte Field12 { get; set; }
        public uint Field13 { get; set; }
        public double Field14 { get; set; }
        public double Expiry { get; set; }
    }
}
