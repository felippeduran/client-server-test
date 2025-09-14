using System;
using System.Collections.Generic;
using Core.Runtime;
using Networking.Runtime.Fake;
using Networking.Runtime;
using Services.Runtime;

namespace LocalServer.Runtime
{
    public class AuthenticationHandler
    {
        readonly IAccountStorage accountStorage;

        public AuthenticationHandler(IAccountStorage accountStorage)
        {
            this.accountStorage = accountStorage;
        }

        [EndpointHandler]
        public (AuthenticateRes, Error) Authenticate(ConnectionState connState, AuthenticateArgs args)
        {
            if (args.AccountId == null)
            {
                return (null, new Error { Message = "missing account id argument" });
            }

            if (args.AccessToken == null)
            {
                return (null, new Error { Message = "missing access token argument" });
            }

            if (connState.AccountId != null)
            {
                return (null, new Error { Message = "connection already authenticated" });
            }

            var (accessToken, error) = accountStorage.GetAccessToken(args.AccountId);
            if (error != null)
            {
                if (error.Message != "account not found")
                {
                    return (null, error);
                }

                error = CreateAccount(args.AccountId, args.AccessToken);
                if (error != null)
                {
                    return (null, error);
                }
                accessToken = args.AccessToken;
            }

            if (accessToken != args.AccessToken)
            {
                return (null, new Error { Message = "Invalid access token" });
            }

            connState.AccountId = args.AccountId;

            return (new AuthenticateRes { }, null);
        }

        Error CreateAccount(string accountId, string accessToken)
        {
            var initialState = new PersistentState
            {
                Energy = new Energy
                {
                    CurrentAmount = 1,
                    LastRechargeAt = DateTime.UtcNow
                },
                LevelProgression = new LevelProgression
                {
                    CurrentLevel = 1,
                    Statistics = new SortedSet<LevelStats> { },
                }
            };

            var error = accountStorage.Create(accountId, accessToken, initialState);
            if (error != null)
            {
                return error;
            }

            return null;
        }
    }
}