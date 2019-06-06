using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchaseAccount)]
    public class ClientStorefrontPurchaseAccount : IReadable
    {
        public StorePurchaseRequest StorePurchaseRequest { get; set; } = new StorePurchaseRequest();
        public uint Unknown0 { get; set; }
        public TargetPlayerIdentity TargetPlayerIdentity { get; set; } = new TargetPlayerIdentity();
        public string Message { get; set; }

        public void Read(GamePacketReader reader)
        {
            StorePurchaseRequest.Read(reader);
            Unknown0 = reader.ReadUInt();
            TargetPlayerIdentity.Read(reader);
            Message = reader.ReadWideString();
        }
    }
}
