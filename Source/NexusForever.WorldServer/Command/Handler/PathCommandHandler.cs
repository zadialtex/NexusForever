using System.Threading.Tasks;
using NexusForever.WorldServer.Command.Attributes;
using NexusForever.WorldServer.Command.Contexts;
using NexusForever.WorldServer.Game.Account.Static;
using NexusForever.WorldServer.Game.Entity.Static;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Name("Path", Permission.None)]
    public class PathCommandHandler : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public PathCommandHandler()
            : base(true, "path")
        {
        }

        [SubCommandHandler("activate", "pathId - Activate a path for this player.", Permission.CommandPathActivate)]
        public Task AddPathActivateSubCommand(CommandContext context, string command, string[] parameters)
        {
            if (parameters.Length <= 0)
                return Task.CompletedTask;

            uint newPath = 0;
            if (parameters.Length > 0)
                newPath = uint.Parse(parameters[0]);

            if (newPath > 3)
            {
                context.SendMessageAsync($"Path not recognised.");
                return Task.CompletedTask;
            }

            PathHandler.HandlePathActivate(context.Session, new Network.Message.Model.ClientPathActivate
            {
                Path = (Path)newPath,
                UseTokens = true
            });

            return Task.CompletedTask;
        }

        [SubCommandHandler("unlock", "pathId - Unlock a path for this player.", Permission.CommandPathUnlock)]
        public Task AddPathUnlockSubCommand(CommandContext context, string command, string[] parameters)
        {
            if (parameters.Length <= 0)
            {
                context.SendMessageAsync($"invalid parameters. Ensure you pass the pathId you wish to unlock.");
                return Task.CompletedTask;
            }
                
            uint unlockPath = uint.Parse(parameters[0]);
            if (unlockPath > 3)
            {
                context.SendMessageAsync($"Path not recognised.");
                return Task.CompletedTask;
            }

            PathHandler.HandlePathUnlock(context.Session, new Network.Message.Model.ClientPathUnlock
            {
                Path = (Path)unlockPath
            });

            return Task.CompletedTask;
        }

        [SubCommandHandler("addxp", "xp - Add the XP value to the player's curent Path XP.", Permission.CommandPathAddXp)]
        public Task AddPathAddXPSubCommand(CommandContext context, string command, string[] parameters)
        {
            if (parameters.Length > 0)
            {
                uint xp = uint.Parse(parameters[0]);
                context.Session.Player.PathManager.AddXp(xp);
            }

            return Task.CompletedTask;
        }
    }
}
