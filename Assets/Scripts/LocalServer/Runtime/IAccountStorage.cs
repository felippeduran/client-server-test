public interface IAccountStorage
{
    Error Create(string accountId, string accessToken, PersistentState state);
    (PersistentState, Error) GetPersistentState(string accountId);
    (string, Error) GetAccessToken(string accountId);
}