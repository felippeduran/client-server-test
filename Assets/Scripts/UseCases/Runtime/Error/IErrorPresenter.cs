using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UseCases.Runtime
{
    public interface IErrorPresenter
    {
        Task ShowAsync(string error, string buttonLabel, CancellationToken ct);
    }
}