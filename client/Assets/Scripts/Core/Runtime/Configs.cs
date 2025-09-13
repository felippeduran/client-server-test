using System;

[Serializable]
public struct Configs
{
    public LevelConfig[] Levels;
    public EnergyConfig Energy;
}

[Serializable]
public struct EnergyConfig
{
    public int MaxEnergy;
    public int RechargeIntervalSeconds;
    
    public TimeSpan RechargeInterval => TimeSpan.FromSeconds(RechargeIntervalSeconds);
}

[Serializable]
public struct LevelConfig
{
    public int EnergyCost;
    public int MaxRolls;
    public int TargetNumber;
    public int EnergyReward;
}