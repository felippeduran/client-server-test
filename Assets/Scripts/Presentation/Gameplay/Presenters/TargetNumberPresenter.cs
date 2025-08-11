using TMPro;
using UnityEngine;

namespace Presentation.Gameplay.Presenters
{
    public class TargetNumberPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public int TargetNumber { get; private set; }
        
        public void Init(int value)
        {
            TargetNumber = value;
            labelText.text = $"{value}";
        }
    }
}