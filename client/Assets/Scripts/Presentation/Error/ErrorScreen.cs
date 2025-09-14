using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UseCases.Runtime;

namespace Presentation.Error.Screen
{
    public class ErrorScreen : MonoBehaviour, IErrorPresenter
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text errorLabel;
        [SerializeField] private TMP_Text buttonLabel;

        public async Task ShowAsync(string error, string buttonLabel, CancellationToken ct)
        {
            errorLabel.text = error;
            this.buttonLabel.text = buttonLabel;
            gameObject.SetActive(true);
            await closeButton.OnClickAsync(ct).SuppressCancellationThrow();
            gameObject.SetActive(false);
        }
    }
}