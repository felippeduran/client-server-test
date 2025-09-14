using System.Threading;
using System.Threading.Tasks;
using Core.Runtime;
using Utilities.Runtime.Clock;

namespace UseCases.Runtime
{
    public struct Context
    {
        public ICommandExecutor CommandExecutor;
        public IReadOnlyPlayer PlayerState;
        public IClock Clock;
        public Configs Configs;
    }

    public class CoreLoopUseCase
    {
        readonly IMainMenuUseCase mainMenuUseCase;
        readonly IGameplayUseCase gameplayUseCase;

        public CoreLoopUseCase(IMainMenuUseCase mainMenuUseCase, IGameplayUseCase gameplayUseCase)
        {
            this.mainMenuUseCase = mainMenuUseCase;
            this.gameplayUseCase = gameplayUseCase;
        }

        public async Task HandleCoreLoopAsync(Context context, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var play = await mainMenuUseCase.OpenMainMenuAsync(context.PlayerState, context.Clock, context.Configs, cts.Token);

                if (play != null)
                {
                    await gameplayUseCase.OpenGameplayAsync(play.Level, context.CommandExecutor, context.Clock, context.Configs, cts.Token);
                }
            }
        }
    }
}