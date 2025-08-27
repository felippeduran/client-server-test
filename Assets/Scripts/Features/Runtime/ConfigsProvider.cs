using System;

[Serializable]
public struct Configs
{
    public LevelConfig[] Levels;
}

[Serializable]
public struct LevelConfig
{
    public int EnergyCost;
    public int MaxRolls;
    public int TargetNumber;
    public int EnergyReward;
}

public static class ConfigsProvider
{
    public static Configs GetHardcodedConfigs()
    {
        return new Configs
        {
            Levels = new LevelConfig[] {
                new LevelConfig { },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 1, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 2, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 2, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 2, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 2, EnergyReward = 5 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 3, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 3, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 3, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 3, EnergyReward = 5 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 4, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 4, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 4, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 4, EnergyReward = 5 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 5, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 5, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 5, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 5, EnergyReward = 5 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 6, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 5, TargetNumber = 6, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 3, TargetNumber = 6, EnergyReward = 1 },
                new LevelConfig { EnergyCost = 1, MaxRolls = 1, TargetNumber = 6, EnergyReward = 5 },
            },
        };
    }
}