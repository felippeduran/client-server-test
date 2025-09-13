using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public readonly struct GameplayResult
    {
        public bool Won { get; init; }
        public int Score { get; init; }
    }

    public interface IGameplayPresenter
    {
        Task<GameplayResult> ShowAsync(int maxRolls, int targetNumber, CancellationToken ct);
    }
}