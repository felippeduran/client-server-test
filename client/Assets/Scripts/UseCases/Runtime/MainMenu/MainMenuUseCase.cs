using System;
using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public class MainMenuUseCase : IMainMenuUseCase
    {
        readonly IScreenLibrary screenLibrary;

        public MainMenuUseCase(IScreenLibrary screenLibrary)
        {
            this.screenLibrary = screenLibrary;
        }

        public async Task<Play> OpenMainMenuAsync(IReadOnlyPlayer player, IClock clock, Configs configs, CancellationToken ct)
        {
            var currentLevel = player.State.Persistent.LevelProgression.CurrentLevel;
            screenLibrary.MainMenu.Show();
            screenLibrary.MainMenu.Setup(GetMainScreenData(player, currentLevel, clock.Now(), configs));

            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var actions = await screenLibrary.MainMenu.WaitForInputAsync(cts.Token);

                if (actions.Play)
                {
                    if (player.State.Persistent.Energy.GetPredictedAmount(clock.Now(), configs.Energy) < configs.Levels[currentLevel].EnergyCost)
                    {
                        await screenLibrary.Error.ShowAsync("Not enough energy", "OK", cts.Token);
                        continue;
                    }

                    return new Play { Level = currentLevel };
                }

                if (actions.ChangeLevelDirection != 0)
                {
                    currentLevel = Math.Clamp(currentLevel + actions.ChangeLevelDirection, 1, player.State.Persistent.LevelProgression.CurrentLevel);
                    screenLibrary.MainMenu.Setup(GetMainScreenData(player, currentLevel, clock.Now(), configs));
                }

                if (actions.OpenStats)
                {
                    await screenLibrary.Stats.OpenAsync(player.State.Persistent.LevelProgression.Statistics, cts.Token);
                }

                if (actions.Refresh)
                {
                    screenLibrary.MainMenu.Setup(GetMainScreenData(player, currentLevel, clock.Now(), configs));
                }
            }

            screenLibrary.MainMenu.Hide();
            return null;
        }

        private MainScreenData GetMainScreenData(IReadOnlyPlayer player, int currentLevel, DateTime now, Configs configs)
        {
            var levelConfig = configs.Levels[currentLevel];
            return new MainScreenData
            {
                AccountId = player.AccountId,
                EnergyData = new MainScreenEnergyData
                {
                    EnergyAmount = player.State.Persistent.Energy.GetPredictedAmount(now, configs.Energy),
                    NextRechargeIn = player.State.Persistent.Energy.GetTimeRemainingForNextRecharge(now, configs.Energy),
                },
                LevelData = new MainScreenLevelData
                {
                    CurrentLevel = currentLevel,
                    EnergyCost = levelConfig.EnergyCost,
                    EnergyReward = levelConfig.EnergyReward,
                    CanPlay = player.State.Persistent.Energy.GetPredictedAmount(now, configs.Energy) >= levelConfig.EnergyCost,
                },
            };
        }
    }
}