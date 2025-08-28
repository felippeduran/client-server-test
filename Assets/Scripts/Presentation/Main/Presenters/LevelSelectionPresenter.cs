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

        public void Setup(int currentLevel, int levelCount)
        {
            // Ensure currentLevel is at least 1 and within valid range
            CurrentLevelIndex = Mathf.Clamp(currentLevel, 1, levelCount);
            this.levelCount = levelCount;
            UpdateText();
        }
        
        private void Awake()
        {
            levelCount = 1;
            CurrentLevelIndex = 1; // Start at level 1 instead of 0
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
            // CurrentLevelIndex is now one-based, so no need to add 1
            labelText.text = $"Level {CurrentLevelIndex}";
        }
        
        private void OnLeftButtonClicked()
        {
            // Cycle to previous level, ensuring we never go below 1
            CurrentLevelIndex = CurrentLevelIndex == 1 ? levelCount : CurrentLevelIndex - 1;
            UpdateText();
        }
        
        private void OnRightButtonClicked()
        {
            // Cycle to next level, ensuring we cycle back to 1 when exceeding levelCount
            CurrentLevelIndex = CurrentLevelIndex == levelCount ? 1 : CurrentLevelIndex + 1;
            UpdateText();
        }
    }
}