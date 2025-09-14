using Core.Runtime;
using Networking.Runtime;

namespace LocalServer.Runtime
{
    public interface IAccountStorage
    {
        Error Create(string accountId, string accessToken, PersistentState state);
        (PersistentState, Error) GetPersistentState(string accountId);
        Error SetPersistentState(string accountId, PersistentState state);
        (string, Error) GetAccessToken(string accountId);
    }
}