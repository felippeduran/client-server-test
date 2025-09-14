using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Core.Runtime;

namespace Core.Tests
{
    [TestFixture]
    public class EndLevelCommandTests
    {
        [Test]
        public void TestEndCommand_WithLevelInProgress_ShouldSucceed()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = false,
                Score = 8,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Session.CurrentLevelId, Is.Null);
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndSuccess_ShouldAddLevelProgression()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        CurrentLevel = 1,
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = true,
                Score = 8,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.LevelProgression.CurrentLevel, Is.EqualTo(2));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.Count, Is.EqualTo(1));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().BestScore, Is.EqualTo(8));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Wins, Is.EqualTo(1));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Losses, Is.EqualTo(0));
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndFailure_ShouldAddLevelProgression()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        CurrentLevel = 1,
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = false,
                Score = 0,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.LevelProgression.CurrentLevel, Is.EqualTo(1));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.Count, Is.EqualTo(1));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().BestScore, Is.EqualTo(0));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Losses, Is.EqualTo(1));
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndBetterScore_ShouldUpdateLevelProgression()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        CurrentLevel = 1,
                        Statistics = new SortedSet<LevelStats> { new LevelStats { LevelId = 1, BestScore = 5, Wins = 1, Losses = 0 } },
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = true,
                Score = 8,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().BestScore, Is.EqualTo(8));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Wins, Is.EqualTo(2));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Losses, Is.EqualTo(0));
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndWorseScore_ShouldNotUpdateLevelProgression()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        CurrentLevel = 1,
                        Statistics = new SortedSet<LevelStats> { new LevelStats { LevelId = 1, BestScore = 5, Wins = 1, Losses = 0 } },
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = true,
                Score = 2,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().BestScore, Is.EqualTo(5));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Wins, Is.EqualTo(2));
            Assert.That(playerState.Persistent.LevelProgression.Statistics.First().Losses, Is.EqualTo(0));
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndFailure_ShouldNotDeliverRewards()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = false,
                Score = 8,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.Energy.CurrentAmount, Is.EqualTo(0));
        }

        [Test]
        public void TestEndCommand_WithLevelInProgressAndSuccess_ShouldDeliverRewards()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = true,
                Score = 8,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.Energy.CurrentAmount, Is.EqualTo(2));
        }

        [Test]
        public void TestEndCommand_WithPreviousLevelInProgress_ShouldNotChangeCurrentLevel()
        {
            var playerState = new PlayerState
            {
                Persistent = new PersistentState
                {
                    LevelProgression = new LevelProgression
                    {
                        CurrentLevel = 3,
                        Statistics = new SortedSet<LevelStats>(),
                    },
                },
                Session = new SessionState { CurrentLevelId = 1 },
            };

            var configs = GetTestConfigs();

            var command = new EndLevelCommand
            {
                Success = true,
            };

            command.Execute(playerState, configs);

            Assert.That(playerState.Persistent.LevelProgression.CurrentLevel, Is.EqualTo(3));
        }

        Configs GetTestConfigs()
        {
            return new Configs
            {
                Levels = new[]
                {
                new LevelConfig { },
                new LevelConfig { EnergyCost = 1, MaxRolls = 10, TargetNumber = 1, EnergyReward = 2 },
            },
            };
        }
    }
}