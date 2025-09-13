using System.Threading;
using System.Threading.Tasks;

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