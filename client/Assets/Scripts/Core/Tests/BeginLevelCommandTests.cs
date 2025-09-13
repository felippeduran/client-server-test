using System;
using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class BeginLevelCommandTests
{
    [Test]
    public void TestBeginCommand_WithEnergyAndLevelUnlocked_ShouldSucceed()
    {
        var playerState = new PlayerState
        {
            Persistent = new PersistentState
            {
                Energy = new Energy
                {
                    CurrentAmount = 1,
                    LastRechargeAt = DateTime.UtcNow,
                },
                LevelProgression = new LevelProgression
                {
                    CurrentLevel = 1,
                    Statistics = new SortedSet<LevelStats>(),
                }
            },
        };

        var configs = GetTestConfigs();

        var command = new BeginLevelCommand
        {
            LevelId = 1,
        };

        command.Execute(playerState, configs);

        Assert.AreEqual(0, playerState.Persistent.Energy.CurrentAmount);
        Assert.AreEqual(1, playerState.Session.CurrentLevelId);
    }

    [Test]
    public void TestBeginCommand_WithoutEnergy_ShouldThrow()
    {
        var playerState = new PlayerState
        {
            Persistent = new PersistentState
            {
                Energy = new Energy { CurrentAmount = 0, LastRechargeAt = DateTime.UtcNow },
                LevelProgression = new LevelProgression
                {
                    CurrentLevel = 1,
                    Statistics = new SortedSet<LevelStats>(),
                }
            },
        };

        var configs = GetTestConfigs();

        var command = new BeginLevelCommand
        {
            LevelId = 1,
        };

        var exception = Assert.Throws<MetagameException>(() => command.Execute(playerState, configs));
        Assert.AreEqual("not enough energy", exception.Message);
    }

    [Test]
    public void TestBeginCommand_LevelNotUnlocked_ShouldThrow()
    {
        var playerState = new PlayerState
        {
            Persistent = new PersistentState
            {
                Energy = new Energy { CurrentAmount = 1, },
                LevelProgression = new LevelProgression { CurrentLevel = 0, Statistics = new SortedSet<LevelStats>() },
            },
        };

        var configs = GetTestConfigs();

        var command = new BeginLevelCommand
        {
            LevelId = 1,
        };

        var exception = Assert.Throws<MetagameException>(() => command.Execute(playerState, configs));
        Assert.AreEqual("level not unlocked", exception.Message);
    }
    
    Configs GetTestConfigs()
    {
        return new Configs
        {
            Energy = new EnergyConfig { MaxEnergy = 10, RechargeInterval = TimeSpan.FromSeconds(10) },
            Levels = new[]
            {
                new LevelConfig { },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 1, EnergyReward = 1 },
            },
        };
    }
}