using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerContactsUpdateStatus, MessageDirection.Server)]
    public class ServerContactsUpdateStatus : IWritable
    {
        public CharacterIdentity CharacterIdentity { get; set; }
        public float LastOnlineInDays { get; set; }

        public void Write(GamePacketWriter writer)
        {
            CharacterIdentity.Write(writer);
            writer.Write(LastOnlineInDays);
        }
    }
}
