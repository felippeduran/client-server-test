using Presentation.Loading.Views;
using UnityEngine;

namespace Presentation.Loading.Screen
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private LoadingBarPresenter loadingBarPresenter;

        private void OnEnable()
        {
            loadingBarPresenter.SetProgress(0);
        }
    }
}
