using System;
using Presentation.Gameplay.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Presentation.Gameplay.Screen
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button rollButton;
        [SerializeField] private RollCounterPresenter rollCounterPresenter;
        [SerializeField] private TargetNumberPresenter targetNumberPresenter;
        [SerializeField] private DicePresenter dicePresenter;

        private void Awake()
        {
            Init(10, 6);
        }

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
        }

        private async void OnRollButtonClicked()
        {
            rollCounterPresenter.ConsumeRoll();
            
            rollButton.interactable = false;
            await dicePresenter.Roll(Random.Range(0, 6));
            rollButton.interactable = true;

            if (dicePresenter.CurrentDiceNumber == targetNumberPresenter.TargetNumber)
            {
                Debug.Log("WIN");
            }
            else if (rollCounterPresenter.RemainingRolls <= 0)
            {
                Debug.Log("LOSE");
            }
        }
    }
}
