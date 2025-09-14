package core

import (
	"time"
)

type Player struct {
	AccountID string      `json:"accountId"`
	State     PlayerState `json:"state"`
}

type PlayerState struct {
	Persistent *PersistentState `json:"persistent"`
	Session    *SessionState    `json:"session"`
}

type PersistentState struct {
	Energy           Energy           `json:"energy"`
	LevelProgression LevelProgression `json:"levelProgression"`
}

type SessionState struct {
	CurrentLevelID *int `json:"currentLevelId,omitempty"`
}

type Energy struct {
	CurrentAmount  int       `json:"currentAmount"`
	LastRechargeAt time.Time `json:"lastRechargeAt"`
}

type LevelProgression struct {
	CurrentLevel int          `json:"currentLevel"`
	Statistics   []LevelStats `json:"statistics"`
}

type LevelStats struct {
	LevelID   int `json:"levelId"`
	BestScore int `json:"bestScore"`
	Wins      int `json:"wins"`
	Losses    int `json:"losses"`
}
