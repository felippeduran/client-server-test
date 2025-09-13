using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using UseCases.Runtime;

namespace Application.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] ScreensLibrary screensLibrary;
        [SerializeField] ConnectivityClientDecorator.Config connectivityConfig;

        private async void Start()
        {
            Logger.Instance = new UnityLogger();
            Logger.Log("Starting Bootstrap");

            var accountStorage = new InMemoryAccounts();
            var server = FakeServerFactory.CreateServer<ConnectionState>(new object[] {
                new AuthenticationHandler(accountStorage),
                new CommandHandler(new CommandHandler.Config { MaxTimeDifferenceMilliseconds = 1000 }, accountStorage),
                new InitializationHandler(accountStorage),
            });

            var client = new ConnectivityClientDecorator(connectivityConfig, new FakeClient(server));

            var commandService = new CommandService(client);
            var authenticationService = new AuthenticationService(client);
            var synchronizationService = new SynchronizationService(client);
            var connectionHandler = new ConnectionHandler(client, authenticationService, synchronizationService);

            var account = new Account { Id = Guid.NewGuid().ToString(), AccessToken = Guid.NewGuid().ToString() };

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

                var coreLoopContext = new Context {
                    CommandExecutor = executionQueue,
                    PlayerState = synchronized.player,
                    Clock = synchronized.clock,
                    Configs = synchronized.configs,
                };

                var coreLoopTask = coreLoopUseCase.HandleCoreLoopAsync(coreLoopContext, cts.Token).ContinueWith<Error>(t => null);

                var completedTask = await Task.WhenAny(commandsTask, coreLoopTask);

                if (completedTask.Result != null)
                {
                    client.Disconnect();
                    await screensLibrary.errorScreen.ShowAsync(completedTask.Result.Message, "Try again", cts.Token);
                }
            }
        }
    }
}