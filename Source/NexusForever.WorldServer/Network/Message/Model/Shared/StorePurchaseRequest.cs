using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class StorePurchaseRequest: IReadable
    {
        public uint OfferId { get; set; } // OfferId
        public byte CurrencyId { get; set; }
        public float Price { get; set; }
        public ushort Unknown3 { get; set; }
        public uint CategoryId { get; set; }
        public TargetPlayerIdentity PlayerIdentity { get; set; } = new TargetPlayerIdentity();
        public uint Unknown5 { get; set; }

        public void Read(GamePacketReader reader)
        {
            OfferId = reader.ReadUInt();
            CurrencyId = reader.ReadByte(5u);
            Price = reader.ReadSingle();
            Unknown3 = reader.ReadUShort(14u);
            CategoryId = reader.ReadUInt();
            PlayerIdentity.Read(reader);
            Unknown5 = reader.ReadUInt();
        }
    }
}
