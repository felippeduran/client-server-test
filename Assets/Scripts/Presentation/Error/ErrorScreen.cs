using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Error.Screen
{
    public class ErrorScreen : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text errorLabel;
        [SerializeField] private TMP_Text buttonLabel;

        public async UniTask ShowAsync(string error, string buttonLabel, CancellationToken ct)
        {
            errorLabel.text = error;
            this.buttonLabel.text = buttonLabel;
            gameObject.SetActive(true);
            await closeButton.OnClickAsync(ct);
            gameObject.SetActive(false);
        }
    }
}