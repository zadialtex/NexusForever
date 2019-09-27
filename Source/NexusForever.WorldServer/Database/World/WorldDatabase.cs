using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Database.World.Model;

namespace NexusForever.WorldServer.Database.World
{
    public static class WorldDatabase
    {
        public static ImmutableList<Entity> GetEntities(ushort world)
        {
            using (var context = new WorldContext())
                return context.Entity.Where(e => e.World == world)
                    .Include(e => e.EntityVendor)
                    .Include(e => e.EntityVendorCategory)
                    .Include(e => e.EntityVendorItem)
                    .Include(e => e.EntityStats)
                    .AsNoTracking()
                    .ToImmutableList();
        }

        public static ImmutableList<Entity> GetEntitiesWithoutArea()
        {
            using (var context = new WorldContext())
                return context.Entity.Where(e => e.Area == 0)
                    .AsNoTracking()
                    .ToImmutableList();
        }

        public static Entity GetEntity(uint creatureId)
        {
            using (var context = new WorldContext())
                return context.Entity.SingleOrDefault(e => e.Creature == creatureId);
        }

        public static void UpdateEntities(IEnumerable<Entity> models)
        {
            using (var context = new WorldContext())
            {
                foreach (Entity model in models)
                {
                    EntityEntry<Entity> entity = context.Attach(model);
                    entity.State = EntityState.Modified;
                }
               
                context.SaveChanges();
            }
        }

        public static ImmutableList<Tutorial> GetTutorialTriggers()
        {
            using (var context = new WorldContext())
                return context.Tutorial.ToImmutableList();
        }

        public static ImmutableList<Model.Disable> GetDisables()
        {
            using (var context = new WorldContext())
                return context.Disable.ToImmutableList();
        }

        public static ImmutableList<StoreCategory> GetStoreCategories()
        {
            using (var context = new WorldContext())
                return context.StoreCategory
                    .AsNoTracking()
                    .ToImmutableList();
        }

        public static ImmutableList<StoreOfferGroup> GetStoreOfferGroups()
        {
            using (var context = new WorldContext())
                return context.StoreOfferGroup
                    .Include(e => e.StoreOfferGroupCategory)
                    .Include(e => e.StoreOfferItem)
                        .ThenInclude(e => e.StoreOfferItemData)
                    .Include(e => e.StoreOfferItem)
                        .ThenInclude(e => e.StoreOfferItemPrice)
                    .AsNoTracking()
                    .ToImmutableList();
        }
    }
}
