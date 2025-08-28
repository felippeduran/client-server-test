using System;
using System.Collections.Generic;

[Serializable]
public class AuthenticateArgs
{
    public string AccountId;
    public string AccessToken;
}

[Serializable]
public class AuthenticateRes { }

public class AuthenticationHandler
{
    readonly IAccountStorage accountStorage;

    public AuthenticationHandler(IAccountStorage accountStorage)
    {
        this.accountStorage = accountStorage;
    }

    // [Serializable]
    // public class CreateAccountArgs
    // {
    //     public string AccountId;
    //     public string AccessToken;
    // }

    // [Serializable]
    // public class CreateAccountRes { }

    // [EndpointHandler]
    // public (CreateAccountArgs, Error) CreateAccount(ConnectionState connState, CreateAccountArgs args)
    // {
    //     if (args.AccountId == null)
    //     {
    //         return (null, new Error { Message = "missing account id argument" });
    //     }

    //     if (args.AccessToken == null)
    //     {
    //         return (null, new Error { Message = "missing access token argument" });
    //     }

    //     if (connState.AccountId != args.AccountId)
    //     {
    //         return (null, new Error { Message = "connection already assigned to another account" });
    //     }

    //     var error = CreateAccount(args.AccountId, args.AccessToken);
    //     if (error != null)
    //     {
    //         return (null, error);
    //     }

    //     connState.AccountId = args.AccountId;

    //     return (new CreateAccountArgs
    //     {
    //         AccountId = args.AccountId,
    //     }, null);
    // }

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

        if (connState.AccountId != null && connState.AccountId != args.AccountId)
        {
            return (null, new Error { Message = "connection already assigned to another account" });
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
                CurrentAmount = 5,
                LastRechargeAt = DateTime.Now
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