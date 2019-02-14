using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class CharacterIdentity : IWritable, IReadable
    {
        public ushort RealmId { get; set; }
        public ulong CharacterId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(RealmId, 14u);
            writer.Write(CharacterId);
        }

        public void Read(GamePacketReader reader)
        {
            RealmId = reader.ReadUShort(14u);
            CharacterId = reader.ReadULong();
        }
    }
}
