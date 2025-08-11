using TMPro;
using UnityEngine;

namespace Presentation.Stats.Views
{
    public class StatEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI winCountText;
        [SerializeField] private TextMeshProUGUI loseCountText;

        public void Init(int levelIndex, int winCount, int loseCount)
        {
            labelText.text = $"Level {levelIndex}";
            winCountText.text = $"{winCount}";
            loseCountText.text = $"{loseCount}";
        }
    }
}
