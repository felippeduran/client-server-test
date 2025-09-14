using System;
using Core.Runtime;

namespace LocalServer.Runtime
{
    public static class ConfigsProvider
    {
        public static Configs GetHardcodedConfigs()
        {
            return new Configs
            {
                Energy = new EnergyConfig { MaxEnergy = 50, RechargeIntervalSeconds = 10 },
                Levels = new LevelConfig[] {
                new LevelConfig { },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 1, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 2, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 2, EnergyReward = 4 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 2, EnergyReward = 7 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 2, EnergyReward = 10 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 3, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 3, EnergyReward = 4 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 3, EnergyReward = 7 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 3, EnergyReward = 10 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 4, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 4, EnergyReward = 4 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 4, EnergyReward = 7 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 4, EnergyReward = 10 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 5, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 5, EnergyReward = 4 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 5, EnergyReward = 7 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 5, EnergyReward = 10 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 6, EnergyReward = 2 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 6, EnergyReward = 4 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 6, EnergyReward = 7 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 6, EnergyReward = 10 },
            },
            };
        }
    }
}