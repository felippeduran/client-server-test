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
        
        public int CurrentLevelIndex { get; private set; }

        private int levelCount;
        
        private void Awake()
        {
            levelCount = 1;
            CurrentLevelIndex = 0;
            UpdateText();
        }
        
        private void OnEnable()
        {
            leftButton.onClick.AddListener(OnLeftButtonClicked);
            rightButton.onClick.AddListener(OnRightButtonClicked);
        }

        private void OnDisable()
        {
            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners(); 
        }

        private void UpdateText()
        {
            labelText.text = $"Level {CurrentLevelIndex + 1}";
        }
        
        private void OnLeftButtonClicked()
        {
            CurrentLevelIndex = (CurrentLevelIndex + levelCount - 1) % levelCount;
            UpdateText();
        }
        
        private void OnRightButtonClicked()
        {
            CurrentLevelIndex = (CurrentLevelIndex + 1) % levelCount;
            UpdateText();
        }
    }
}