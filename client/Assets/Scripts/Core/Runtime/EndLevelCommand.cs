using System;

[Serializable]
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

        if (!state.Persistent.LevelProgression.Statistics.TryGetValue(new LevelStats { LevelId = currentLevelId }, out var stats))
        {
            stats = new LevelStats
            {
                LevelId = currentLevelId,
                Wins = 0,
                Losses = 0,
                BestScore = 0,
            };
        }

        if (Success && Score > stats.BestScore)
        {
            stats.BestScore = Score;
        }
        stats.Wins += Success ? 1 : 0;
        stats.Losses += Success ? 0 : 1;

        state.Persistent.LevelProgression.Statistics.Remove(new LevelStats { LevelId = currentLevelId });
        state.Persistent.LevelProgression.Statistics.Add(stats);

        if (Success)
        {
            if (currentLevelId == state.Persistent.LevelProgression.CurrentLevel)
            {
                state.Persistent.LevelProgression.CurrentLevel = currentLevelId + 1;
            }
            state.Persistent.Energy.CurrentAmount += levelConfig.EnergyReward;
        }

        state.Session.CurrentLevelId = null;
    }
}