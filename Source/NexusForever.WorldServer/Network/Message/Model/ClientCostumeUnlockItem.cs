using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientCostumeItemUnlock, MessageDirection.Client)]
    public class ClientCostumeUnlockItem : IReadable
    {
        public uint Unknown0 { get; set; }
        public uint Unknown1 { get; set; }

        public void Read(GamePacketReader reader)
        {
            Unknown0 = reader.ReadUInt(9);
            Unknown1 = reader.ReadUInt(20);
        }
    }
}
