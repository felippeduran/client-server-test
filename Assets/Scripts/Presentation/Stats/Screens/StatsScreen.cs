using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Stats.Screens
{
    public class StatsScreen : MonoBehaviour
    {
        [SerializeField] private Button closeButton;
        
        private void OnEnable()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveAllListeners();
        }

        private void OnCloseButtonClicked()
        {
            
        }
    }
}