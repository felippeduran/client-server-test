using System;

public class BeginLevelCommand : ITimedCommand
{
    public int LevelId { get; set; }
    public DateTime Now { get; set; }

    public void Execute(PlayerState state, Configs configs)
    {
        if (!state.Persistent.LevelProgression.CanPlayLevel(LevelId))
        {
            throw new MetagameException("level not unlocked");
        }

        if (state.Persistent.Energy.GetPredictedAmount(Now, configs.Energy) < configs.Levels[LevelId].EnergyCost)
        {
            throw new MetagameException("not enough energy");
        }

        var levelConfig = configs.Levels[LevelId];

        state.Persistent.Energy.UpdateEnergy(Now, configs.Energy);
        state.Persistent.Energy.CurrentAmount -= levelConfig.EnergyCost;
        state.Session.CurrentLevelId = LevelId;
    }
}