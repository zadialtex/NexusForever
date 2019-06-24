﻿using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientHousingPlugUpdate)]
    public class ClientHousingPlugUpdate : IReadable
    {
        public TargetPlayerIdentity TargetPlayerIdentity { get; } = new TargetPlayerIdentity();

        public void Read(GamePacketReader reader)
        {
            TargetPlayerIdentity.Read(reader); // PlotInfo + ResidenceId

            reader.ReadUInt(); // PlugItem
            reader.ReadUInt(); // PlugFacing?
            reader.ReadUInt(); // PlotFlags?
            reader.ReadUInt(); 
            reader.ReadByte(3u); // Operation

            // HousingContribution related, client function that sends this looks up values from HousingContributionInfo.tbl
            reader.ReadBytes(5 * 20);
        }
    }
}
