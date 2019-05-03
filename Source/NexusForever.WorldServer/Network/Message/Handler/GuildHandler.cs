using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Guild;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GuildHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGuildRegister)]
        public static void HandleGuildRegister(WorldSession session, ClientGuildRegister request)
        {
            GuildManager.RegisterGuild(session, request);

            // Do guild holomark visual
            var itemVisualUpdate = new ServerItemVisualUpdate
            {
                Guid = session.Player.Guid
            };

            foreach (ItemVisual itemVisual in session.Player.GetAppearance())
                itemVisualUpdate.ItemVisuals.Add(itemVisual);

            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardScanLines,
                DisplayId = 2961 // request.GuildHolomark.HolomarkPart1.GuildStandardPartId
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardBackgroundIcon,
                DisplayId = 5506 // request.GuildHolomark.HolomarkPart1.GuildStandardPartId
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardForegroundIcon,
                DisplayId = 5432 // request.GuildHolomark.HolomarkPart1.GuildStandardPartId
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardChest,
                DisplayId = 5411 // request.GuildHolomark.HolomarkPart1.GuildStandardPartId
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardBack,
                DisplayId = 7163
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardShoulderL,
                DisplayId = 7164
            });
            itemVisualUpdate.ItemVisuals.Add(new ItemVisual
            {
                Slot = Game.Entity.Static.ItemSlot.GuildStandardShoulderR,
                DisplayId = 7165
            });
            // 7163 - Near Back
            // 7164 - Near Left
            // 7165 - Near Right
            // 5580 - Far Back
            // 5581 - Far Left
            // 5582 - Far Right

            if (!session.Player.IsLoading)
                session.Player.EnqueueToVisible(itemVisualUpdate, true);
        }

        [MessageHandler(GameMessageOpcode.ClientGuildHolomarkUpdate)]
        public static void HandleHolomarkUpdate(WorldSession session, ClientGuildHolomarkUpdate clientGuildHolomarkUpdate)
        {
            // TODO: Need to figure out which packet tells the client the player's holomark preferences at login / map change.

            //log.Info($"{clientGuildHolomarkUpdate.Unknown0}, {clientGuildHolomarkUpdate.Unknown1}, {clientGuildHolomarkUpdate.BackDisabled}, {clientGuildHolomarkUpdate.DistanceNear}");
        }

        [MessageHandler(GameMessageOpcode.ClientGuildOperation)]
        public static void HandleOperation(WorldSession session, ClientGuildOperation clientGuildOperation)
        {
            GuildManager.HandleGuildOperation(session, clientGuildOperation);
        }

        [MessageHandler(GameMessageOpcode.ClientGuildInviteResponse)]
        public static void HandeInviteResponse(WorldSession session, ClientGuildInviteResponse clientGuildInviteResponse)
        {
            if (session.Player.PendingGuildInvite == null)
                return;

            if (clientGuildInviteResponse.Accepted)
                GuildManager.JoinGuild(session, session.Player.PendingGuildInvite);
            else
            {
                var targetSession = NetworkManager<WorldSession>.GetSession(i => i.Player?.CharacterId == session.Player.PendingGuildInvite.InviteeId);
                if (targetSession != null)
                    targetSession.EnqueueMessageEncrypted(new ServerGuildResult
                    {
                        RealmId = WorldServer.RealmId,
                        GuildId = session.Player.PendingGuildInvite.GuildId,
                        Result = GuildResult.InviteDeclined,
                        ReferenceText = session.Player.Name
                    });
            }

            session.Player.PendingGuildInvite = null;
        }
    }
}
