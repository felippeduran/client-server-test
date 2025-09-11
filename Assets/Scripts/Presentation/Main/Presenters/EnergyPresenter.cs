using System;
using TMPro;
using UnityEngine;

namespace Presentation.Main.Presenters
{
    public class EnergyPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI labelNextInText;

        public void Setup(int energy, TimeSpan timeRemaining)
        {
            labelText.text = $"{energy}";
            labelNextInText.gameObject.SetActive(timeRemaining > TimeSpan.Zero);
            if (timeRemaining > TimeSpan.Zero)
            {
                labelNextInText.text = $"Next in {Math.Ceiling(timeRemaining.TotalSeconds):F0}s";
            }
        }
    }
}