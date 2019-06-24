using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Network.Command;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using ResidenceEntity = NexusForever.WorldServer.Game.Entity.Residence;

namespace NexusForever.WorldServer.Game.Map
{
    public class ResidenceMap : BaseMap
    {
        public ulong Id => residence?.Id ?? 0ul;
        // housing maps have unlimited vision range.
        public override float VisionRange { get; protected set; } = -1f;

        private Housing.Residence residence;

        public override void Initialise(MapInfo info, Player player)
        {
            base.Initialise(info, player);

            if (info.ResidenceId != 0u)
            {
                residence = ResidenceManager.GetCachedResidence(info.ResidenceId);
                if (residence == null)
                    throw new InvalidOperationException();
            }
            else
                residence = ResidenceManager.CreateResidence(player);

            // initialise plug entities
            foreach (Plot plot in residence.GetPlots().Where(p => p.PlugEntry != null))
            {
                var plug = new Plug(plot.PlotEntry, plot.PlugEntry);

                if (plot.Index == 0)
                    plug.SetPosition(new Vector3
                    {
                        X = 1472f,
                        Y = -715.0843505859375f,
                        Z = 1440f,
                    });

                EnqueueAdd(plug, Vector3.Zero);
            }
        }

        public override void OnAddToMap(Player player)
        {
            if (residence == null)
                throw new InvalidOperationException();

            SendHousingPrivacy(player);
            SendHousingProperties(player);

            var housingPlots = new ServerHousingPlots
            {
                RealmId     = WorldServer.RealmId,
                ResidenceId = residence.Id,
            };

            foreach (Plot plot in residence.GetPlots())
            {
                housingPlots.Plots.Add(new ServerHousingPlots.Plot
                {
                    PlotPropertyIndex = plot.Index,
                    PlotInfoId        = plot.PlotEntry.Id,
                    PlugFacing        = plot.PlugFacing,
                    PlugItemId        = plot.PlugEntry?.Id ?? 0u,
                    BuildState        = plot.BuildState
                });
            } 

            player.Session.EnqueueMessageEncrypted(housingPlots);

            ResidenceEntity residenceEntity = new ResidenceEntity();
            player.Map.EnqueueAdd(residenceEntity, residenceEntity.Position);

            // this shows the housing toolbar, might need to move this to a more generic place in the future
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet            = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid                   = player.Guid
            });

            SendResidenceDecor(player);

            Simple door1 = new Simple
            {
                CreatureId = 72338,
                QuestChecklistIdx = 255,
                CreateFlags = Entity.Static.EntityCreateFlag.SpawnAnimation,
                DisplayInfo = 32614,
                Position = new Vector3
                {
                    X = 1487.921875f,
                    Y = -711.7042236328125f,
                    Z = 1440.810546875f,
                }
            };
            door1.SetPropData(84255740, 1159);
            Simple door2 = new Simple
            {
                CreatureId = 72338,
                QuestChecklistIdx = 255,
                CreateFlags = Entity.Static.EntityCreateFlag.SpawnAnimation,
                DisplayInfo = 32614,
                Position = new Vector3
                {
                    X = 1482.39453125f,
                    Y = -811.41650390625f,
                    Z = 1444.5390625f
                }
            };
            door2.SetPropData(0, 1159);

            player.Map.EnqueueAdd(door1, door1.Position);
            player.Map.EnqueueAdd(door2, door2.Position);
        }

        private void SendHousingPrivacy(Player player = null)
        {
            var housingPrivacy = new ServerHousingPrivacy
            {
                ResidenceId     = residence.Id,
                NeighbourhoodId = 0x190000000000000A, // magic numbers are bad
                PrivacyLevel    = ResidencePrivacyLevel.Public
            };

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingPrivacy);
            else
                EnqueueToAll(housingPrivacy);
        }

        private void SendHousingProperties(Player player = null)
        {
            var housingProperties = new ServerHousingProperties
            {
                Residences =
                {
                    new ServerHousingProperties.Residence
                    {
                        RealmId           = WorldServer.RealmId,
                        ResidenceId       = residence.Id,
                        NeighbourhoodId   = 0x190000000000000A,
                        CharacterIdOwner  = residence.OwnerId,
                        Name              = residence.Name,
                        PropertyInfoId    = residence.PropertyInfoId,
                        ResidenceInfoId   = 22,
                        WallpaperExterior = residence.Wallpaper,
                        Entryway          = residence.Entryway,
                        Roof              = residence.Roof,
                        Door              = residence.Door,
                        Ground            = residence.Ground,
                        Sky               = residence.Sky,
                        Flags             = residence.Flags,
                        ResourceSharing   = residence.ResourceSharing,
                        GardenSharing     = residence.GardenSharing
                    }
                }
            };

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
        }

        private void SendResidenceDecor(Player player)
        {
            var residenceDecor = new ServerHousingResidenceDecor();

            Decor[] decors = residence.GetDecor().ToArray();
            for (uint i = 0u; i < decors.Length; i++)
            {
                // client freaks out if too much decor is sent in a single message, limit to 100
                if (i != 0u && i % 100u == 0u)
                {
                    player.Session.EnqueueMessageEncrypted(residenceDecor);
                    residenceDecor = new ServerHousingResidenceDecor();
                }

                Decor decor = decors[i];
                residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId     = WorldServer.RealmId,
                    DecorId     = decor.DecorId,
                    ResidenceId = residence.Id,
                    DecorType   = decor.Type,
                    Scale       = decor.Scale,
                    Position    = decor.Position,
                    Rotation    = decor.Rotation,
                    DecorInfoId = decor.Entry.Id
                });

                if (i == decors.Length - 1)
                    player.Session.EnqueueMessageEncrypted(residenceDecor);
            }
        }

        /// <summary>
        /// Crate all placed <see cref="Decor"/>, this is called directly from a packet hander.
        /// </summary>
        public void CrateAllDecor(Player player)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            var housingResidenceDecor = new ServerHousingResidenceDecor();
            foreach (Decor decor in residence.GetPlacedDecor())
            {
                decor.Crate();

                housingResidenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId     = WorldServer.RealmId,
                    DecorId     = decor.DecorId,
                    ResidenceId = residence.Id,
                    DecorType   = decor.Type,
                    Scale       = decor.Scale,
                    Position    = decor.Position,
                    Rotation    = decor.Rotation,
                    DecorInfoId = decor.Entry.Id
                });
            }

            EnqueueToAll(housingResidenceDecor);
        }

        /// <summary>
        /// Update <see cref="Decor"/> (create, move or delete), this is called directly from a packet hander.
        /// </summary>
        public void DecorUpdate(Player player, ClientHousingDecorUpdate housingDecorUpdate)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            foreach (ClientHousingDecorUpdate.DecorUpdate update in housingDecorUpdate.DecorUpdates)
            {
                switch (housingDecorUpdate.Operation)
                {
                    case DecorUpdateOperation.Create:
                        DecorCreate(player, update);
                        break;
                    case DecorUpdateOperation.Move:
                        DecorMove(update);
                        break;
                    case DecorUpdateOperation.Delete:
                        DecorDelete(update);
                        break;
                    default:
                        throw new InvalidPacketValueException();
                }
            }
        }

        /// <summary>
        /// Create and add <see cref="Decor"/> from supplied <see cref="HousingDecorInfoEntry"/> to your crate.
        /// </summary>
        public void DecorCreate(HousingDecorInfoEntry entry, uint quantity)
        {
            var residenceDecor = new ServerHousingResidenceDecor();
            for (uint i = 0u; i < quantity; i++)
            {
                Decor decor = residence.DecorCreate(entry);
                decor.Type = DecorType.Crate;

                residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId     = WorldServer.RealmId,
                    DecorId     = decor.DecorId,
                    ResidenceId = residence.Id,
                    DecorType   = decor.Type,
                    Scale       = decor.Scale,
                    Position    = decor.Position,
                    Rotation    = decor.Rotation,
                    DecorInfoId = decor.Entry.Id
                });
            }

            EnqueueToAll(residenceDecor);
        }

        private void DecorCreate(Player player, ClientHousingDecorUpdate.DecorUpdate update)
        {
            HousingDecorInfoEntry entry = GameTableManager.HousingDecorInfo.GetEntry(update.DecorInfoId);
            if (entry == null)
                throw new InvalidPacketValueException();

            if (entry.CostCurrencyTypeId != 0u && entry.Cost != 0u)
            {
                /*if (!player.CurrencyManager.CanAfford((byte)entry.CostCurrencyTypeId, entry.Cost))
                {
                    // TODO: show error
                    return;
                }

                player.CurrencyManager.CurrencySubtractAmount((byte)entry.CostCurrencyTypeId, entry.Cost);*/
            }

            if (update.ColourShiftId != 0u)
            {
                ColorShiftEntry colourEntry = GameTableManager.ColorShift.GetEntry(update.ColourShiftId);
                if (colourEntry == null)
                    throw new InvalidPacketValueException();

                // TODO: colour shift
            }

            Decor decor = residence.DecorCreate(entry);
            decor.Type = update.DecorType;

            if (update.DecorType != DecorType.Crate)
            {
                if (update.Scale < 0f)
                    throw new InvalidPacketValueException();

                // new decor is being placed directly in the world
                decor.Position = update.Position;
                decor.Rotation = update.Rotation;
                decor.Scale    = update.Scale;
            }

            EnqueueToAll(new ServerHousingResidenceDecor
            {
                Operation = 0,
                DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                    new ServerHousingResidenceDecor.Decor
                    {
                        RealmId     = WorldServer.RealmId,
                        DecorId     = decor.DecorId,
                        ResidenceId = residence.Id,
                        DecorType   = decor.Type,
                        Scale       = decor.Scale,
                        Position    = decor.Position,
                        Rotation    = decor.Rotation,
                        DecorInfoId = decor.Entry.Id
                    }
                }
            });
        }

        private void DecorMove(ClientHousingDecorUpdate.DecorUpdate update)
        {
            Decor decor = residence.GetDecor(update.DecorId);
            if (decor == null)
                throw new InvalidPacketValueException();

            // TODO: research 0.835f
            if (decor.Type == DecorType.Crate)
            {
                if (decor.Entry.Creature2IdActiveProp != 0u)
                {
                    // TODO: used for decor that have an associated entity
                }

                // crate->world
                var position = new Vector3(update.Position.X, update.Position.Y + 0.835f, update.Position.Z);
                decor.Move(update.DecorType, position, update.Rotation, update.Scale);
            }
            else
            {
                if (update.DecorType == DecorType.Crate)
                    decor.Crate();
                else
                {
                    // world->world
                    var position = new Vector3(update.Position.X, update.Position.Y + 0.835f, update.Position.Z);
                    decor.Move(update.DecorType, position, update.Rotation, update.Scale);
                }
            }

            EnqueueToAll(new ServerHousingResidenceDecor
            {
                Operation = 0,
                DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                    new ServerHousingResidenceDecor.Decor
                    {
                        RealmId     = WorldServer.RealmId,
                        DecorId     = decor.DecorId,
                        ResidenceId = residence.Id,
                        DecorType   = decor.Type,
                        Scale       = decor.Scale,
                        Position    = decor.Position,
                        Rotation    = decor.Rotation,
                        DecorInfoId = decor.Entry.Id
                    }
                }
            });
        }

        private void DecorDelete(ClientHousingDecorUpdate.DecorUpdate update)
        {
            Decor decor = residence.GetDecor(update.DecorId);
            if (decor == null)
                throw new InvalidPacketValueException();

            if (decor.Position != Vector3.Zero)
                throw new InvalidOperationException();

            residence.DecorDelete(decor);

            // TODO: send packet to remove from decor list
        }

        /// <summary>
        /// Rename <see cref="Residence"/>, this is called directly from a packet hander.
        /// </summary>
        public void Rename(Player player, ClientHousingRenameProperty housingRenameProperty)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            residence.Name = housingRenameProperty.Name;
            SendHousingProperties();
        }

        /// <summary>
        /// Set <see cref="ResidencePrivacyLevel"/>, this is called directly from a packet hander.
        /// </summary>
        public void SetPrivacyLevel(Player player, ClientHousingSetPrivacyLevel housingSetPrivacyLevel)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            if (housingSetPrivacyLevel.PrivacyLevel == ResidencePrivacyLevel.Public)
                ResidenceManager.RegisterResidenceVists(residence.Id, residence.OwnerName, residence.Name);
            else
                ResidenceManager.DeregisterResidenceVists(residence.Id);

            residence.PrivacyLevel = housingSetPrivacyLevel.PrivacyLevel;
            SendHousingPrivacy();
        }

        /// <summary>
        /// Remodel <see cref="Residence"/>, this is called directly from a packet hander.
        /// </summary>
        public void Remodel(Player player, ClientHousingRemodel housingRemodel)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            // TODO
        }
    }
}
