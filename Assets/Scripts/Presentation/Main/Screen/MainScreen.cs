using System.Threading;
using System.Threading.Tasks;
using Presentation.Main.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using TMPro;

namespace Presentation.Main.Screen
{
    public class MainScreen : MonoBehaviour
    {
        [Serializable]
        public struct MainScreenData
        {
            public string AccountId;
            public int Energy;
            public int CurrentLevel;
            public int MaxLevel;
        }

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
            energyPresenter.Setup(data.Energy);
        }

        public async Task<(bool, Play)> WaitForInputAsync(CancellationToken ct)
        {
            var statsButtonTask = WaitForStatsButtonClickAsync(ct);
            var playButtonTask = WaitForPlayButtonClickAsync(ct);

            var (_, openStats, play) = await UniTask.WhenAny(statsButtonTask, playButtonTask);

            if (openStats)
            {
                return (true, null);
            }

            return (false, new Play { Level = levelSelectionPresenter.CurrentLevelIndex });
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
