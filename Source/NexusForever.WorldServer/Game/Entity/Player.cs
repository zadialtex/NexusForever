using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Database;
using NexusForever.Shared.Database.Auth;
using NexusForever.Shared.Database.Auth.Model;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.GameTable.Static;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Database;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.Setting;
using NexusForever.WorldServer.Game.Setting.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Static;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Player : UnitEntity, ISaveAuth, ISaveCharacter
    {
        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        public ulong CharacterId { get; }
        public string Name { get; }
        public Sex Sex { get; }
        public Race Race { get; }
        public Class Class { get; }
        public List<float> Bones { get; } = new List<float>();

        public uint TotalXp
        {
            get => totalXp;
            set
            {
                totalXp = value;
                saveMask |= PlayerSaveMask.Xp;
            }
        }

        private uint totalXp;
        public uint XpToNextLevel { get; private set; }

        public Path Path
        {
            get => path;
            set
            {
                path = value;
                saveMask |= PlayerSaveMask.Path;
            }
        }
        private Path path;

        public sbyte CostumeIndex
        {
            get => costumeIndex;
            set
            {
                costumeIndex = value;
                saveMask |= PlayerSaveMask.Costume;
            }
        }
        private sbyte costumeIndex;

        public InputSets InputKeySet
        {
            get => inputKeySet;
            set
            {
                inputKeySet = value;
                saveMask |= PlayerSaveMask.InputKeySet;
            }
        }
        private InputSets inputKeySet;

        public byte InnateIndex
        {
            get => innateIndex;
            set
            {
                innateIndex = value;
                saveMask |= PlayerSaveMask.Innate;
            }
        }
        private byte innateIndex;
        public ushort BindPoint
        {
            get => bindPoint;
            set
            {
                bindPoint = value;
                saveMask |= PlayerSaveMask.BindPoint;
            }
        }
        private ushort bindPoint;

        public DateTime CreateTime { get; }
        public double TimePlayedTotal { get; private set; }
        public double TimePlayedLevel { get; private set; }
        public double TimePlayedSession { get; private set; }

        /// <summary>
        /// Guid of the <see cref="WorldEntity"/> that currently being controlled by the <see cref="Player"/>.
        /// </summary>
        public uint ControlGuid { get; private set; }

        /// <summary>
        /// Guid of the <see cref="Vehicle"/> the <see cref="Player"/> is a passenger on.
        /// </summary>
        public uint VehicleGuid
        {
            get => MovementManager.GetPlatform() ?? 0u;
            set => MovementManager.SetPlatform(value);
        }

        /// <summary>
        /// Guid of the <see cref="VanityPet"/> currently summoned by the <see cref="Player"/>.
        /// </summary>
        public uint? VanityPetGuid { get; set; }

        public WorldSession Session { get; }
        public bool IsLoading { get; private set; } = true;

        public Inventory Inventory { get; }
        public CurrencyManager CurrencyManager { get; }
        public PathManager PathManager { get; }
        public TitleManager TitleManager { get; }
        public SpellManager SpellManager { get; }
        public CostumeManager CostumeManager { get; }
        public PetCustomisationManager PetCustomisationManager { get; }
        public KeybindingManager KeybindingManager { get; }
        public DatacubeManager DatacubeManager { get; }
        public MailManager MailManager { get; }
        public ZoneMapManager ZoneMapManager { get; }
        public QuestManager QuestManager { get; }

        public VendorInfo SelectedVendorInfo { get; set; } // TODO unset this when too far away from vendor

        private UpdateTimer saveTimer = new UpdateTimer(SaveDuration);
        private PlayerSaveMask saveMask;

        private LogoutManager logoutManager;
        private PendingTeleport pendingTeleport;

        public Player(WorldSession session, Character model)
            : base(EntityType.Player)
        {
            ActivationRange = BaseMap.DefaultVisionRange;

            Session         = session;

            CharacterId     = model.Id;
            Name            = model.Name;
            Sex             = (Sex)model.Sex;
            Race            = (Race)model.Race;
            Class           = (Class)model.Class;
            Path            = (Path)model.ActivePath;
            CostumeIndex    = model.ActiveCostumeIndex;
            InputKeySet     = (InputSets)model.InputKeySet;
            Faction1        = (Faction)model.FactionId;
            Faction2        = (Faction)model.FactionId;
            innateIndex     = model.InnateIndex;
            BindPoint       = model.BindPoint;
            TotalXp         = model.TotalXp;
            XpToNextLevel   = GameTableManager.XpPerLevel.Entries.FirstOrDefault(c => c.Id == Level + 1).MinXpForLevel;

            CreateTime      = model.CreateTime;
            TimePlayedTotal = model.TimePlayedTotal;
            TimePlayedLevel = model.TimePlayedLevel;

            // managers
            CostumeManager          = new CostumeManager(this, session.Account, model);
            Inventory               = new Inventory(this, model);
            CurrencyManager         = new CurrencyManager(this, model);
            PathManager             = new PathManager(this, model);
            TitleManager            = new TitleManager(this, model);
            SpellManager            = new SpellManager(this, model);
            PetCustomisationManager = new PetCustomisationManager(this, model);
            KeybindingManager       = new KeybindingManager(this, session.Account, model);
            DatacubeManager         = new DatacubeManager(this, model);
            MailManager             = new MailManager(this, model);
            ZoneMapManager          = new ZoneMapManager(this, model);
            QuestManager            = new QuestManager(this, model);

            Session.EntitlementManager.OnNewCharacter(model);

            // temp
            Properties.Add(Property.BaseHealth, new PropertyValue(Property.BaseHealth, 200f, 800f));
            Properties.Add(Property.ShieldCapacityMax, new PropertyValue(Property.ShieldCapacityMax, 0f, 450f));
            Properties.Add(Property.MoveSpeedMultiplier, new PropertyValue(Property.MoveSpeedMultiplier, 1f, 1f));
            Properties.Add(Property.JumpHeight, new PropertyValue(Property.JumpHeight, 2.5f, 2.5f));
            Properties.Add(Property.GravityMultiplier, new PropertyValue(Property.GravityMultiplier, 1f, 1f));
            // sprint
            Properties.Add(Property.ResourceMax0, new PropertyValue(Property.ResourceMax0, 500f, 500f));
            // dash
            Properties.Add(Property.ResourceMax7, new PropertyValue(Property.ResourceMax7, 200f, 200f));

            Costume costume = null;
            if (CostumeIndex >= 0)
                costume = CostumeManager.GetCostume((byte)CostumeIndex);

            SetAppearance(Inventory.GetItemVisuals(costume));
            SetAppearance(model.CharacterAppearance
                .Select(a => new ItemVisual
                {
                    Slot      = (ItemSlot)a.Slot,
                    DisplayId = a.DisplayId
                }));

            foreach (CharacterBone bone in model.CharacterBone.OrderBy(bone => bone.BoneIndex))
                Bones.Add(bone.Bone);

            foreach (CharacterStats statModel in model.CharacterStats)
                stats.Add((Stat)statModel.Stat, new StatValue(statModel));

            SetStat(Stat.Sheathed, 1u);

            // temp
            SetStat(Stat.Dash, 200F);
            // sprint
            SetStat(Stat.Resource0, 500f);
            SetStat(Stat.Shield, 450u);
        }

        public override void Update(double lastTick)
        {
            if (logoutManager != null)
            {
                // don't process world updates while logout is finalising
                if (logoutManager.ReadyToLogout)
                    return;

                logoutManager.Update(lastTick);
            }

            base.Update(lastTick);
            TitleManager.Update(lastTick);
            SpellManager.Update(lastTick);
            CostumeManager.Update(lastTick);
            QuestManager.Update(lastTick);

            saveTimer.Update(lastTick);
            if (saveTimer.HasElapsed)
            {
                double timeSinceLastSave = GetTimeSinceLastSave();
                TimePlayedSession += timeSinceLastSave;
                TimePlayedLevel += timeSinceLastSave;
                TimePlayedTotal += timeSinceLastSave;

                Save();
            }
        }

        /// <summary>
        /// Save <see cref="Account"/> and <see cref="Character"/> to the database.
        /// </summary>
        public void Save(Action callback = null)
        {
            Session.EnqueueEvent(new TaskEvent(AuthDatabase.Save(Save),
            () =>
            {
                Session.EnqueueEvent(new TaskEvent(CharacterDatabase.Save(Save),
                () =>
                {
                    callback?.Invoke();
                    Session.CanProcessPackets = true;
                    saveTimer.Resume();
                }));
            }));

            saveTimer.Reset(false);

            // prevent packets from being processed until asynchronous player save task is complete
            Session.CanProcessPackets = false;
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PlayerEntityModel
            {
                Id       = CharacterId,
                RealmId  = WorldServer.RealmId,
                Name     = Name,
                Race     = Race,
                Class    = Class,
                Sex      = Sex,
                Bones    = Bones,
                Title    = TitleManager.ActiveTitleId,
                PvPFlag  = PvPFlag.Disabled
            };
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            IsLoading = true;

            Session.EnqueueMessageEncrypted(new ServerChangeWorld
            {
                WorldId  = (ushort)map.Entry.Id,
                Position = new Position(vector)
            });

            base.OnAddToMap(map, guid, vector);
            map.OnAddToMap(this);

            // resummon vanity pet if it existed before teleport
            if (pendingTeleport?.VanityPetId != null)
            {
                var vanityPet = new VanityPet(this, pendingTeleport.VanityPetId.Value);
                map.EnqueueAdd(vanityPet, Position);
            }

            pendingTeleport = null;

            SendPacketsAfterAddToMap();
            Session.EnqueueMessageEncrypted(new ServerPlayerEnteredWorld());

            IsLoading = false;
        }

        public override void OnRelocate(Vector3 vector)
        {
            base.OnRelocate(vector);
            saveMask |= PlayerSaveMask.Location;

            ZoneMapManager.OnRelocate(vector);
        }

        protected override void OnZoneUpdate()
        {
            if (Zone != null)
            {
                TextTable tt = GameTableManager.GetTextTable(Language.English);

                Session.EnqueueMessageEncrypted(new ServerChat
                {
                    Guid    = Session.Player.Guid,
                    Channel = ChatChannel.System,
                    Text    = $"New Zone: ({Zone.Id}){tt.GetEntry(Zone.LocalizedTextIdName)}"
                });

                uint tutorialId = AssetManager.GetTutorialIdForZone(Zone.Id);
                if (tutorialId > 0)
                {
                    Session.EnqueueMessageEncrypted(new ServerTutorial
                    {
                        TutorialId = tutorialId
                    });
                }

                QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, Zone.Id, 1);
            }

            ZoneMapManager.OnZoneUpdate();
        }

        private void SendPacketsAfterAddToMap()
        {
            SendInGameTime();
            PathManager.SendInitialPackets();
            BuybackManager.SendBuybackItems(this);

            Session.EnqueueMessageEncrypted(new ServerHousingNeighbors());
            Session.EnqueueMessageEncrypted(new Server00F1());
            SetControl(this);

            // TODO: Move to Unlocks/Rewards Handler. A lot of these are tied to Entitlements which display in the character sheet, but don't actually unlock anything without this packet.
            Session.EnqueueMessageEncrypted(new ServerRewardPropertySet
            {
                Variables = new List<ServerRewardPropertySet.RewardProperty>
                {
                    new ServerRewardPropertySet.RewardProperty
                    {
                        Id    = RewardProperty.CostumeSlots,
                        Type  = 1,
                        Value = CostumeManager.CostumeCap
                    },
                    new ServerRewardPropertySet.RewardProperty
                    {
                        Id    = RewardProperty.ExtraDecorSlots,
                        Type  = 1,
                        Value = 2000
                    },
                    new ServerRewardPropertySet.RewardProperty
                    {
                        Id    = RewardProperty.BagSlots,
                        Type  = 1,
                        Value = 4
                    },
                    new ServerRewardPropertySet.RewardProperty
                    {
                        Id    = RewardProperty.Trading,
                        Type  = 1,
                        Value = 1
                    },
                    new ServerRewardPropertySet.RewardProperty
                    {
                        Id    = RewardProperty.ExtraDecorSlots,
                        Type  = 1,
                        Value = 2000
                    }
                }
            });

            CostumeManager.SendInitialPackets();

            var playerCreate = new ServerPlayerCreate
            {
                ItemProficiencies = GetItemProficiences(),
                FactionData       = new ServerPlayerCreate.Faction
                {
                    FactionId = Faction1, // This does not do anything for the player's "main" faction. Exiles/Dominion
                },
                ActiveCostumeIndex    = CostumeIndex,
                InputKeySet           = (uint)InputKeySet,
                CharacterEntitlements = Session.EntitlementManager.GetCharacterEntitlements()
                    .Select(e => new ServerPlayerCreate.CharacterEntitlement
                    {
                        Entitlement = e.Type,
                        Count       = e.Amount
                    })
                    .ToList()
                BindPoint = BindPoint,
                Xp = TotalXp
            };

            foreach (Currency currency in CurrencyManager)
                playerCreate.Money[(byte)currency.Id - 1] = currency.Amount;

            foreach (Item item in Inventory
                .Where(b => b.Location != InventoryLocation.Ability)
                .SelectMany(i => i))
            {
                playerCreate.Inventory.Add(new InventoryItem
                {
                    Item   = item.BuildNetworkItem(),
                    Reason = ItemUpdateReason.NoReason
                });
            }

            playerCreate.SpecIndex = SpellManager.ActiveActionSet;

            Session.EnqueueMessageEncrypted(playerCreate);

            TitleManager.SendTitles();
            SpellManager.SendInitialPackets();
            PetCustomisationManager.SendInitialPackets();
            KeybindingManager.SendInitialPackets();
            DatacubeManager.SendInitialPackets();
            MailManager.SendInitialPackets();
            ZoneMapManager.SendInitialPackets();
            Session.AccountCurrencyManager.SendInitialPackets();
            QuestManager.SendInitialPackets();
            Session.EnqueueMessageEncrypted(new ServerPlayerInnate
            {
                InnateIndex = InnateIndex
            });
        }

        public ItemProficiency GetItemProficiences()
        {
            ClassEntry classEntry = GameTableManager.Class.GetEntry((ulong)Class);
            return (ItemProficiency)classEntry.StartingItemProficiencies;

            //TODO: Store proficiences in DB table and load from there. Do they change ever after creation? Perhaps something for use on custom servers?
        }

        public override void OnRemoveFromMap()
        {
            // enqueue removal of existing vanity pet if summoned
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.RemoveFromMap();
                VanityPetGuid = null;
            }

            base.OnRemoveFromMap();

            if (pendingTeleport != null)
                MapManager.AddToMap(this, pendingTeleport.Info, pendingTeleport.Vector);
        }

        public override void AddVisible(GridEntity entity)
        {
            base.AddVisible(entity);
            Session.EnqueueMessageEncrypted(((WorldEntity)entity).BuildCreatePacket());

            if (entity is Player player)
                player.PathManager.SendSetUnitPathTypePacket();

            if (entity == this)
            {
                Session.EnqueueMessageEncrypted(new ServerPlayerChanged
                {
                    Guid     = entity.Guid,
                    Unknown1 = 1
                });
            }
        }

        public override void RemoveVisible(GridEntity entity)
        {
            base.RemoveVisible(entity);

            if (entity != this)
            {
                Session.EnqueueMessageEncrypted(new ServerEntityDestroy
                {
                    Guid     = entity.Guid,
                    Unknown0 = true
                });
            }
        }

        /// <summary>
        /// Set the <see cref="WorldEntity"/> that currently being controlled by the <see cref="Player"/>.
        /// </summary>
        public void SetControl(WorldEntity entity)
        {
            ControlGuid = entity.Guid;
            entity.ControllerGuid = Guid;

            Session.EnqueueMessageEncrypted(new ServerMovementControl
            {
                Ticket    = 1,
                Immediate = true,
                UnitId    = entity.Guid
            });
        }

        /// <summary>
        /// Start delayed logout with optional supplied time and <see cref="LogoutReason"/>.
        /// </summary>
        public void LogoutStart(double timeToLogout = 30d, LogoutReason reason = LogoutReason.None, bool requested = true)
        {
            if (logoutManager != null)
                return;

            logoutManager = new LogoutManager(timeToLogout, reason, requested);

            Session.EnqueueMessageEncrypted(new ServerLogoutUpdate
            {
                TimeTillLogout     = (uint)timeToLogout * 1000,
                Unknown0           = false,
                SignatureBonusData = new ServerLogoutUpdate.SignatureBonuses
                {
                    // see FillSignatureBonuses in ExitWindow.lua for more information
                    Xp                = 0,
                    ElderPoints       = 0,
                    Currencies        = new ulong[15],
                    AccountCurrencies = new ulong[19]
                }
            });
        }

        /// <summary>
        /// Cancel the current logout, this will fail if the timer has already elapsed.
        /// </summary>
        public void LogoutCancel()
        {
            // can't cancel logout if timer has already elapsed
            if (logoutManager?.ReadyToLogout ?? false)
                return;

            logoutManager = null;
        }

        /// <summary>
        /// Finishes the current logout, saving and cleaning up the <see cref="Player"/> before redirect to the character screen.
        /// </summary>
        public void LogoutFinish()
        {
            if (logoutManager == null)
                throw new InvalidPacketValueException();

            Session.EnqueueMessageEncrypted(new ServerLogout
            {
                Requested = logoutManager.Requested,
                Reason    = logoutManager.Reason
            });

            CleanUp();
        }

        /// <summary>
        /// Save to the database, remove from the world and release from parent <see cref="WorldSession"/>.
        /// </summary>
        public void CleanUp()
        {
            CleanupManager.Track(Session.Account);

            try
            {
                Save(() =>
                {
                    RemoveFromMap();
                    Session.Player = null;
                });
            }
            finally
            {
                CleanupManager.Untrack(Session.Account);
            }
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to supplied location.
        /// </summary>
        public void TeleportTo(ushort worldId, float x, float y, float z, uint instanceId = 0u, ulong residenceId = 0ul)
        {
            WorldEntry entry = GameTableManager.World.GetEntry(worldId);
            if (entry == null)
                throw new ArgumentException();

            TeleportTo(entry, new Vector3(x, y, z), instanceId, residenceId);
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to supplied location.
        /// </summary>
        public void TeleportTo(WorldEntry entry, Vector3 vector, uint instanceId = 0u, ulong residenceId = 0ul)
        {
            if (DisableManager.Instance.IsDisabled(DisableType.World, entry.Id))
            {
                SendSystemMessage($"Unable to teleport to world {entry.Id} because it is disabled.");
                return;
            }

            if (Map?.Entry.Id == entry.Id)
            {
                // TODO: don't remove player from map if it's the same as destination
            }

            // store vanity pet summoned before teleport so it can be summoned again after being added to the new map
            uint? vanityPetId = null;
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                vanityPetId = pet?.Creature.Id;
            }

            var info = new MapInfo(entry, instanceId, residenceId);
            pendingTeleport = new PendingTeleport(info, vector, vanityPetId);
            RemoveFromMap();
        }

        /// <summary>
        /// Used to send the current in game time to this player
        /// </summary>
        private void SendInGameTime()
        {
            uint lengthOfInGameDayInSeconds = ConfigurationManager<WorldServerConfiguration>.Config.LengthOfInGameDay;
            if (lengthOfInGameDayInSeconds == 0u)
                lengthOfInGameDayInSeconds = (uint)TimeSpan.FromHours(3.5d).TotalSeconds; // Live servers were 3.5h per in game day

            double timeOfDay = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds / lengthOfInGameDayInSeconds % 1;

            Session.EnqueueMessageEncrypted(new ServerTimeOfDay
            {
                TimeOfDay = (uint)(timeOfDay * TimeSpan.FromDays(1).TotalSeconds),
                LengthOfDay = lengthOfInGameDayInSeconds
            });
        }

        /// <summary>
        /// Reset and restore default appearance for <see cref="Player"/>.
        /// </summary>
        public void ResetAppearance()
        {
            DisplayInfo = 0;

            EnqueueToVisible(new ServerEntityVisualUpdate
            {
                UnitId      = Guid,
                Race        = (byte)Race,
                Sex         = (byte)Sex,
                ItemVisuals = GetAppearance().ToList()
            }, true);
        }
        
        /// Grants <see cref="Player"/> the supplied experience, handling level up if necessary.
        /// </summary>
        /// <param name="xp">Experience to grant</param>
        /// <param name="reason"><see cref="ExpReason"/> for the experience grant</param>
        public void GrantXp(uint xp, ExpReason reason = ExpReason.KillCreature)
        {
            uint maxLevel = 50;

            if (xp < 1)
                return;

            //if (!IsAlive)
            //    return;

            if (Level >= maxLevel)
                return;

            // TODO: Signature Bonus XP Calculation
            uint signatureXp = 0;

            // TODO: Rest XP Calculation
            uint restXp = 0;

            uint currentLevel = Level;
            uint currentXp = TotalXp;
            uint xpToNextLevel = XpToNextLevel;
            uint totalXp = xp + currentXp + signatureXp + restXp;

            Session.EnqueueMessageEncrypted(new ServerExperienceGained
            {
                TotalXpGained = xp,
                RestXpAmount = restXp,
                SignatureXpAmount = signatureXp,
                Reason = reason
            });

            while(totalXp >= xpToNextLevel && currentLevel < maxLevel)// WorldServer.Rules.MaxLevel)
            {
                totalXp -= xpToNextLevel;

                if (currentLevel < maxLevel)
                    GrantLevel((byte)(Level + 1));

                currentLevel = Level;
                xpToNextLevel = XpToNextLevel;
            }

            SetXp(xp + currentXp + signatureXp + restXp);
        }

        /// <summary>
        /// Sets <see cref="Player"/> <see cref="TotalXp"/> to supplied value
        /// </summary>
        /// <param name="xp"></param>
        private void SetXp(uint xp)
        {
            TotalXp = xp;
        }

        /// <summary>
        /// Grants <see cref="Player"/> the supplied level and adjusts XP accordingly
        /// </summary>
        /// <param name="newLevel">New level to be set</param>
        public void GrantLevel(byte newLevel)
        {
            uint oldLevel = Level;

            if (newLevel == oldLevel)
                return;

            Level = newLevel;
            XpToNextLevel = GameTableManager.XpPerLevel.GetEntry((ulong)newLevel + 1).MinXpForLevel;

            // Grant Rewards for level up
            SpellManager.GrantSpells();
                // Unlock LAS slots
                // Unlock AMPs
                // Add feature access

            // Play Level up effect
            PlayLevelUpEffect(newLevel);
        }

        /// <summary>
        /// Play the level up effect for the supplied level on this player
        /// </summary>
        /// <param name="level">Level to play effect for</param>
        public void PlayLevelUpEffect(byte level)
        {
            CastSpell(53378, level, new SpellParameters());
        }

        /// <summary>
        /// Sets <see cref="Player"/> to the supplied level and adjusts XP accordingly. Mainly for use with GM commands.
        /// </summary>
        /// <param name="newLevel">New level to be set</param>
        /// <param name="reason"><see cref="ExpReason"/> for the level grant</param>
        public void SetLevel(byte newLevel, ExpReason reason = ExpReason.Cheat)
        {
            uint oldLevel = Level;

            if (newLevel == oldLevel)
                return;

            uint newXp = GameTableManager.XpPerLevel.GetEntry(newLevel).MinXpForLevel;
            Session.EnqueueMessageEncrypted(new ServerExperienceGained
            {
                TotalXpGained = newXp - TotalXp,
                RestXpAmount = 0,
                SignatureXpAmount = 0,
                Reason = reason
            });
            SetXp(newXp);

            GrantLevel(newLevel);
        }

        /// <summary>
        /// Send <see cref="GenericError"/> to <see cref="Player"/>.
        /// </summary>
        public void SendGenericError(GenericError error)
        {
            Session.EnqueueMessageEncrypted(new ServerGenericError
            {
                Error = error
            });
        }

        public void SendSystemMessage(string text)
        {
            Session.EnqueueMessageEncrypted(new ServerChat
            {
                Channel = ChatChannel.System,
                Text    = text
            });
        }

        public void Save(AuthContext context)
        {
            Session.GenericUnlockManager.Save(context);
            Session.AccountCurrencyManager.Save(context);
            Session.EntitlementManager.Save(context);

            CostumeManager.Save(context);
            KeybindingManager.Save(context);
        }

        public void Save(CharacterContext context)
        {
            var model = new Character
            {
                Id = CharacterId
            };

            EntityEntry<Character> entity = context.Attach(model);

            if (saveMask != PlayerSaveMask.None)
            {
                if ((saveMask & PlayerSaveMask.Location) != 0)
                {
                    model.LocationX = Position.X;
                    entity.Property(p => p.LocationX).IsModified = true;

                    model.LocationY = Position.Y;
                    entity.Property(p => p.LocationY).IsModified = true;

                    model.LocationZ = Position.Z;
                    entity.Property(p => p.LocationZ).IsModified = true;

                    model.WorldId = (ushort)Map.Entry.Id;
                    entity.Property(p => p.WorldId).IsModified = true;

                    model.WorldZoneId = (ushort)Zone.Id;
                    entity.Property(p => p.WorldZoneId).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Path) != 0)
                {
                    model.ActivePath = (uint)Path;
                    entity.Property(p => p.ActivePath).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Costume) != 0)
                {
                    model.ActiveCostumeIndex = CostumeIndex;
                    entity.Property(p => p.ActiveCostumeIndex).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.InputKeySet) != 0)
                {
                    model.InputKeySet = (sbyte)InputKeySet;
                    entity.Property(p => p.InputKeySet).IsModified = true;
                }
                
                if ((saveMask & PlayerSaveMask.Xp) != 0)
                {
                    model.TotalXp = TotalXp;
                    entity.Property(p => p.TotalXp).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Innate) != 0)
                {
                    model.InnateIndex = InnateIndex;
                    entity.Property(p => p.InnateIndex).IsModified = true;
                }
                
                if ((saveMask & PlayerSaveMask.BindPoint) != 0)
                {
                    model.BindPoint = BindPoint;
                    entity.Property(p => p.BindPoint).IsModified = true;
                }

                saveMask = PlayerSaveMask.None;
            }

            model.TimePlayedLevel = (uint)TimePlayedLevel;
            entity.Property(p => p.TimePlayedLevel).IsModified = true;
            model.TimePlayedTotal = (uint)TimePlayedTotal;
            entity.Property(p => p.TimePlayedTotal).IsModified = true;

            foreach (StatValue stat in stats.Values)
                stat.SaveCharacter(CharacterId, context);

            Inventory.Save(context);
            CurrencyManager.Save(context);
            PathManager.Save(context);
            TitleManager.Save(context);
            CostumeManager.Save(context);
            PetCustomisationManager.Save(context);
            KeybindingManager.Save(context);
            SpellManager.Save(context);
            DatacubeManager.Save(context);
            MailManager.Save(context);
            ZoneMapManager.Save(context);
            QuestManager.Save(context);

            Session.EntitlementManager.Save(context);
        }

        /// <summary>
        /// Returns the time in seconds that has past since the last <see cref="Player"/> save.
        /// </summary>
        public double GetTimeSinceLastSave()
        {
            return SaveDuration - saveTimer.Time;
        }
    }
}
