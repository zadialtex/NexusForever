﻿using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NexusForever.WorldServer.Game.Guild
{
    public static partial class GuildManager
    {
        [GuildOperationHandler(GuildOperation.AdditionalInfo)]
        private static void GuildOperationAdditionalInfo(WorldSession session, ClientGuildOperation operation, GuildBase guildBase)
        {
            if (guildBase.Type != GuildType.Guild) // TODO: Add support, if necessary, to other guild types.
                return;

            Guild guild = (Guild)guildBase;
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if (memberRank.Index > 0)
                result = GuildResult.RankLacksSufficientPermissions;

            if (result == GuildResult.Success)
            {
                guild.AdditionalInfo = operation.TextValue;
                guild.SendToOnlineUsers(new ServerGuildInfoMessage
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    AdditionalInfo = guild.AdditionalInfo
                });
            }
            else
                SendGuildResult(session, result, guild);
        }

        [GuildOperationHandler(GuildOperation.MessageOfTheDay)]
        private static void GuildOperationMessageOfTheDay(WorldSession session, ClientGuildOperation operation, GuildBase guildBase)
        {
            if (guildBase.Type != GuildType.Guild) // TODO: Add support, if necessary, to other guild types.
                return;

            Guild guild = (Guild)guildBase;
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.MessageOfTheDay) == 0)
                result = GuildResult.RankLacksSufficientPermissions;

            if (result == GuildResult.Success)
            {
                guild.MessageOfTheDay = operation.TextValue;
                guild.SendToOnlineUsers(new ServerGuildMotdUpdate
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    MessageOfTheDay = guild.MessageOfTheDay
                });
            }
            else
                SendGuildResult(session, result, guild);
        }

        [GuildOperationHandler(GuildOperation.Flags)]
        private static void GuildOperationTaxUpdate(WorldSession session, ClientGuildOperation operation, GuildBase guildBase)
        {
            if (guildBase.Type != GuildType.Guild) // TODO: Add support, if necessary, to other guild types.
                return;

            Guild guild = (Guild)guildBase;
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if (memberRank.Index > 0)
                result = GuildResult.RankLacksSufficientPermissions;

            if (result == GuildResult.Success)
            {
                guild.SetTaxes(Convert.ToBoolean(operation.Data));
                guild.SendToOnlineUsers(new ServerGuildTaxUpdate
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Value = guild.Flags
                });
            }
            else
                SendGuildResult(session, result, guild);
        }
    }
}
