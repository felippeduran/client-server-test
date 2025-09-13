using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Gameplay.Presenters
{
    public class DicePresenter : MonoBehaviour
    {
        [SerializeField] private Image diceImage;
        [SerializeField] private Sprite[] diceFaceSprites;

        [SerializeField] private float idleRollingSeconds;
        [SerializeField] private float faceChangeSeconds;

        public int CurrentDiceNumber { get; private set; }
        public bool Rolling { get; private set; }

        private float idleRollingTimer;
        private float faceChangeTimer;
        private int currentDiceFace;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            currentDiceFace = 0;
            diceImage.sprite = diceFaceSprites[currentDiceFace];
            CurrentDiceNumber = -1;
        }

        public async Task Roll(int targetFace)
        {
            Rolling = true;
            idleRollingTimer = idleRollingSeconds;
            faceChangeTimer = faceChangeSeconds;

            while (idleRollingTimer > 0)
            {
                NextDiceFace();
                idleRollingTimer -= Time.deltaTime;
                await Task.Yield();
            }

            while (currentDiceFace != targetFace)
            {
                NextDiceFace();
                await Task.Yield();
            }

            CurrentDiceNumber = currentDiceFace + 1;
            Rolling = false;
        }

        private void NextDiceFace()
        {
            faceChangeTimer -= Time.deltaTime;
            if (faceChangeTimer <= 0f)
            {
                currentDiceFace = (currentDiceFace + 1) % diceFaceSprites.Length;
                diceImage.sprite = diceFaceSprites[currentDiceFace];
                faceChangeTimer += faceChangeSeconds;
            }
        }
    }
}
