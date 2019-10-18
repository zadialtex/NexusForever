using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{

    [Message(GameMessageOpcode.ServerHousingActiveEntitySet)]
    public class ServerHousingActiveEntitySet : IWritable
    {
        public ushort RealmId { get; set; }
        public ulong ResidenceId { get; set; }
        public ulong DecorId { get; set; }
        public uint ActiveUnitId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(RealmId, 14);
            writer.Write(ResidenceId);
            writer.Write(DecorId);
            writer.Write(ActiveUnitId);
        }
    }
}
