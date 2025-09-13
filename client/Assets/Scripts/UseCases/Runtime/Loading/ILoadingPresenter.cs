namespace UseCases.Runtime
{
    public interface ILoadingPresenter
    {
        void Show();
        void Hide();
        void SetProgress(float progress);
    }
}