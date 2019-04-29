using NexusForever.Shared.Network;
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
        private static void GuildOperationAdditionalInfo(WorldSession session, ClientGuildOperation operation)
        {
            var guild = (Guild)guilds[operation.GuildId];
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

        [GuildOperationHandler(GuildOperation.InitGuildWindow)]
        private static void GuildOperationInitGuildWindow(WorldSession session, ClientGuildOperation operation)
        {
            // Probably want to send roster update
        }

        [GuildOperationHandler(GuildOperation.MemberInvite)]
        private static void GuildOperationMemberInvite(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.Invite) == 0)
                result = GuildResult.RankLacksSufficientPermissions;
            else if (!CharacterManager.IsCharacter(operation.TextValue))
                result = GuildResult.UnknownCharacter;
            else if (guild.GetGuildMembersPackets().Count() >= maxGuildSize[guild.Type])
                result = GuildResult.CannotInviteGuildFull;

            //var targetPlayer = NetworkManager<WorldSession>.GetSession(i => i.Player?.CharacterId = )
            var targetPlayer = (Player)CharacterManager.GetCharacterInfo(operation.TextValue);
            if (targetPlayer.Session == null)
                result = GuildResult.UnknownCharacter;
            else if (targetPlayer.PendingGuildInvite != null)
                result = GuildResult.CharacterAlreadyHasAGuildInvite;

            if (result == GuildResult.Success)
            {
                targetPlayer.PendingGuildInvite = new GuildInvite
                {
                    GuildId = guild.Id,
                    InviteeId = session.Player.CharacterId
                };
                SendPendingInvite(targetPlayer.Session);
                SendGuildResult(session, GuildResult.CharacterInvited, guild, referenceText: targetPlayer.Name);
            }
            else
                SendGuildResult(session, result, guild, referenceText: operation.TextValue);
        }

        [GuildOperationHandler(GuildOperation.MemberRemove)]
        private static void GuildOperationMemberRemove(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;
            var targetMember = guild.GetMember(operation.TextValue);

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.Kick) == 0)
                result = GuildResult.RankLacksSufficientPermissions;
            else if (targetMember == null)
                result = GuildResult.CharacterNotInYourGuild;
            else if (memberRank.Index >= targetMember.Rank.Index)
                result = GuildResult.CannotKickHigherOrEqualRankedMember;

            if (result == GuildResult.Success)
            {
                guild.RemoveMember(targetMember.CharacterId, out WorldSession memberSession);
                guild.SendToOnlineUsers(new ServerGuildMemberRemove
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    PlayerIdentity = new TargetPlayerIdentity
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = targetMember.CharacterId
                    },
                });
                guild.AnnounceGuildResult(GuildResult.KickedMember, referenceText: CharacterManager.GetCharacterInfo(targetMember.CharacterId).Name);

                if (memberSession != null)
                {
                    SendGuildResult(memberSession, GuildResult.KickedYou, referenceText: session.Player.Name);
                    memberSession.Player.GuildMemberships.Remove(guild.Id);
                    memberSession.EnqueueMessageEncrypted(new ServerGuildRemove
                    {
                        RealmId = WorldServer.RealmId,
                        GuildId = guild.Id
                    });
                }
                    
            }
            else
                SendGuildResult(session, result, guild, referenceText: operation.TextValue);
        }

        [GuildOperationHandler(GuildOperation.MessageOfTheDay)]
        private static void GuildOperationMessageOfTheDay(WorldSession session, ClientGuildOperation operation)
        {
            var guild = (Guild)guilds[operation.GuildId];
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

        [GuildOperationHandler(GuildOperation.RankAdd)]
        private static void GuildOperationRankAdd(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.CreateAndRemoveRank) == 0)
                result = GuildResult.RankLacksSufficientPermissions;
            else if (guild.GetRank((byte)operation.Id) != null)
                result = GuildResult.InvalidRank;
            else if (guild.RankExists(operation.TextValue))
                result = GuildResult.DuplicateRankName;
            else if (Regex.IsMatch(operation.TextValue, @"[^A-Za-z0-9\s]")) // Ensure only Alphanumeric characters are used
                result = GuildResult.InvalidRankName;

            if (result == GuildResult.Success)
            {
                guild.AddRank(new Rank(operation.TextValue, guild.Id, (byte)operation.Id, (GuildRankPermission)operation.Value, 0, 0, 0));
                guild.SendToOnlineUsers(new ServerGuildRankChange
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Ranks = guild.GetGuildRanksPackets().ToList()
                });
                guild.AnnounceGuildResult(GuildResult.RankCreated, operation.Id, operation.TextValue);
            }
            else
                SendGuildResult(session, result, guild, operation.Id, operation.TextValue);
        }

        [GuildOperationHandler(GuildOperation.RankDelete)]
        private static void GuildOperationRankDelete(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.CreateAndRemoveRank) == 0)
                result = GuildResult.RankLacksSufficientPermissions;
            else if (guild.GetRank((byte)operation.Id) == null)
                result = GuildResult.InvalidRank;
            else if (memberRank.Index >= operation.Id)
                result = GuildResult.CanOnlyModifyRanksBelowYours;
            else if (operation.Id < 2 || operation.Id > 8)
                result = GuildResult.CannotDeleteDefaultRanks;
            else if (guild.GetMembersOfRank((byte)operation.Id).Count() > 0)
                result = GuildResult.CanOnlyDeleteEmptyRanks;

            if (result == GuildResult.Success)
            {
                string rankName = guild.GetRank((byte)operation.Id).Name;
                guild.RemoveRank((byte)operation.Id);
                guild.AnnounceGuildResult(GuildResult.RankDeleted, operation.Id, rankName);
                guild.SendToOnlineUsers(new ServerGuildRankChange
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Ranks = guild.GetGuildRanksPackets().ToList()
                });
            }
            else
                SendGuildResult(session, result, guild, operation.Id, operation.TextValue);
        }

        [GuildOperationHandler(GuildOperation.RankRename)]
        private static void GuildOperationRankRename(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.RenameRank) == 0)
                result = GuildResult.RankLacksRankRenamePermission;
            else if (guild.GetRank((byte)operation.Id) == null)
                result = GuildResult.InvalidRank;
            else if (memberRank.Index >= operation.Id)
                result = GuildResult.CanOnlyModifyRanksBelowYours;
            else if (guild.RankExists(operation.TextValue))
                result = GuildResult.DuplicateRankName;
            else if (Regex.IsMatch(operation.TextValue, @"[^A-Za-z0-9\s]")) // Ensure only Alphanumeric characters are used
                result = GuildResult.InvalidRankName;

            if (result == GuildResult.Success)
            {
                guild.GetRank((byte)operation.Id).Rename(operation.TextValue);
                guild.SendToOnlineUsers(new ServerGuildRankChange
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Ranks = guild.GetGuildRanksPackets().ToList()
                });
                guild.AnnounceGuildResult(GuildResult.RankRenamed, operation.Id, operation.TextValue);
            }
            else
                SendGuildResult(session, result, guild, operation.Id, operation.TextValue);
        }

        [GuildOperationHandler(GuildOperation.RankPermissions)]
        private static void GuildOperationRankPermissions(WorldSession session, ClientGuildOperation operation)
        {
            var guild = guilds[operation.GuildId];
            Rank rankToModify = guild.GetRank((byte)operation.Id);
            Rank memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if ((memberRank.GuildPermission & GuildRankPermission.EditLowerRankPermissions) == 0)
                result = GuildResult.RankLacksRankRenamePermission;
            else if (rankToModify == null)
                result = GuildResult.InvalidRank;
            else if (memberRank.Index >= operation.Id)
                result = GuildResult.CanOnlyModifyRanksBelowYours;
            
            if (result == GuildResult.Success)
            {
                rankToModify.SetPermission((GuildRankPermission)operation.Value - 1);
                guild.SendToOnlineUsers(new ServerGuildRankChange
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Ranks = guild.GetGuildRanksPackets().ToList()
                });
                guild.AnnounceGuildResult(GuildResult.RankModified, operation.Id, operation.TextValue);
            }
            else
                SendGuildResult(session, result, guild);
        }

        [GuildOperationHandler(GuildOperation.TaxUpdate)]
        private static void GuildOperationTaxUpdate(WorldSession session, ClientGuildOperation operation)
        {
            var guild = (Guild)guilds[operation.GuildId];
            var memberRank = guild.GetMember(session.Player.CharacterId).Rank;

            GuildResult result = GuildResult.Success;

            if (memberRank.Index > 0)
                result = GuildResult.RankLacksSufficientPermissions;

            if (result == GuildResult.Success)
            {
                guild.SetTaxes(Convert.ToBoolean(operation.Value));
                guild.SendToOnlineUsers(new ServerGuildTaxUpdate
                {
                    RealmId = WorldServer.RealmId,
                    GuildId = guild.Id,
                    Value = guild.Taxes
                });
            }
            else
                SendGuildResult(session, result, guild);
        }
    }
}
