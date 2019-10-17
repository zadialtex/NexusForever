using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Event;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Spell
{
    public delegate void SpellEffectDelegate(Spell spell, UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info);

    public partial class Spell
    {
        [SpellEffectHandler(SpellEffectType.Damage)]
        private void HandleEffectDamage(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            // TODO: calculate damage
            info.AddDamage((DamageType)info.Entry.DamageType, 1337);
        }

        [SpellEffectHandler(SpellEffectType.UnitPropertyModifier)]
        private void HandleEffectPropertyModifier(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            PropertyModifier modifier = null;

            if (info.Entry.DataBits01 == 1) // Adjust value by percent
                modifier = new PropertyModifier(ModifierType.AdjustPercent, BitConverter.Int32BitsToSingle((int)info.Entry.DataBits02) * BitConverter.Int32BitsToSingle((int)info.Entry.DataBits03));

            if (info.Entry.DataBits01 == 2) // Override current value (mainly used by debuffs, and NPC buffs)
                modifier = new PropertyModifier(ModifierType.SetValue, BitConverter.Int32BitsToSingle((int)info.Entry.DataBits02));

            if (info.Entry.DataBits01 == 3) // Adjust current value
            {
                if (info.Entry.DataBits03 > 0u)
                    modifier = new PropertyModifier(ModifierType.AdjustValue, BitConverter.Int32BitsToSingle((int)info.Entry.DataBits02) * BitConverter.Int32BitsToSingle((int)info.Entry.DataBits03));
                else
                    modifier = new PropertyModifier(ModifierType.SetBase, BitConverter.Int32BitsToSingle((int)info.Entry.DataBits02)); // 0 = Set Base
            }

            if (info.Entry.DataBits01 == 4) // Adjust current value per stack
                modifier = new PropertyModifier(ModifierType.AdjustValueStack, BitConverter.Int32BitsToSingle((int)info.Entry.DataBits02) + BitConverter.Int32BitsToSingle((int)info.Entry.DataBits03), 0); // TODO: Increase stack count as necessary

            player.AddSpellModifierProperty((Property)info.Entry.DataBits00, parameters.SpellInfo.Entry.Id, modifier);
        }

        [SpellEffectHandler(SpellEffectType.Proxy)]
        private void HandleEffectProxy(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            SpellParameters proxyParameters = new SpellParameters
            {
                ParentSpellInfo = parameters.SpellInfo,
                RootSpellInfo = parameters.RootSpellInfo,
                UserInitiatedSpellCast = parameters.UserInitiatedSpellCast,
                BypassGlobalCooldown = true
            };

            events.EnqueueEvent(new SpellEvent(info.Entry.DelayTime / 1000d, () =>
            {
                if (info.Entry.TickTime > 0)
                {
                    double tickTime = info.Entry.TickTime;
                    for (int i = 1; i == info.Entry.DurationTime / tickTime; i++)
                        events.EnqueueEvent(new SpellEvent(tickTime * i / 1000d, () =>
                        {
                            target.CastSpell(info.Entry.DataBits01, proxyParameters);
                        }));
                }
                else
                    target.CastSpell(info.Entry.DataBits00, proxyParameters);
            }));
        }

        [SpellEffectHandler(SpellEffectType.Disguise)]
        private void HandleEffectDisguise(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            Creature2Entry creature2 = GameTableManager.Creature2.GetEntry(info.Entry.DataBits02);
            if (creature2 == null)
                return;

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Creature2DisplayGroupEntry.Entries.FirstOrDefault(d => d.Creature2DisplayGroupId == creature2.Creature2DisplayGroupId);
            if (displayGroupEntry == null)
                return;

            player.SetDisplayInfo(displayGroupEntry.Creature2DisplayInfoId);
        }

        [SpellEffectHandler(SpellEffectType.SummonMount)]
        private void HandleEffectSummonMount(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            // TODO: handle NPC mounting?
            if (!(target is Player player))
                return;

            if (!player.CanMount())
                return;

            var mount = new Mount(player, parameters.SpellInfo.Entry.Id, info.Entry.DataBits00, info.Entry.DataBits01, info.Entry.DataBits04);
            mount.EnqueuePassengerAdd(player, VehicleSeatType.Pilot, 0);

            // usually for hover boards
            /*if (info.Entry.DataBits04 > 0u)
            {
                mount.SetAppearance(new ItemVisual
                {
                    Slot      = ItemSlot.Mount,
                    DisplayId = (ushort)info.Entry.DataBits04
                });
            }*/

            player.Map.EnqueueAdd(mount, player.Position);

            // FIXME: also cast 52539,Riding License - Riding Skill 1 - SWC - Tier 1,34464 -- upon further investigation, this appeared to only trigger for characters who were created earlier in the game's lifetime.

            // TODO: There are other Riding Skills which need to be added when the player has them as known effects.
            uint mountSpeedSpell4Id = 0;
            switch (mount.MountType)
            {
                case PetType.GroundMount: // Cast 80530, Mount Sprint  - Tier 2, 36122
                    mountSpeedSpell4Id = 80530;
                    break;
                case PetType.HoverBoard: // Cast 80531, Hoverboard Sprint  - Tier 2, 36122
                    mountSpeedSpell4Id = 80531;
                    break;
                default:
                    mountSpeedSpell4Id = 80530;
                    break;

            }
            player.CastSpell(mountSpeedSpell4Id, new SpellParameters
            {
                ParentSpellInfo = parameters.SpellInfo,
                RootSpellInfo = parameters.RootSpellInfo,
                UserInitiatedSpellCast = false
            });
        }

        [SpellEffectHandler(SpellEffectType.Teleport)]
        private void HandleEffectTeleport(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            // Handle NPC teleporting?

            if (!(target is Player player))
                return;

            // Assuming that this is Recall to Transmat
            if (info.Entry.DataBits00 == 0)
            {
                if (player.BindPoint == 0) // Must have bindpoint set
                    return;

                Location bindPointLocation = AssetManager.GetBindPoint(player.BindPoint);
                Vector3 offset = new Vector3(0f, 1.5f, 0f); // TODO: Should use new Vector3(0f, 1.5f, 0f); when map props are being used

                player.Rotation = bindPointLocation.Rotation;
                player.TeleportTo(bindPointLocation.World, Vector3.Add(bindPointLocation.Position, offset));
                return;
            }

            WorldLocation2Entry locationEntry = GameTableManager.WorldLocation2.GetEntry(info.Entry.DataBits00);
            if (locationEntry == null)
                return;

            player.Rotation = new Quaternion(locationEntry.Facing0, locationEntry.Facing1, locationEntry.Facing2, locationEntry.Facing3).ToEulerDegrees();
            player.TeleportTo((ushort)locationEntry.WorldId, locationEntry.Position0, locationEntry.Position1, locationEntry.Position2);
        }

        [SpellEffectHandler(SpellEffectType.FullScreenEffect)]
        private void HandleFullScreenEffect(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            // TODO/FIXME: Add duration into the queue so that the spell will automatically finish at the correct time. This is a workaround for Full Screen Effects.
            events.EnqueueEvent(new Event.SpellEvent(info.Entry.DurationTime / 1000d, () => { status = SpellStatus.Finished; SendSpellFinish(); }));
        }

        [SpellEffectHandler(SpellEffectType.RapidTransport)]
        private void HandleEffectRapidTransport(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            TaxiNodeEntry taxiNode = GameTableManager.TaxiNode.GetEntry(parameters.TaxiNode);
            if (taxiNode == null)
                return;

            WorldLocation2Entry worldLocation = GameTableManager.WorldLocation2.GetEntry(taxiNode.WorldLocation2Id);
            if (worldLocation == null)
                return;

            if (!(target is Player player))
                return;

            var rotation = new Quaternion(worldLocation.Facing0, worldLocation.Facing0, worldLocation.Facing2, worldLocation.Facing3);
            player.Rotation = rotation.ToEulerDegrees();
            player.TeleportTo((ushort)worldLocation.WorldId, worldLocation.Position0, worldLocation.Position1, worldLocation.Position2);
        }

        [SpellEffectHandler(SpellEffectType.LearnDyeColor)]
        private void HandleEffectLearnDyeColor(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            player.Session.GenericUnlockManager.Unlock((ushort)info.Entry.DataBits00);
        }

        [SpellEffectHandler(SpellEffectType.UnlockMount)]
        private void HandleEffectUnlockMount(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            Spell4Entry spell4Entry = GameTableManager.Spell4.GetEntry(info.Entry.DataBits00);
            player.SpellManager.AddSpell(spell4Entry.Spell4BaseIdBaseSpell);

            player.Session.EnqueueMessageEncrypted(new ServerUnlockMount
            {
                Spell4Id = info.Entry.DataBits00
            });
        }

        [SpellEffectHandler(SpellEffectType.UnlockPetFlair)]
        private void HandleEffectUnlockPetFlair(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            player.PetCustomisationManager.UnlockFlair((ushort)info.Entry.DataBits00);
        }

        [SpellEffectHandler(SpellEffectType.UnlockVanityPet)]
        private void HandleEffectUnlockVanityPet(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            Spell4Entry spell4Entry = GameTableManager.Spell4.GetEntry(info.Entry.DataBits00);
            player.SpellManager.AddSpell(spell4Entry.Spell4BaseIdBaseSpell);

            player.Session.EnqueueMessageEncrypted(new ServerUnlockMount
            {
                Spell4Id = info.Entry.DataBits00
            });
        }

        [SpellEffectHandler(SpellEffectType.SummonVanityPet)]
        private void HandleEffectSummonVanityPet(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            if (!(target is Player player))
                return;

            // enqueue removal of existing vanity pet if summoned
            if (player.VanityPetGuid != null)
            {
                VanityPet oldVanityPet = player.GetVisible<VanityPet>(player.VanityPetGuid.Value);
                oldVanityPet?.RemoveFromMap();
                player.VanityPetGuid = 0u;
            }

            var vanityPet = new VanityPet(player, info.Entry.DataBits00);
            player.Map.EnqueueAdd(vanityPet, player.Position);
        }

        [SpellEffectHandler(SpellEffectType.Activate)]
        private void HandleEffectActivate(UnitEntity target, SpellTargetInfo.SpellTargetEffectInfo info)
        {
            log.Warn("Activate fired");

            if (!(caster is Player player))
                throw new ArgumentNullException("player");
            
            switch(target.CreatureId)
            {
                case 65958:
                case 66264:
                    Item effigyItem = player.Inventory.GetItemById(49083);
                    if (effigyItem == null)
                        return;

                    if(PublicEventManager.AddEffigy(player, effigyItem.StackCount))
                    {
                        player.Inventory.ItemDelete(new Network.Message.Model.Shared.ItemLocation
                        {
                            BagIndex = effigyItem.BagIndex,
                            Location = effigyItem.Location
                        }, ItemUpdateReason.Quest);

                        target.Map.Search(player.Position, target.Map.VisionRange, new SearchCheckRange(player.Position, player.Map.VisionRange), out List<GridEntity> intersectedEntities);
                        foreach (GridEntity entity in intersectedEntities)
                        {
                            if (entity is NonPlayer nonPlayer)
                                if (nonPlayer.CreatureId == 65980 || nonPlayer.CreatureId == 66263)
                                {
                                    if (PublicEventManager.GetEffigyCount() >= PublicEventManager.Step1Threshold && PublicEventManager.GetEffigyCount() < PublicEventManager.Step2Threshold)
                                        nonPlayer.CastSpell(75505, new SpellParameters());
                                    else if (PublicEventManager.GetEffigyCount() >= PublicEventManager.Step2Threshold)
                                        nonPlayer.CastSpell(75506, new SpellParameters());
                                }
                        }
                    }
                    break;
                default:
                    log.Warn($"Unhandled spell effect SpellEffectType.Activate");
                    break;
            }
        }
    }
}
