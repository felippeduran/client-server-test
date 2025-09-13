using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Loading.Views
{
    public class LoadingBarPresenter : MonoBehaviour
    {
        [SerializeField] private Slider slider;

        public void SetProgress(float value)
        {
            slider.value = value;
        }
    }
}