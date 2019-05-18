﻿using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Database.Character.Model
{
    public partial class GuildData
    {
        public ulong Id { get; set; }
        public uint Taxes { get; set; }
        public string MessageOfTheDay { get; set; }
        public string AdditionalInfo { get; set; }
        public ushort BackgroundIconPartId { get; set; }
        public ushort ForegroundIconPartId { get; set; }
        public ushort ScanLinesPartId { get; set; }

        public virtual Guild IdNavigation { get; set; }
    }
}
