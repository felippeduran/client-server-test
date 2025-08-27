using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Presentation.Stats.Screens
{
    public class StatsScreen : MonoBehaviour
    {
        [SerializeField] private Button closeButton;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public async UniTask OpenAsync(LevelStats[] stats, CancellationToken ct)
        {
            Show();
            await closeButton.OnClickAsync(ct);
            Hide();
        }
    }
}