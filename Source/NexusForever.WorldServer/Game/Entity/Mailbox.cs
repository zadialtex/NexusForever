using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.MailBox)]
    public class Mailbox : WorldEntity
    {
        public Mailbox()
            : base(EntityType.MailBox)
        {
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new MailboxEntityModel
            {
                CreatureId = CreatureId
            };
        }

        public override ServerEntityCreate BuildCreatePacket()
        {
            ServerEntityCreate entityCreate = base.BuildCreatePacket();
            entityCreate.CreateFlags = 0;

            // Proof of Concept for map props being "used/controlled" by entities
            if (CreatureId == 54655) // Tremor Ridge, Algoroc, mailbox entity
            {
                entityCreate.UnknownB0 = new ServerEntityCreate.UnknownStructureB0
                {
                    Type = 1,
                    ActivePropId = 4787740, // 4787740 = Tremor Ridge, Algoroc, Mailbox Prop
                    Unknown2 = 0
                };
            }

            return entityCreate;
        }
    }
}
