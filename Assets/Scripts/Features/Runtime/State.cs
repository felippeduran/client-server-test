using System;
using System.Collections.Generic;

[Serializable]
public struct Energy
{
    public int CurrentAmount;
    public DateTime LastRechargeAt;
}

[Serializable]
public struct LevelProgression
{
    public int CurrentLevel;
    public SortedSet<LevelStats> Statistics;
}

[Serializable]
public struct LevelStats : IComparable<LevelStats>
{
    public int LevelId;
    public int BestScore;
    public int Wins;
    public int Losses;

    public int CompareTo(LevelStats other)
    {
        return LevelId.CompareTo(other.LevelId);
    }
}

[Serializable]
public struct Account
{
    public string Id;
    public string AccessToken;
}

[Serializable]
public class Player
{
    public string AccountId;
    public PlayerState State;
}

[Serializable]
public class PlayerState
{
    public PersistentState Persistent;
    public SessionState Session;
}

[Serializable]
public struct PersistentState
{
    public Energy Energy;
    public LevelProgression LevelProgression;
}

[Serializable]
public struct SessionState
{
    public int? CurrentLevelId;
}