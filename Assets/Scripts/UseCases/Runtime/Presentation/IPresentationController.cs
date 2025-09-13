using UseCases.Runtime;

public interface IScreenLibrary
{
    public IGameplayPresenter Gameplay { get; }
    public IMainMenuPresenter MainMenu { get; }
    public IStatsPresenter Stats { get; }
    public IResultsPresenter Results { get; }
    // public ILoadingPresenter Loading { get; }
    public IErrorPresenter Error { get; }
}