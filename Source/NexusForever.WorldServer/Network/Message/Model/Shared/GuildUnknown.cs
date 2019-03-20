using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class GuildUnknown: IWritable
    {
        public ulong Unknown0 { get; set; }
        public byte[] Unknown1 { get; set; } = new byte[40];
        public ulong Unknown2 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Unknown0);
            writer.WriteBytes(Unknown1);
            writer.Write(Unknown2);
        }
    }
}
