using System.Threading;
using System.Threading.Tasks;
using Presentation.Main.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;

[Serializable]
public struct MainScreenData
{
    public string AccountId;
    public int CurrentLevel;
    public int EnergyAmount;
    public int MaxLevel;
    public bool CanPlay;
}

namespace Presentation.Main.Screen
{
    public class MainScreen : MonoBehaviour
    {
        public class Play
        {
            public int Level;
        }

        [SerializeField] private TMP_Text accountIdText;
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
            levelSelectionPresenter.Setup(data.CurrentLevel, data.MaxLevel);
            energyPresenter.Setup(data.EnergyAmount);
        }

        public async Task<(bool, Play, bool)> WaitForInputAsync(CancellationToken ct)
        {
            var statsButtonTask = WaitForStatsButtonClickAsync(ct);
            var playButtonTask = WaitForPlayButtonClickAsync(ct);
            var refreshTask = WaitForRefreshAsync(ct);

            var (_, openStats, play, refresh) = await UniTask.WhenAny(statsButtonTask, playButtonTask, refreshTask);

            if (openStats)
            {
                return (true, null, false);
            }

            if (play)
            {
                return (false, new Play { Level = levelSelectionPresenter.CurrentLevelIndex }, false);
            }

            if (refresh)
            {
                return (false, null, true);
            }

            return (false, null, false);
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
