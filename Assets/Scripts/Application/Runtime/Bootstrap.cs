using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Presentation.Main.Screen;
using System.Linq;
using System;

namespace Application.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] ScreensLibrary screensLibrary;
        [SerializeField] PlayerState stateDisplay;

        private async void Start()
        {
            Debug.Log("Starting Bootstrap");

            var accountStorage = new InMemoryAccounts();
            var server = FakeServerFactory.CreateServer<ConnectionState>(new object[] {
                new AuthenticationHandler(accountStorage),
                new CommandHandler(),
                new InitializationHandler(accountStorage),
            });
            var client = new FakeClient(server);

            var ct = UnityEngine.Application.exitCancellationToken;
            while (!ct.IsCancellationRequested)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                screensLibrary.loadingScreen.gameObject.SetActive(true);

                var (synchronized, exit) = await HandleConnectionAsync(client, cts.Token);
                if (exit)
                {
                    return;
                }

                stateDisplay = synchronized.player.State;
                screensLibrary.loadingScreen.SetProgress(1);
                screensLibrary.loadingScreen.gameObject.SetActive(false);

                var executionQueue = new ExecutionQueue(synchronized.player.State, synchronized.configs);

                var commandsTask = HandleCommandsAsync(executionQueue, client, cts.Token);

                var gameLoopTask = HandleGameLoopAsync(executionQueue, synchronized.player, synchronized.configs, cts.Token);

                var completedTask = await Task.WhenAny(commandsTask, gameLoopTask);

                if (completedTask.Result != null)
                {
                    Debug.LogError(completedTask.Result.Message);
                }
            }
        }
        
        private async Task<((Player player, Configs configs), bool)> HandleConnectionAsync(IClient client, CancellationToken ct)
        {
            var account = new Account { Id = Guid.NewGuid().ToString(), AccessToken = "test" };
            var player = new Player { AccountId = account.Id };
            Configs configs = default;

            while (!ct.IsCancellationRequested)
            {
                await client.ConnectAsync(ct);
                Debug.Log("Connected 2");

                var metagameClient = new MetagameClient(client);

                await metagameClient.AuthenticateAsync(account);
                Debug.Log("Authenticated");

                var (result, error) = await metagameClient.SynchronizeAsync();
                if (error != null)
                {
                    Debug.LogError(error.Message);
                    continue;
                }
                Debug.Log("Synchronized");

                (player.State, configs) = result;
                break;
            }

            return ((player, configs), ct.IsCancellationRequested);
        }

        private async Task<Error> HandleCommandsAsync(ExecutionQueue executionQueue, IClient client, CancellationToken ct)
        {
            var commandSender = new CommandSender(client);

            Error error = null;
            while (!ct.IsCancellationRequested && client.IsConnected)
            {
                var command = await executionQueue.WaitForCommandAsync(ct);
                if (command != null)
                {
                    error = await commandSender.Send(command);
                    if (error != null)
                    {
                        client.Disconnect();
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