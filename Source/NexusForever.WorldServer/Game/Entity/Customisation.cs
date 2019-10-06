﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Customisation
    {
        public ulong CharacterId { get; }
        public uint Label { get; }

        public uint Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    saveMask |= CustomisationSaveMask.Modify;
                }
            }
        }
        private uint value;

        public bool PendingDelete => (saveMask & CustomisationSaveMask.Delete) != 0;

        private CustomisationSaveMask saveMask;

        public Customisation(CharacterCustomisation model)
        {
            CharacterId = model.Id;
            Label = model.Label;
            value = model.Value;

            saveMask = CustomisationSaveMask.None;
        }

        public Customisation(ulong characterId, uint label, uint value)
        {
            CharacterId = characterId;
            Label = label;
            this.value = value;

            saveMask = CustomisationSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != CustomisationSaveMask.None)
            {
                var model = new CharacterCustomisation
                {
                    Id = CharacterId,
                    Label = Label
                };

                EntityEntry<CharacterCustomisation> entity = context.Attach(model);

                if ((saveMask & CustomisationSaveMask.Create) != 0)
                {
                    model.Value = Value;

                    context.Add(model);
                }
                else if ((saveMask & CustomisationSaveMask.Delete) != 0)
                {
                    context.Entry(model).State = EntityState.Deleted;
                }
                else
                {
                    if ((saveMask & CustomisationSaveMask.Modify) != 0)
                    {
                        model.Value = Value;
                        entity.Property(e => e.Value).IsModified = true;
                    }
                }
            }

            saveMask = CustomisationSaveMask.None;
        }

        public void Delete()
        {
            if ((saveMask & CustomisationSaveMask.Create) != 0)
                saveMask = CustomisationSaveMask.None;
            else
                saveMask = CustomisationSaveMask.Delete;
        }
    }
}
