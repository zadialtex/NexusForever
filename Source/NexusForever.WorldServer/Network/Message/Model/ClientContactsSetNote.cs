using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientContactsSetNote, MessageDirection.Client)]
    public class ClientContactsSetNote : IReadable
    {
        public CharacterIdentity CharacterIdentity { get; private set; } = new CharacterIdentity();
        public string Note { get; private set; }

        public void Read(GamePacketReader reader)
        {
            CharacterIdentity.Read(reader);
            Note = reader.ReadWideString();
        }
    }
}
