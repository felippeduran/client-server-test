using System.Collections.Generic;

public class InMemoryAccounts : IAccountStorage
{
    public Dictionary<string, string> Accounts = new();
    public Dictionary<string, PersistentState> PersistentStates = new();

    public Error Create(string accountId, string accessToken, PersistentState state)
    {
        if (Accounts.ContainsKey(accountId))
        {
            return new Error { Message = "account already exists" };
        }

        Accounts.Add(accountId, accessToken);
        PersistentStates.Add(accountId, state);
        return null;
    }

    public (PersistentState, Error) GetPersistentState(string accountId)
    {
        if (!PersistentStates.TryGetValue(accountId, out var PersistentState))
        {
            return (default, new Error { Message = "player state not found" });
        }

        return (PersistentState, null);
    }

    public (string, Error) GetAccessToken(string accountId)
    {
        if (!Accounts.TryGetValue(accountId, out var accessToken))
        {
            return (null, new Error { Message = "account not found" });
        }

        return (accessToken, null);
    }
}