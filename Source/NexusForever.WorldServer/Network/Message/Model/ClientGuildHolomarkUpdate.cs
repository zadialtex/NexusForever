using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGuildHolomarkUpdate)]
    public class ClientGuildHolomarkUpdate : IReadable
    {
        public bool Unknown0 { get; private set; }
        public bool Unknown1 { get; private set; }
        public bool BackDisabled { get; private set; }
        public bool DistanceNear { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Unknown0 = reader.ReadBit();
            Unknown1 = reader.ReadBit();
            BackDisabled = reader.ReadBit();
            DistanceNear = reader.ReadBit();
        }
    }
}
