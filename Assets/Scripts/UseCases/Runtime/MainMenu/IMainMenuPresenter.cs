using System;
using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public interface IMainMenuPresenter
    {
        void Show();
        void Hide();
        void Setup(MainScreenData data);
        Task<Actions> WaitForInputAsync(CancellationToken ct);
    }

    [Serializable]
    public struct MainScreenData
    {
        public string AccountId;
        public MainScreenEnergyData EnergyData;
        public MainScreenLevelData LevelData;
    }

    [Serializable]
    public struct MainScreenEnergyData
    {
        public int EnergyAmount;
        public TimeSpan NextRechargeIn;
    }

    [Serializable]
    public struct MainScreenLevelData
    {
        public int CurrentLevel;
        public int EnergyCost;
        public int EnergyReward;
        public bool CanPlay;
    }

    [Serializable]
    public struct Actions
    {
        public bool OpenStats;
        public int ChangeLevelDirection;
        public bool Refresh;
        public bool Play;
    }
}