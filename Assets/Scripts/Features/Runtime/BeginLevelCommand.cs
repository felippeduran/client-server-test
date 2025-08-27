public class BeginLevelCommand : ICommand
{
    public int LevelId { get; set; }

    public void Execute(PlayerState state, Configs configs)
    {
        if (!state.Persistent.LevelProgression.CanPlayLevel(LevelId))
        {
            throw new MetagameException("level not unlocked");
        }

        if (state.Persistent.Energy.CurrentAmount < configs.Levels[LevelId].EnergyCost)
        {
            throw new MetagameException("not enough energy");
        }

        var levelConfig = configs.Levels[LevelId];

        state.Persistent.Energy.CurrentAmount -= levelConfig.EnergyCost;
        state.Session.CurrentLevelId = LevelId;
    }
}