using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientMovementDash)]
    public class ClientMovementDash : IReadable
    {
        public byte Unknown0 { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Unknown0 = reader.ReadByte(3u);
        }
    }
}
