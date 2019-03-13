using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class MiscHandler
    {
        [MessageHandler(GameMessageOpcode.ClientPing)]
        public static void HandlePing(WorldSession session, ClientPing ping)
        {
            session.Heartbeat.OnHeartbeat();
        }

        [MessageHandler(GameMessageOpcode.ClientReplayLevelRequest)]
        public static void HandleReplayLevel(WorldSession session, ClientReplayLevelRequest request)
        {
            session.Player.PlayLevelUpEffect((byte)request.Level);
        }
    }
}
