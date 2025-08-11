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

        public int CurrentDiceNumber => currentDiceFace + 1;

        private float idleRollingTimer;
        private float faceChangeTimer;
        private int currentDiceFace;

        private void Awake()
        {
            currentDiceFace = 0;
            diceImage.sprite = diceFaceSprites[currentDiceFace];
        }

        public async Task Roll(int targetFace)
        {
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
