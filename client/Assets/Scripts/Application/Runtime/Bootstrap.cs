using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using UseCases.Runtime;
using Networking.Runtime;
using Services.Runtime;
using Core.Runtime;
using Utilities.Runtime.Logging.Unity;
using Logger = Utilities.Runtime.Logging.Logger;

namespace Application.Runtime
{
    [Serializable]
    public struct AppConfig
    {
        public string BaseUrl;
        public bool UseThrottling;
        public ThrottlingConfig Throttling;
        public bool UseFakeServer;
    }

    [Serializable]
    public struct ThrottlingConfig
    {
        public double FailureRate;
        public double RTTSeconds;
    }

    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] ScreensLibrary screensLibrary;
        [SerializeField] AppConfig config;

        private async void Start()
        {
            Logger.Instance = new UnityLogger();
            Logger.Log("Starting Bootstrap");

            IClient client = new ClientFactory().CreateClient(config);

            var commandService = new CommandService(client);
            var authenticationService = new AuthenticationService(client);
            var synchronizationService = new SynchronizationService(client);
            var connectionHandler = new ConnectionHandler(client, authenticationService, synchronizationService);

            var accountLoader = new AccountLoader();
            var account = accountLoader.Load();

            var ct = UnityEngine.Application.exitCancellationToken;
            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                screensLibrary.Loading.Show();

                void SetProgress(float progress)
                {
                    screensLibrary.Loading.SetProgress(progress);
                }

                var (synchronized, error) = await connectionHandler.HandleAsync(account, SetProgress, cts.Token);
                if (error != null)
                {
                    client.Disconnect();
                    await screensLibrary.errorScreen.ShowAsync(error.Message, "Try again", cts.Token);
                    continue;
                }

                screensLibrary.Loading.Hide();

                var executionQueue = new ExecutionQueue(synchronized.player.State, synchronized.configs);

                var commandsHandler = new CommandsHandler(executionQueue, commandService);
                var commandsTask = commandsHandler.HandleAsync(cts.Token);

                var coreLoopUseCase = new CoreLoopUseCase(new MainMenuUseCase(screensLibrary), new GameplayUseCase(screensLibrary));

                var coreLoopContext = new Context
                {
                    CommandExecutor = executionQueue,
                    PlayerState = synchronized.player,
                    Clock = synchronized.clock,
                    Configs = synchronized.configs,
                };

                async Task<Error> HandleCoreLoopAsync(Context context, CancellationToken ct)
                {
                    await coreLoopUseCase.HandleCoreLoopAsync(context, ct);
                    return null;
                }

                var coreLoopTask = HandleCoreLoopAsync(coreLoopContext, cts.Token);

                var completedTask = await Task.WhenAny(commandsTask, coreLoopTask);

                if (completedTask.IsFaulted && !cts.IsCancellationRequested)
                {
                    client.Disconnect();
                    Logger.LogError(completedTask.Exception.Message);
                    screensLibrary.Loading.Show();
                    await screensLibrary.errorScreen.ShowAsync(completedTask.Exception.Message, "Try again", cts.Token);
                }
                cts.Cancel();
            }
        }
    }
}