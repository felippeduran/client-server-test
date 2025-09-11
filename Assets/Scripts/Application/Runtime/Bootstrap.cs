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
                new CommandHandler(new CommandHandler.Config { MaxTimeDifferenceMilliseconds = 1000 }, accountStorage),
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
                    await screensLibrary.errorScreen.ShowAsync(error.Message, "Try again", cts.Token);
                    continue;
                }

                screensLibrary.loadingScreen.SetProgress(1);
                screensLibrary.loadingScreen.gameObject.SetActive(false);

                var executionQueue = new ExecutionQueue(synchronized.player.State, synchronized.configs);

                var commandsTask = HandleCommandsAsync(executionQueue, client, cts.Token);
                var gameLoopTask = HandleGameLoopAsync(executionQueue, synchronized.player, synchronized.clock, synchronized.configs, cts.Token);

                var completedTask = await Task.WhenAny(commandsTask, gameLoopTask);

                if (completedTask.Result != null)
                {
                    client.Disconnect();
                    await screensLibrary.errorScreen.ShowAsync(completedTask.Result.Message, "Try again", cts.Token);
                }
            }
        }

        private async Task<((Player player, Configs configs, IClock clock), Error)> HandleConnectionAsync(Account account, IClient client, CancellationToken ct)
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
            DateTime serverTime;
            ((state, configs, serverTime), error) = await metagameClient.SynchronizeAsync(ct);
            if (error != null)
            {
                Debug.LogError(error.Message);
                return (default, error);
            }

            Debug.Log("Synchronized");

            return ((new Player { AccountId = account.Id, State = state }, configs, new OffsetClock(serverTime)), null);
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

        private async Task<Error> HandleGameLoopAsync(ExecutionQueue executionQueue, IReadOnlyPlayer player, IClock clock, Configs configs, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var play = await OpenMainMenuAsync(player, clock, configs, cts.Token);

                if (play != null)
                {
                    executionQueue.Execute(new BeginLevelCommand
                    {
                        LevelId = play.Level,
                        Now = clock.Now(),
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

        private async Task<MainScreen.Play> OpenMainMenuAsync(IReadOnlyPlayer player, IClock clock, Configs configs, CancellationToken ct)
        {
            var currentLevel = player.State.Persistent.LevelProgression.CurrentLevel;
            screensLibrary.mainMenuScreen.Show();
            screensLibrary.mainMenuScreen.Setup(GetMainScreenData(player, currentLevel, clock.Now(), configs));

            while (!ct.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var (openStats, play, refresh) = await screensLibrary.mainMenuScreen.WaitForInputAsync(cts.Token);

                if (play != null)
                {
                    if (player.State.Persistent.Energy.GetPredictedAmount(clock.Now(), configs.Energy) < configs.Levels[play.Level].EnergyCost)
                    {
                        await screensLibrary.errorScreen.ShowAsync("Not enough energy", "OK", cts.Token);
                        continue;
                    }

                    return play;
                }

                if (openStats)
                {
                    var statistics = player.State.Persistent.LevelProgression.Statistics.ToArray();
                    await screensLibrary.statsScreen.OpenAsync(statistics, cts.Token);
                }

                if (refresh)
                {
                    screensLibrary.mainMenuScreen.Setup(GetMainScreenData(player, currentLevel, clock.Now(), configs));
                }
            }

            screensLibrary.mainMenuScreen.Hide();
            return null;
        }

        private MainScreenData GetMainScreenData(IReadOnlyPlayer player, int currentLevel, DateTime now, Configs configs)
        {
            var levelConfig = configs.Levels[currentLevel];
            return new MainScreenData
            {
                AccountId = player.AccountId,
                EnergyAmount = player.State.Persistent.Energy.GetPredictedAmount(now, configs.Energy),
                CurrentLevel = currentLevel,
                MaxLevel = player.State.Persistent.LevelProgression.CurrentLevel,
                CanPlay = player.State.Persistent.Energy.GetPredictedAmount(now, configs.Energy) > levelConfig.EnergyCost,
            };
        }
    }
}