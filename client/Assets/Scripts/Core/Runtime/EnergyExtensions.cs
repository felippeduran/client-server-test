using System;

namespace Core.Runtime
{
    public static class EnergyExtensions
    {
        public static int GetPredictedAmount(this IReadOnlyEnergy energy, DateTime now, EnergyConfig energyConfig)
        {
            var lastRechargeAt = energy.LastRechargeAt;
            var timeSinceLastRecharge = now - lastRechargeAt;
            var predictedEnergy = energy.CurrentAmount + (int)(timeSinceLastRecharge.TotalSeconds / energyConfig.RechargeInterval.TotalSeconds);
            return Math.Min(predictedEnergy, energyConfig.MaxEnergy);
        }

        public static TimeSpan GetTimeRemainingForNextRecharge(this IReadOnlyEnergy energy, DateTime now, EnergyConfig energyConfig)
        {
            if (energy.GetPredictedAmount(now, energyConfig) == energyConfig.MaxEnergy)
            {
                return TimeSpan.Zero;
            }

            return energyConfig.RechargeInterval - TimeSpan.FromSeconds((now - energy.LastRechargeAt).TotalSeconds % energyConfig.RechargeInterval.TotalSeconds);
        }

        public static void UpdateEnergy(this Energy energy, DateTime now, EnergyConfig energyConfig)
        {
            var timeSinceLastRecharge = now - energy.LastRechargeAt;
            var recharges = (int)(timeSinceLastRecharge / energyConfig.RechargeInterval);

            energy.CurrentAmount = GetPredictedAmount(energy, now, energyConfig);
            energy.LastRechargeAt += recharges * energyConfig.RechargeInterval;
        }
    }
}