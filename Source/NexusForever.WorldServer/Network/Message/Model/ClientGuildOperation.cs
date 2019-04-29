using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGuildOperation)]
    public class ClientGuildOperation : IReadable
    {
        public ushort RealmId { get; private set; }
        public ulong GuildId { get; private set; }
        public uint Id { get; private set; }
        public ulong Value { get; private set; }
        public string TextValue { get; private set; }
        public GuildOperation Operation { get; private set; }

        public void Read(GamePacketReader reader)
        {
            RealmId = reader.ReadUShort(14u);
            GuildId = reader.ReadULong();
            Id = reader.ReadUInt();
            Value = reader.ReadULong();
            TextValue = reader.ReadWideString();
            Operation = reader.ReadEnum<GuildOperation>(6u);
        }
    }
}
