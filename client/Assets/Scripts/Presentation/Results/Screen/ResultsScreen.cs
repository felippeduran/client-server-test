using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UseCases.Runtime;

namespace Presentation.Results.Screen
{
    public class ResultsScreen : MonoBehaviour, IResultsPresenter
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text resultText;

        public async Task<bool> ShowAsync(bool won, CancellationToken ct)
        {
            resultText.text = won ? "WIN" : "LOSE";

            gameObject.SetActive(true);
            await continueButton.OnClickAsync(ct).SuppressCancellationThrow();
            gameObject.SetActive(false);
            return true;
        }
    }
}
