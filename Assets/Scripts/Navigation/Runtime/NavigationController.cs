using UnityEngine;
public class NavigationController
{
    private ScreensLibrary screensLibrary;

    public NavigationController(ScreensLibrary screensLibrary)
    {
        this.screensLibrary = screensLibrary;
    }

    public void OpenGameplayScreenAsync()
    {
        HideAllScreens();
        screensLibrary.gameplayScreen.Init(10, 6);
        screensLibrary.gameplayScreen.gameObject.SetActive(true);
        
    }

    public void OpenMainMenuScreen()
    {
        HideAllScreens();
        screensLibrary.mainMenuScreen.gameObject.SetActive(true);
    }

    public void OpenResultScreen()
    {
        HideAllScreens();
        screensLibrary.resultScreen.gameObject.SetActive(true);
    }

    public void OpenLoadingScreen()
    {
        HideAllScreens();
        screensLibrary.loadingScreen.gameObject.SetActive(true);
    }


    public void OpenStatsScreen()
    {
        HideAllScreens();
        screensLibrary.statsScreen.gameObject.SetActive(true);
    }

    public void HideAllScreens()
    {
        screensLibrary.gameplayScreen.gameObject.SetActive(false);
        screensLibrary.mainMenuScreen.gameObject.SetActive(false);
        screensLibrary.resultScreen.gameObject.SetActive(false);
        screensLibrary.loadingScreen.gameObject.SetActive(false);
        screensLibrary.statsScreen.gameObject.SetActive(false);
    }
}