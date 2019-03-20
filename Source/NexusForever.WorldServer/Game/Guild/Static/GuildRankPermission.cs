using System;

namespace NexusForever.WorldServer.Game.Guild.Static
{
    [Flags]
    public enum GuildRankPermission
    {
        Leader                      = -2,
        None                        = 0,
        NoRank                      = 1,
        CreateAndRemoveRank         = 4,
        EditLowerRankPermissions    = 8,
        SpendInfluence              = 16,
        RenameRank                  = 32,
        Vote                        = 64,
        ChangeMemberRank            = 128,
        Invite                      = 256,
        Kick                        = 512,
        EditGuildHolomark           = 1024,
        MemberChat                  = 2048,
        CouncilChat                 = 4096,
        BankTabRename               = 8192,
        MessageOfTheDay             = 134217728,
        BankTabLog                  = 536870912,
    }
}
