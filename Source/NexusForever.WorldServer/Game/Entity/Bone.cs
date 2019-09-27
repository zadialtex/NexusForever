﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Bone
    {
        public ulong Owner { get; }
        public byte BoneIndex { get; }

        public float BoneValue
        {
            get => boneValue;
            set
            {
                if (boneValue != value)
                {
                    boneValue = value;
                    saveMask |= CustomisationSaveMask.Modify;
                }
            }
        }
        private float boneValue;

        public bool PendingDelete => (saveMask & CustomisationSaveMask.Delete) != 0;

        private CustomisationSaveMask saveMask;

        public Bone(CharacterBone model)
        {
            Owner = model.Id;
            BoneIndex = model.BoneIndex;
            boneValue = model.Bone;

            saveMask = CustomisationSaveMask.None;
        }

        public Bone(ulong characterId, byte boneIndex, float value)
        {
            Owner = characterId;
            BoneIndex = boneIndex;
            boneValue = value;

            saveMask = CustomisationSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != CustomisationSaveMask.None)
            {
                var model = new CharacterBone
                {
                    Id = Owner,
                    BoneIndex = BoneIndex
                };

                EntityEntry<CharacterBone> entity = context.Attach(model);

                if ((saveMask & CustomisationSaveMask.Create) != 0)
                {
                    model.Bone = BoneValue;

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
                        model.Bone = BoneValue;
                        entity.Property(e => e.Bone).IsModified = true;
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
