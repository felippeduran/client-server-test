using System.Threading;
using System.Threading.Tasks;
using Core.Runtime;
using Utilities.Runtime.Clock;

namespace UseCases.Runtime
{
    public interface IGameplayUseCase
    {
        Task OpenGameplayAsync(int levelId, ICommandExecutor commandExecutor, IClock clock, Configs configs, CancellationToken ct);
    }
}