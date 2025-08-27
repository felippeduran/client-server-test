// public interface IGameplayProvider
// {
//     public LevelData BeginLevel(int levelId);
//     public void CompleteLevel(LevelData levelData);
// }

public struct LevelResults
{
    public bool Won;
    public Rewards Rewards;
}

public struct Rewards
{
    public int EnergyAmount;
}