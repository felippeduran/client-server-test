using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Presentation.Gameplay.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UseCases.Runtime;

namespace Presentation.Gameplay.Screen
{
    public interface IGameplay
    {
        public int MaxRolls { get; }
    }

    public class Gameplay
    {
        public int MaxRolls { get; private set; }
        public int TargetNumber { get; private set; }
        public int RemainingRolls { get; private set; }

        public Gameplay(int maxRolls, int targetNumber)
        {
            MaxRolls = maxRolls;
            TargetNumber = targetNumber;
            RemainingRolls = maxRolls;
        }

        public void ConsumeRoll()
        {
            RemainingRolls--;
        }
    }

    public class GameplayScreen : MonoBehaviour, IGameplayPresenter
    {
        [SerializeField] private Button rollButton;
        [SerializeField] private RollCounterPresenter rollCounterPresenter;
        [SerializeField] private TargetNumberPresenter targetNumberPresenter;
        [SerializeField] private DicePresenter dicePresenter;

        private void OnEnable()
        {
            rollButton.onClick.AddListener(OnRollButtonClicked);
        }

        private void OnDisable()
        {
            rollButton.onClick.RemoveAllListeners();
        }

        public void Init(int maxRolls, int targetNumber)
        {
            rollCounterPresenter.Init(maxRolls);
            targetNumberPresenter.Init(targetNumber);
            dicePresenter.Init();
            rollButton.interactable = true;
        }

        public async Task<GameplayResult> ShowAsync(int maxRolls, int targetNumber, CancellationToken ct)
        {
            Init(maxRolls, targetNumber);
            gameObject.SetActive(true);

            var victoryTask = WaitForVictoryAsync(ct);
            var lossTask = WaitForLossAsync(ct);

            var (_, won, _) = await UniTask.WhenAny(victoryTask, lossTask);

            rollButton.interactable = false;

            await Task.Delay(TimeSpan.FromSeconds(0.7f));

            gameObject.SetActive(false);
            return new GameplayResult
            {
                Won = won,
                Score = rollCounterPresenter.RemainingRolls,
            };
        }

        private async UniTask<bool> WaitForVictoryAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(HasWon, cancellationToken: ct);
            return true;
        }

        private async UniTask<bool> WaitForLossAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(HasLost, cancellationToken: ct);
            return true;
        }

        private bool HasWon()
        {
            return !dicePresenter.Rolling && dicePresenter.CurrentDiceNumber == targetNumberPresenter.TargetNumber;
        }

        private bool HasLost()
        {
            return !dicePresenter.Rolling && rollCounterPresenter.RemainingRolls <= 0;
        }

        private async void OnRollButtonClicked()
        {
            rollCounterPresenter.ConsumeRoll();

            rollButton.interactable = false;
            await dicePresenter.Roll(Random.Range(0, 6));
            rollButton.interactable = true;
        }
    }
}
