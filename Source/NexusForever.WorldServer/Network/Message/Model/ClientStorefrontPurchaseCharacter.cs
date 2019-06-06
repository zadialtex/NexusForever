using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchaseCharacter)]
    public class ClientStorefrontPurchaseCharacter : IReadable
    {
        public StorePurchaseRequest StorePurchaseRequest { get; set; } = new StorePurchaseRequest();

        public void Read(GamePacketReader reader)
        {
            StorePurchaseRequest.Read(reader);
        }
    }
}