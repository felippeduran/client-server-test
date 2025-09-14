using System.Collections.Generic;
using Core.Runtime;
using Networking.Runtime;

namespace LocalServer.Runtime
{
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

            return (PersistentState.DeepCopy(), null);
        }

        public Error SetPersistentState(string accountId, PersistentState state)
        {
            PersistentStates[accountId] = state;
            return null;
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

    public static class PersistentStateExtensions
    {
        public static PersistentState DeepCopy(this PersistentState state)
        {
            return new PersistentState
            {
                Energy = new Energy
                {
                    CurrentAmount = state.Energy.CurrentAmount,
                    LastRechargeAt = state.Energy.LastRechargeAt,
                },
                LevelProgression = new LevelProgression
                {
                    CurrentLevel = state.LevelProgression.CurrentLevel,
                    Statistics = new SortedSet<LevelStats>(state.LevelProgression.Statistics),
                }
            };
        }
    }
}