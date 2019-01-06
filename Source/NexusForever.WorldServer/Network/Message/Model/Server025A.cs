using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server025A, MessageDirection.Server)]
    public class Server025A : IWritable
    {
        public List<uint> Unknown0 { get; set; } = new List<uint>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unknown0.Count, 32);
            Unknown0.ForEach(e => writer.Write(e));
        }
    }
}
