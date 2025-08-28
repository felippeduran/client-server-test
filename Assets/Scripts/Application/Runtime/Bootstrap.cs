using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Presentation.Main.Screen;
using System.Linq;
using System;

namespace Application.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] ScreensLibrary screensLibrary;
        [SerializeField] ConnectivityClientDecorator.Config connectivityConfig;

        private async void Start()
        {
            Debug.Log("Starting Bootstrap");

            var accountStorage = new InMemoryAccounts();
            var server = FakeServerFactory.CreateServer<ConnectionState>(new object[] {
                new AuthenticationHandler(accountStorage),
                new CommandHandler(accountStorage),
                new InitializationHandler(accountStorage),
            });

            var client = new ConnectivityClientDecorator(connectivityConfig, new FakeClient(server));

            var account = new Account { Id = Guid.NewGuid().ToString(), AccessToken = Guid.NewGuid().ToString() };

            var ct = UnityEngine.Application.exitCancellationToken;
            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                screensLibrary.loadingScreen.gameObject.SetActive(true);

                var (synchronized, error) = await HandleConnectionAsync(account, client, cts.Token);
                if (error != null)
                {
                    client.Disconnect();
                    await screensLibrary.errorScreen.ShowAsync(error.Message, cts.Token);
                    continue;
                }

                screensLibrary.loadingScreen.SetProgress(1);
                screensLibrary.loadingScreen.gameObject.SetActive(false);

                var executionQueue = new ExecutionQueue(synchronized.player.State, synchronized.configs);

                var commandsTask = HandleCommandsAsync(executionQueue, client, cts.Token);
                var gameLoopTask = HandleGameLoopAsync(executionQueue, synchronized.player, synchronized.configs, cts.Token);

                var completedTask = await Task.WhenAny(commandsTask, gameLoopTask);

                if (completedTask.Result != null)
                {
                    client.Disconnect();
                    await screensLibrary.errorScreen.ShowAsync(completedTask.Result.Message, cts.Token);
                }
            }
        }
        
        private async Task<((Player player, Configs configs), Error)> HandleConnectionAsync(Account account, IClient client, CancellationToken ct)
        {
            var error = await client.ConnectAsync(ct);
            if (error != null)
            {
                return (default, error);
            }
            Debug.Log("Connected");

            var metagameClient = new MetagameClient(client);

            error = await metagameClient.AuthenticateAsync(account, ct);
            if (error != null)
            {
                return (default, error);
            }
            Debug.Log("Authenticated");

            PlayerState state;
            Configs configs;
            ((state, configs), error) = await metagameClient.SynchronizeAsync(ct);
            if (error != null)
            {
                Debug.LogError(error.Message);
                return (default, error);
            }

            Debug.Log("Synchronized");

            return ((new Player { AccountId = account.Id, State = state }, configs), null);
        }

        private async Task<Error> HandleCommandsAsync(ExecutionQueue executionQueue, IClient client, CancellationToken ct)
        {
            var commandSender = new CommandSender(client);

            Error error = null;
            while (!ct.IsCancellationRequested && client.IsConnected)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var command = await executionQueue.WaitForCommandAsync(cts.Token);
                if (command != null)
                {
                    Debug.Log($"Sending command: {command.GetType().Name}");
                    error = await commandSender.Send(command, cts.Token);
                    if (error != null)
                    {
                        break;
                    }
                }
            }

            return error;
        }

        private async Task<Error> HandleGameLoopAsync(ExecutionQueue executionQueue, Player player, Configs configs, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var play = await OpenMainMenuAsync(player, cts.Token);

                if (play != null)
                {
                    executionQueue.Execute(new BeginLevelCommand
                    {
                        LevelId = play.Level,
                    });

                    var levelConfig = configs.Levels[play.Level];
                    var result = await screensLibrary.gameplayScreen.ShowAsync(levelConfig.MaxRolls, levelConfig.TargetNumber, cts.Token);

                    executionQueue.Execute(new EndLevelCommand
                    {
                        Success = result.Won,
                        Score = result.Score,
                    });

                    await screensLibrary.resultScreen.ShowAsync(result.Won, cts.Token);
                }
            }

            return null;
        }

        private async Task<MainScreen.Play> OpenMainMenuAsync(Player player, CancellationToken ct)
        {
            screensLibrary.mainMenuScreen.Show();
            screensLibrary.mainMenuScreen.Setup(new MainScreen.MainScreenData
            {
                AccountId = player.AccountId,
                Energy = player.State.Persistent.Energy.CurrentAmount,
                CurrentLevel = player.State.Persistent.LevelProgression.CurrentLevel,
                MaxLevel = player.State.Persistent.LevelProgression.CurrentLevel,
            });

            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var (openStats, play) = await screensLibrary.mainMenuScreen.WaitForInputAsync(cts.Token);

                if (play != null)
                {
                    return play;
                }

                if (openStats)
                {
                    var statistics = player.State.Persistent.LevelProgression.Statistics.ToArray();
                    await screensLibrary.statsScreen.OpenAsync(statistics, cts.Token);
                }
            }

            screensLibrary.mainMenuScreen.Hide();
            return null;
        }
    }
}