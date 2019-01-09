using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server096D, MessageDirection.Server)]
    public class Server096D : IWritable
    {
        public class AccountItem : IWritable
        {
            public ulong Unknown0 { get; set; }
            public uint Unknown1 { get; set; }
            public byte Unknown2 { get; set; }
            public bool Unknown3 { get; set; }
            public ushort RealmId { get; set; }
            public ulong CharacterId { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown0);
                writer.Write(Unknown1);
                writer.Write(Unknown1, 5);
                writer.Write(Unknown3);
                writer.Write(RealmId);
                writer.Write(CharacterId);
            }
        }
        
        public List<AccountItem> AccountItemList { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(AccountItemList.Count, 32);
            AccountItemList.ForEach(e => e.Write(writer));
        }
    }
}
