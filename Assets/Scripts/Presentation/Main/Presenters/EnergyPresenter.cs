using TMPro;
using UnityEngine;

namespace Presentation.Main.Presenters
{
    public class EnergyPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public int Energy { get; private set; }
        

        public void GainEnergy(int value)
        {
            Energy += value;
            UpdateText();
        }
        
        public void ConsumeEnergy(int value)
        {
            Energy -= value;
            UpdateText();
        }

        private void UpdateText()
        {
            labelText.text = $"{Energy}";
        }
    }
}