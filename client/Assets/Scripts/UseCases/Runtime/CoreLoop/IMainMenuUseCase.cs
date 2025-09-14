using System.Threading;
using System.Threading.Tasks;
using Core.Runtime;
using Utilities.Runtime.Clock;

namespace UseCases.Runtime
{
    public interface IMainMenuUseCase
    {
        Task<Play> OpenMainMenuAsync(IReadOnlyPlayer player, IClock clock, Configs configs, CancellationToken ct);
    }

    public class Play
    {
        public int Level;
    }
}