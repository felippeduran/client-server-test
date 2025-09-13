using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public interface IResultsPresenter
    {
        Task<bool> ShowAsync(bool won, CancellationToken ct);
    }
}