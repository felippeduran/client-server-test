using Presentation.Loading.Views;
using UnityEngine;
using UseCases.Runtime;

namespace Presentation.Loading.Screen
{
    public class LoadingScreen : MonoBehaviour, ILoadingPresenter
    {
        [SerializeField] private LoadingBarPresenter loadingBarPresenter;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            loadingBarPresenter.SetProgress(0);
        }

        public void SetProgress(float progress)
        {
            loadingBarPresenter.SetProgress(progress);
        }
    }
}
