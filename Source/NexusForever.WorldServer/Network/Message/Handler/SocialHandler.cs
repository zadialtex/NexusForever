using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Command;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class SocialHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly string CommandPrefix = "!";

        [MessageHandler(GameMessageOpcode.ClientChat)]
        public static void HandleChat(WorldSession session, ClientChat chat)
        {
            if (chat.Message.StartsWith(CommandPrefix))
            {
                try
                {
                    CommandManager.HandleCommand(session, chat.Message, true);
                    //CommandManager.ParseCommand(chat.Message, out string command, out string[] parameters);
                    //CommandHandlerDelegate handler = CommandManager.GetCommandHandler(command);
                    //handler?.Invoke(session, parameters);
                }
                catch (Exception e)
                {
                    log.Warn(e.Message);
                }
            }
            else
                SocialManager.HandleClientChat(session, chat);
        }

        [MessageHandler(GameMessageOpcode.ClientEmote)]
        public static void HandleEmote(WorldSession session, ClientEmote emote)
        {
            uint emoteId = emote.EmoteId;
            uint standState = 0;
            if (emoteId != 0)
            {
                EmotesEntry entry = GameTableManager.Emotes.GetEntry(emoteId);
                if (entry == null)
                    throw (new InvalidPacketValueException("HandleEmote: Invalid EmoteId"));

                standState = entry.StandState;
            }
            session.Player.EnqueueToVisible(new ServerEmote
            {
                Guid = session.Player.Guid,
                StandState = standState,
                EmoteId = emoteId
            });
        }

        [MessageHandler(GameMessageOpcode.ClientWhoRequest)]
        public static void HandleWhoRequest(WorldSession session, ClientWhoRequest request)
        {
            List<WorldSession> allSessions = NetworkManager<WorldSession>.GetAllSessions().ToList();
            List<ServerWhoResponse.WhoPlayer> players = new List<ServerWhoResponse.WhoPlayer>();

            foreach (WorldSession sessionEntry in allSessions)
                if(sessionEntry.Player != null)
                    players.Add(new ServerWhoResponse.WhoPlayer
                    {
                        Name = sessionEntry.Player.Name,
                        Level = sessionEntry.Player.Level,
                        Race = sessionEntry.Player.Race,
                        Class = sessionEntry.Player.Class,
                        Path = sessionEntry.Player.Path,
                        Faction = sessionEntry.Player.Faction1,
                        Sex = sessionEntry.Player.Sex,
                        Zone = sessionEntry.Player.Zone.Id
                    });

            session.EnqueueMessageEncrypted(new ServerWhoResponse
            {
                Players = players
            });
        }

        [MessageHandler(GameMessageOpcode.ClientChatWhisper)]
        public static void HandleWhisper(WorldSession session, ClientChatWhisper whisper)
        {
            SocialManager.HandleWhisperChat(session, whisper);
        }

        [MessageHandler(GameMessageOpcode.Client018F)]
        public static void HandleChatJoin(WorldSession session, Client018F request)
        {
            SocialManager.JoinChatChannels(session);
            SocialManager.SendMessage(session, "MOTD: Welcome to Kirmmin's Test Server! !character is a new command - you can add XP and level up (without relogging!). !currency is a new command, /who works, and we now have chat channels & whispers! (Updated 14 Mar 2019)", "", ChatChannel.Realm);
        }
    }
}
