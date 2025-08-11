using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Results.Screen
{
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        
        private void OnEnable()
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        private void OnDisable()
        {
            continueButton.onClick.RemoveAllListeners();
        }

        private void OnContinueButtonClicked()
        {
            
        }
    }
}
