using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NexusForever.Shared.Database.Auth.Model;
using NLog;

namespace NexusForever.Shared.Game
{
    public class ServerInfo: IUpdate
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public Server Model { get; }
        public uint Address { get; }
        public bool IsOnline { get; private set; } = false;

        private double PingDuration = 15d;
        private double pingTimer;

        public ServerInfo(Server model)
        {
            try
            {
                if (!IPAddress.TryParse(model.Host, out IPAddress ipAddress))
                {
                    // find first IPv4 address, client doesn't support IPv6 as address is sent as 4 bytes
                    ipAddress = Dns.GetHostEntry(model.Host)
                        .AddressList
                        .First(a => a.AddressFamily == AddressFamily.InterNetwork);
                }

                Address = (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(ipAddress.GetAddressBytes()));
                Model   = model;

                PingHost(Model.Host, Model.Port);

                pingTimer = PingDuration;
            }
            catch (Exception e)
            {
                log.Fatal(e, $"Failed to load server entry id: {model.Id}, host: {model.Host} from the database!");
                throw;
            }
        }

        public void Update(double lastTick)
        {
            pingTimer -= lastTick;
            
            if(pingTimer <= 0d)
            {
                // check server
                PingHost(Model.Host, Model.Port);

                pingTimer = PingDuration;
            }
        }

        private async void PingHost(string hostIP, int port)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(hostIP, port).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                        IsOnline = false;
                    else
                        IsOnline = true;
                });
            }
        }
    }
}
