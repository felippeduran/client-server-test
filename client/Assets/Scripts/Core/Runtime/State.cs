using System;
using System.Collections.Generic;

public interface IReadOnlyEnergy
{
    int CurrentAmount { get; }
    DateTime LastRechargeAt { get; }
}

public interface IReadOnlyLevelProgression
{
    int CurrentLevel { get; }
    IReadOnlyCollection<LevelStats> Statistics { get; }
}

public interface IReadOnlyPersistentState
{
    IReadOnlyEnergy Energy { get; }
    IReadOnlyLevelProgression LevelProgression { get; }
}

public interface IReadOnlyPlayerState
{
    IReadOnlyPersistentState Persistent { get; }
}

public interface IReadOnlyPlayer
{
    string AccountId { get; }
    IReadOnlyPlayerState State { get; }
}

[Serializable]
public class Energy : IReadOnlyEnergy
{
    public int CurrentAmount;
    public DateTime LastRechargeAt;

    int IReadOnlyEnergy.CurrentAmount => CurrentAmount;
    DateTime IReadOnlyEnergy.LastRechargeAt => LastRechargeAt;
}

[Serializable]
public class LevelProgression : IReadOnlyLevelProgression
{
    public int CurrentLevel;
    public SortedSet<LevelStats> Statistics;

    int IReadOnlyLevelProgression.CurrentLevel => CurrentLevel;
    IReadOnlyCollection<LevelStats> IReadOnlyLevelProgression.Statistics => Statistics;
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
public class Account
{
    public string Id;
    public string AccessToken;
}

[Serializable]
public class Player : IReadOnlyPlayer
{
    public string AccountId;
    public PlayerState State = new ();

    string IReadOnlyPlayer.AccountId => AccountId;
    IReadOnlyPlayerState IReadOnlyPlayer.State => State;
}

[Serializable]
public class PlayerState : IReadOnlyPlayerState
{
    public PersistentState Persistent = new ();
    public SessionState Session = new ();

    IReadOnlyPersistentState IReadOnlyPlayerState.Persistent => Persistent;
}

[Serializable]
public class PersistentState : IReadOnlyPersistentState
{
    public Energy Energy = new ();
    public LevelProgression LevelProgression = new ();

    IReadOnlyEnergy IReadOnlyPersistentState.Energy => Energy;
    IReadOnlyLevelProgression IReadOnlyPersistentState.LevelProgression => LevelProgression;
}

[Serializable]
public struct SessionState
{
    public int? CurrentLevelId;
}