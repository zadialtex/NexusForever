using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerPlayerInfoBasicResponse, MessageDirection.Server)]
    public class ServerPlayerInfoBasicResponse : IWritable
    {
        public byte Unk0 { get; set; } = 0;
        public CharacterIdentity CharacterIdentity { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unk0, 3);
            CharacterIdentity.Write(writer);
            writer.WriteStringFixed(Name);
            writer.Write((ushort)Faction, 14);
        }
    }
}
