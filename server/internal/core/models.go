package core

import (
	"time"
)

// Player represents a player with their state
type Player struct {
	AccountID string      `json:"accountId"`
	State     PlayerState `json:"state"`
}

// PlayerState contains both persistent and session state
type PlayerState struct {
	Persistent *PersistentState `json:"persistent"`
	Session    *SessionState    `json:"session"`
}

// PersistentState contains data that survives across sessions
type PersistentState struct {
	Energy           Energy           `json:"energy"`
	LevelProgression LevelProgression `json:"levelProgression"`
}

// SessionState contains temporary session data
type SessionState struct {
	CurrentLevelID *int `json:"currentLevelId,omitempty"`
}

// Energy represents the player's energy system
type Energy struct {
	CurrentAmount  int       `json:"currentAmount"`
	LastRechargeAt time.Time `json:"lastRechargeAt"`
}

// LevelProgression tracks player's level progress
type LevelProgression struct {
	CurrentLevel int          `json:"currentLevel"`
	Statistics   []LevelStats `json:"statistics"`
}

// LevelStats tracks statistics for a specific level
type LevelStats struct {
	LevelID   int `json:"levelId"`
	BestScore int `json:"bestScore"`
	Wins      int `json:"wins"`
	Losses    int `json:"losses"`
}
