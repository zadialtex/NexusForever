using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.WorldServer.Game.Entity;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Event
{
    public static class PublicEventManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static uint Step1Threshold { get; private set; } = 25;
        public static uint Step2Threshold { get; private set; } = 50;

        private static uint EffigyCount;
        private static uint EffigyResetTimeInSecond = 10800;
        private static DateTime BuildTime;
        private static bool dirtyCount = false;
        private static UpdateTimer UpdateTimer = new UpdateTimer(60);

        public static void Initialise()
        {
            EffigyCount = ConfigurationManager<WorldServerConfiguration>.Config.ShadesEveEffigyCount;
            BuildTime = ConfigurationManager<WorldServerConfiguration>.Config?.ShadesEveEffigyBuilt ?? DateTime.UtcNow;

            log.Info($"Effigy Count: {EffigyCount} | {BuildTime}");
        }

        public static void Update(double lastTick)
        {
            UpdateTimer.Update(lastTick);
            if (UpdateTimer.HasElapsed)
            {
                if (EffigyCount >= Step2Threshold && DateTime.UtcNow.Subtract(BuildTime).TotalSeconds >= EffigyResetTimeInSecond)
                {
                    dirtyCount = true;
                    EffigyCount = 0;
                }

                if (dirtyCount)
                {
                    ConfigurationManager<WorldServerConfiguration>.Config.ShadesEveEffigyBuilt = BuildTime;
                    ConfigurationManager<WorldServerConfiguration>.Config.ShadesEveEffigyCount = EffigyCount;
                    ConfigurationManager<WorldServerConfiguration>.Save();

                    dirtyCount = false;
                }

                UpdateTimer.Reset();
            }
        }

        public static bool AddEffigy(Player player, uint count)
        {
            if (EffigyCount >= Step2Threshold)
            {
                player.SendSystemMessage($"The effigy is built! Come back soon when it's burned out to help in the rebuilding!");
                return false;
            }
            dirtyCount = true;

            EffigyCount += count;

            if (EffigyCount >= Step2Threshold)
                BuildTime = DateTime.UtcNow;

            uint nextStep = (uint)(EffigyCount < Step1Threshold ? Step1Threshold : EffigyCount < Step2Threshold ? Step2Threshold : 0);
            string nextStepString = nextStep > 0 ? $"{nextStep - EffigyCount} needed to build next step." : "Thank you! You have completed the effigy! Make sure you come back when it's burned out to help rebuild.";

            player.SendSystemMessage($"Effigy materials accepted! {nextStepString}");

            return true;
        }

        public static uint GetEffigyCount()
        {
            return EffigyCount;
        }
    }
}
