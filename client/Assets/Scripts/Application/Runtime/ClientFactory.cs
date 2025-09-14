using System;
using Networking.Runtime;
using Networking.Runtime.Fake;
using LocalServer.Runtime;
using Networking.Runtime.Http;

namespace Application.Runtime
{
    public class ClientFactory
    {
        public IClient CreateClient(AppConfig config)
        {
            IClient client;
            if (config.UseFakeServer)
            {
                client = CreateFakeClient(config);
            }
            else
            {
                client = CreateHttpClient(config);
            }
            return client;
        }

        IClient CreateFakeClient(AppConfig config)
        {
            var accountStorage = new InMemoryAccounts();
            var server = FakeServerFactory.CreateServer<ConnectionState>(new object[] {
                new AuthenticationHandler(accountStorage),
                new CommandHandler(new CommandHandler.Config { MaxTimeDifferenceMilliseconds = 1000 }, accountStorage),
                    new InitializationHandler(accountStorage),
                });
            IClient client = new FakeClient(server);
            if (config.UseConnectivity)
            {
                client = new ConnectivityClientDecorator(new ConnectivityClientDecorator.Config
                {
                    FailureRate = config.Connectivity.FailureRate,
                    RTTSeconds = config.Connectivity.RTTSeconds
                }, client);
            }
            return client;
        }

        IClient CreateHttpClient(AppConfig config)
        {
            return new HttpClient(new Config
            {
                BaseUrl = config.BaseUrl,
                Timeout = TimeSpan.FromSeconds(5),
                UseConnectivity = config.UseConnectivity,
                Connectivity = new ConnectivityHttpMessageHandler.ConnectivityConfig
                {
                    FailureRate = config.Connectivity.FailureRate,
                    RTTSeconds = config.Connectivity.RTTSeconds
                }
            });
        }
    }
}