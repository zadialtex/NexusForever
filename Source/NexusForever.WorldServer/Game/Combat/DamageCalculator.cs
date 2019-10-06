using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Combat
{
    class DamageCalculator
    {
        public uint GetBaseDamageAmount(WorldEntity entity, float parameterType, float parameterValue)
        {
            switch (parameterType)
            {
                case 10:
                    return (uint)Math.Round(entity.Level * parameterValue);
                case 12:
                    return (uint)Math.Round(entity.GetAssaultPower() * parameterValue);
                case 13:
                    return (uint)Math.Round(entity.GetSupportPower() * parameterValue);
            }

            return 0u;
        }
    }
}
