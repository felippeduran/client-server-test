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

            var levelConfig = configs.Levels[levelId];
            var result = await screenLibrary.Gameplay.ShowAsync(levelConfig.MaxRolls, levelConfig.TargetNumber, ct);
            if (result.Completed)
            {
                commandExecutor.Execute(new EndLevelCommand
                {
                    Success = result.Won,
                    Score = result.Score,
                });

                await screenLibrary.Results.ShowAsync(result.Won, ct);
            }
        }
    }
}