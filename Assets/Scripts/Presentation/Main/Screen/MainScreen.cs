using System.Threading;
using System.Threading.Tasks;
using Presentation.Main.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UseCases.Runtime;

namespace Presentation.Main.Screen
{
    public class MainScreen : MonoBehaviour, IMainMenuPresenter
    {
        [SerializeField] private TMP_Text accountIdText;
        [SerializeField] private TMP_Text energyCostText;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button playButton;
        [SerializeField] private LevelSelectionPresenter levelSelectionPresenter;
        [SerializeField] private EnergyPresenter energyPresenter;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Setup(MainScreenData data)
        {
            accountIdText.text = data.AccountId;
            energyCostText.text = $"{data.LevelData.EnergyCost} Energy";
            levelSelectionPresenter.Setup(data.LevelData.CurrentLevel, data.LevelData.EnergyReward);
            energyPresenter.Setup(data.EnergyData.EnergyAmount, data.EnergyData.NextRechargeIn);
            playButton.interactable = data.LevelData.CanPlay;
        }

        public async Task<Actions> WaitForInputAsync(CancellationToken ct)
        {
            var statsButtonTask = WaitForStatsButtonClickAsync(ct);
            var playButtonTask = WaitForPlayButtonClickAsync(ct);
            var refreshTask = WaitForRefreshAsync(ct);
            var levelSelectionTask = levelSelectionPresenter.WaitForLevelSelectionAsync(ct);

            var (_, openStats, play, refresh, levelSelection) = await UniTask.WhenAny(statsButtonTask, playButtonTask, refreshTask, levelSelectionTask);

            if (openStats)
            {
                return new Actions { OpenStats = true };
            }

            if (levelSelection != 0)
            {
                return new Actions { ChangeLevelDirection = levelSelection };
            }

            if (play)
            {
                return new Actions { Play = true };
            }

            if (refresh)
            {
                return new Actions { Refresh = true };
            }

            return new Actions { };
        }

        async UniTask<bool> WaitForRefreshAsync(CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
            return true;
        }

        async UniTask<bool> WaitForStatsButtonClickAsync(CancellationToken ct)
        {
            await statsButton.OnClickAsync(ct);
            return true;
        }

        async UniTask<bool> WaitForPlayButtonClickAsync(CancellationToken ct)
        {
            await playButton.OnClickAsync(ct);
            return true;
        }
    }
}
