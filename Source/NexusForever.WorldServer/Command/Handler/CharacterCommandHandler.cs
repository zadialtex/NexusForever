using System.Threading.Tasks;
using NexusForever.WorldServer.Command.Attributes;
using NexusForever.WorldServer.Command.Contexts;

namespace NexusForever.WorldServer.Command.Handler
{
    [Name("Character")]
    public class CharacterCommandHandler : CommandCategory
    {
        
        public CharacterCommandHandler()
            : base(true, "character")
        {
        }

        [SubCommandHandler("addxp", "amount - Add the amount to your total xp.")]
        public Task AddXPCommand(CommandContext context, string command, string[] parameters)
        {
            if (parameters.Length > 0)
            {
                uint xp = uint.Parse(parameters[0]);
                context.Session.Player.GrantXp(xp);
            }

            return Task.CompletedTask;
        }
    }
}
