using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Contact.Static;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class ContactData : IWritable
    {
        public ulong ContactId { get; set; }
        public CharacterIdentity IdentityData { get; set; } = new CharacterIdentity();
        public string Note { get; set; } = "";
        public ContactType Type { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ContactId);
            IdentityData.Write(writer);
            writer.WriteStringWide(Note);
            writer.Write(Type, 4u);
        }
    }
}
