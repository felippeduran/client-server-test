using System;
using TMPro;
using UnityEngine;

namespace Presentation.Main.Presenters
{
    public class EnergyPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public void Setup(int energy)
        {
            UpdateText(energy);
        }

        private void UpdateText(int energyAmount)
        {
            labelText.text = $"{energyAmount}";
        }
    }
}