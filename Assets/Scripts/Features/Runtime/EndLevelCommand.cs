public class EndLevelCommand : ICommand
{
    public bool Success { get; set; }
    public int Score { get; set; }

    public void Execute(PlayerState state, Configs configs)
    {
        if (state.Session.CurrentLevelId is not int currentLevelId)
        {
            throw new MetagameException("no level in progress");
        }

        var levelConfig = configs.Levels[currentLevelId];

        var stats = new LevelStats { LevelId = currentLevelId };
        if (state.Persistent.LevelProgression.Statistics.TryGetValue(stats, out stats))
        {
            state.Persistent.LevelProgression.Statistics.Remove(stats);
        }

        if (Success && Score > stats.BestScore)
        {
            stats.BestScore = Score;
        }
        stats.Wins += Success ? 1 : 0;
        stats.Losses += Success ? 0 : 1;

        state.Persistent.LevelProgression.Statistics.Add(stats);

        if (Success)
        {
            state.Persistent.LevelProgression.CurrentLevel = currentLevelId + 1;
            state.Persistent.Energy.CurrentAmount += levelConfig.EnergyReward;
        }

        state.Session.CurrentLevelId = null;
    }
}