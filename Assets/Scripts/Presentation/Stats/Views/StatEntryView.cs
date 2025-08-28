using TMPro;
using UnityEngine;

namespace Presentation.Stats.Views
{
    public class StatEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI winCountText;
        [SerializeField] private TextMeshProUGUI loseCountText;
        [SerializeField] private TextMeshProUGUI bestText;

        public void Init(int levelIndex, int winCount, int loseCount, int bestScore)
        {
            labelText.text = $"Level {levelIndex}";
            winCountText.text = $"{winCount}";
            loseCountText.text = $"{loseCount}";
            bestText.text = $"{bestScore}";
        }
    }
}
