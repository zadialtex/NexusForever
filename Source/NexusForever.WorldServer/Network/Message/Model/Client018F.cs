using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    /// <summary>
    /// Sent after client has processed logging in. This packet was appropriate to execute chat channel join amongst other things.
    /// </summary>
    [Message(GameMessageOpcode.Client018F, MessageDirection.Client)]
    public class Client018F : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}
