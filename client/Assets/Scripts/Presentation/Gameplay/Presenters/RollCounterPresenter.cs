using TMPro;
using UnityEngine;

namespace Presentation.Gameplay.Presenters
{
    public class RollCounterPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public int RemainingRolls { get; private set; }

        public void Init(int maxRolls)
        {
            RemainingRolls = maxRolls;
            UpdateText();
        }

        public void ConsumeRoll()
        {
            RemainingRolls--;
            UpdateText();
        }

        private void UpdateText()
        {
            labelText.text = $"{RemainingRolls}";
        }
    }
}