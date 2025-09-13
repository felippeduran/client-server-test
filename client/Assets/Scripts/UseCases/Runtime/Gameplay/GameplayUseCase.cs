using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public class GameplayUseCase : IGameplayUseCase
    {
        readonly IScreenLibrary screenLibrary;

        public GameplayUseCase(IScreenLibrary screenLibrary)
        {
            this.screenLibrary = screenLibrary;
        }
        
        public async Task OpenGameplayAsync(int levelId, ICommandExecutor commandExecutor, IClock clock, Configs configs, CancellationToken ct)
        {
            commandExecutor.Execute(new BeginLevelCommand
            {
                LevelId = levelId,
                Now = clock.Now(),
            });
            // Debug.Log($"Local Persistent state after: {player.State.Persistent.LevelProgression.Statistics.Count}");
            // Debug.Log($"Local session state after: {player.State.Session.CurrentLevelId}");

            var levelConfig = configs.Levels[levelId];
            var result = await screenLibrary.Gameplay.ShowAsync(levelConfig.MaxRolls, levelConfig.TargetNumber, ct);

            commandExecutor.Execute(new EndLevelCommand
            {
                Success = result.Won,
                Score = result.Score,
            });

            // Debug.Log($"Local session state after: {player.State.Session.CurrentLevelId}");
            // Debug.Log($"Local Persistent state after: {player.State.Persistent.LevelProgression.Statistics.Count}");

            await screenLibrary.Results.ShowAsync(result.Won, ct);
        }
    }
}