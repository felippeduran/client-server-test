using Presentation.Main.Presenters;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Main.Screen
{
    public class MainScreen : MonoBehaviour
    {
        [SerializeField] private Button statsButton;
        [SerializeField] private Button playButton;
        [SerializeField] private LevelSelectionPresenter levelSelectionPresenter;
        [SerializeField] private EnergyPresenter energyPresenter;

        private void OnEnable()
        {
            statsButton.onClick.AddListener(OnStatsButtonClicked);
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        private void OnDisable()
        {
            statsButton.onClick.RemoveAllListeners();
            playButton.onClick.RemoveAllListeners(); 
        }

        private void OnStatsButtonClicked()
        {
            
        }
        
        private void OnPlayButtonClicked()
        {
            // levelSelectionPresenter.CurrentLevelIndex
        }
    }
}
