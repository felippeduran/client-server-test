using System;
using System.Threading;
using System.Threading.Tasks;
using Networking.Runtime;
using Services.Runtime;
using Core.Runtime;
using Utilities.Runtime.Clock;
using Utilities.Runtime.Logging;

namespace Application.Runtime
{
    public class ConnectionHandler
    {
        public delegate void ProgressDelegate(float progress);

        readonly IClient client;
        readonly IAuthenticationService authenticationService;
        readonly ISynchronizationService synchronizationService;

        public ConnectionHandler(IClient client, IAuthenticationService authenticationService, ISynchronizationService synchronizationService)
        {
            this.client = client;
            this.authenticationService = authenticationService;
            this.synchronizationService = synchronizationService;
        }

        public async Task<((Player player, Configs configs, IClock clock), Error)> HandleAsync(Account account, ProgressDelegate setProgress, CancellationToken ct)
        {
            var connectError = await client.ConnectAsync(ct);
            if (connectError != null)
            {
                return (default, connectError);
            }

            setProgress(0.2f);
            Logger.Log("Connected");

            var authError = await authenticationService.AuthenticateAsync(account, ct);
            if (authError != null)
            {
                return (default, authError);
            }

            setProgress(0.5f);
            Logger.Log("Authenticated");

            void SetProgress(float progress)
            {
                setProgress(0.5f * progress + 0.5f);
            }

            var (synchronizedState, syncError) = await synchronizationService.SynchronizeAsync(SetProgress, ct);
            if (syncError != null)
            {
                Logger.LogWarning(syncError.Message);
                return (default, syncError);
            }

            setProgress(1f);

            await Task.Yield();

            Logger.Log("Synchronized. Time offset: " + (synchronizedState.ServerTime - DateTime.UtcNow).TotalMilliseconds + "ms");

            return ((new Player { AccountId = account.Id, State = synchronizedState.PlayerState }, synchronizedState.Configs, new OffsetClock(synchronizedState.ServerTime)), null);
        }
    }
}