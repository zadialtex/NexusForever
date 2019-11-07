using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NexusForever.Shared.Database.Auth;

namespace NexusForever.Shared.Game
{
    public static class ServerManager
    {
        public static ImmutableList<ServerInfo> Servers { get; private set; }

        public static ImmutableList<ServerMessageInfo> ServerMessages { get; private set; }

        private static double CheckDuration = 60d; // TODO: Make this configurable?
        private static double checkTimer;

        public static void Initialise()
        {
            InitialiseServers();
            InitialiseServerMessages();
        }

        private static void InitialiseServers()
        {
            Servers = AuthDatabase.GetServers()
                .Select(s => new ServerInfo(s))
                .ToImmutableList();
        }

        private static void InitialiseServerMessages()
        {
            ServerMessages = AuthDatabase.GetServerMessages()
                .GroupBy(m => m.Index)
                .Select(g => new ServerMessageInfo(g))
                .ToImmutableList();
        }

        public static void Update(double lastTick)
        {
            checkTimer -= lastTick;
            if (checkTimer <= 0d)
            {
                InitialiseServers();
                InitialiseServerMessages();

                checkTimer = CheckDuration;
            }

            foreach(ServerInfo server in Servers)
                server.Update(lastTick);
        }
    }
}
