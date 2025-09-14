using System;
using System.Threading;
using System.Threading.Tasks;
using Networking.Runtime;
using Core.Runtime;

namespace Services.Runtime
{
    public interface IAuthenticationService
    {
        Task<Error> AuthenticateAsync(Account account, CancellationToken ct);
    }

    [Serializable]
    public class AuthenticateArgs
    {
        public string AccountId;
        public string AccessToken;
    }

    [Serializable]
    public class AuthenticateRes
    {
        public string SessionId;
    }

    public class AuthenticationService : IAuthenticationService
    {
        readonly IClient client;

        public AuthenticationService(IClient client)
        {
            this.client = client;
        }

        public async Task<Error> AuthenticateAsync(Account account, CancellationToken ct)
        {
            var (res, error) = await client.SendMessage<AuthenticateArgs, AuthenticateRes>("AuthenticationHandler/Authenticate", new AuthenticateArgs { AccountId = account.Id, AccessToken = account.AccessToken }, ct);
            if (error != null)
            {
                return error;
            }

            return null;
        }
    }
}