using UnityEngine;
using Presentation.Gameplay.Screen;
using Presentation.Main.Screen;
using Presentation.Results.Screen;
using Presentation.Loading.Screen;
using Presentation.Stats.Screens;
using Presentation.Error.Screen;
using UseCases.Runtime;

public class ScreensLibrary : MonoBehaviour, IScreenLibrary
{
    public GameplayScreen gameplayScreen;
    public MainScreen mainMenuScreen;
    public ResultsScreen resultScreen;
    public LoadingScreen loadingScreen;
    public StatsScreen statsScreen;
    public ErrorScreen errorScreen;

    public IMainMenuPresenter MainMenu => mainMenuScreen;
    public IStatsPresenter Stats => statsScreen;
    public IErrorPresenter Error => errorScreen;
    public IGameplayPresenter Gameplay => gameplayScreen;
    public IResultsPresenter Results => resultScreen;
    public ILoadingPresenter Loading => loadingScreen;
}