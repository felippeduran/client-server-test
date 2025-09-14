using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Presentation.Stats.Views;
using UseCases.Runtime;

namespace Presentation.Stats.Screens
{
    public class StatsScreen : MonoBehaviour, IStatsPresenter
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private StatEntryView statEntryViewPrefab;
        [SerializeField] private Transform contentTransform;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        public async Task OpenAsync(IEnumerable<LevelStats> stats, CancellationToken ct)
        {
            Show();
            Setup(stats);
            await closeButton.OnClickAsync(ct).SuppressCancellationThrow();
            Hide();
        }

        void Setup(IEnumerable<LevelStats> stats)
        {
            foreach (var stat in stats)
            {
                var view = Instantiate(statEntryViewPrefab, contentTransform);
                view.Init(stat.LevelId, stat.Wins, stat.Losses, stat.BestScore);
            }
        }
    }
}