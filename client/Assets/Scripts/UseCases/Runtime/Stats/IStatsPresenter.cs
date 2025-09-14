using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Runtime;

namespace UseCases.Runtime
{
    public interface IStatsPresenter
    {
        Task OpenAsync(IEnumerable<LevelStats> stats, CancellationToken ct);
    }
}