using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Main.Presenters
{
    public class LevelSelectionPresenter : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TMP_Text rewardText;

        public void Setup(int currentLevel, int reward)
        {
            labelText.text = $"Level {currentLevel}";
            rewardText.text = $"Reward: {reward} Energy";
        }
        
        public async UniTask<int> WaitForLevelSelectionAsync(CancellationToken ct)
        {
            var leftButtonTask = WaitForLeftButtonClickAsync(ct);
            var rightButtonTask = WaitForRightButtonClickAsync(ct);

            var (_, leftButtonClicked, rightButtonClicked) = await UniTask.WhenAny(leftButtonTask, rightButtonTask);

            var selection = 0;

            if (leftButtonClicked)
            {
                selection = -1;
            }

            if (rightButtonClicked)
            {
                selection = 1;
            }

            return selection;
        }

        private async UniTask<bool> WaitForLeftButtonClickAsync(CancellationToken ct)
        {
            await leftButton.OnClickAsync(ct);
            return true;
        }

        private async UniTask<bool> WaitForRightButtonClickAsync(CancellationToken ct)
        {
            await rightButton.OnClickAsync(ct);
            return true;
        }
    }
}