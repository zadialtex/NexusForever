using NLog;
using NexusForever.WorldServer.Command.Attributes;
using NexusForever.WorldServer.Command.Contexts;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Command.Handler
{
    [Name("Packet")]
    public class PacketHandler : NamedCommand
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public PacketHandler()
            : base(true, "packet")
        {
        }

        protected override Task HandleCommandAsync(CommandContext context, string command, string[] parameters)
        {
            log.Info($"{parameters.Length}");

            foreach (string packet in parameters)
            {
                string[] split = packet.Split(':');
                context.Session.EnqueueMessageEncrypted((uint)Int32.Parse(split[0]), split[1]);
            }

            return Task.CompletedTask;
        }
    }
}
