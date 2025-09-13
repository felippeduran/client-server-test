package core

import (
	"fmt"
	"time"
)

// Player represents a player with their state
type Player struct {
	AccountID string      `json:"accountId"`
	State     PlayerState `json:"state"`
}

// PlayerState contains both persistent and session state
type PlayerState struct {
	Persistent PersistentState `json:"persistent"`
	Session    SessionState    `json:"session"`
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

// Command interfaces
type Command interface {
	Execute(state *PlayerState, configs Configs) error
}

type TimedCommand interface {
	Command
	GetTimestamp() time.Time
}

// BeginLevelCommand represents starting a level
type BeginLevelCommand struct {
	LevelID int       `json:"levelId"`
	Now     time.Time `json:"now"`
}

func (c *BeginLevelCommand) Execute(state *PlayerState, configs Configs) error {
	// Check if level is unlocked
	if !canPlayLevel(state.Persistent.LevelProgression.CurrentLevel, c.LevelID) {
		return fmt.Errorf("level not unlocked")
	}

	// Check energy requirements
	levelConfig := configs.Levels[c.LevelID]
	predictedEnergy := getPredictedEnergyAmount(state.Persistent.Energy, c.Now, configs.Energy)
	if predictedEnergy < levelConfig.EnergyCost {
		return fmt.Errorf("not enough energy")
	}

	// Update energy
	updateEnergy(&state.Persistent.Energy, c.Now, configs.Energy)
	state.Persistent.Energy.CurrentAmount -= levelConfig.EnergyCost
	state.Session.CurrentLevelID = &c.LevelID

	return nil
}

func (c *BeginLevelCommand) GetTimestamp() time.Time {
	return c.Now
}

// EndLevelCommand represents completing a level
type EndLevelCommand struct {
	Success bool `json:"success"`
	Score   int  `json:"score"`
}

func (c *EndLevelCommand) Execute(state *PlayerState, configs Configs) error {
	if state.Session.CurrentLevelID == nil {
		return fmt.Errorf("no level in progress")
	}

	currentLevelID := *state.Session.CurrentLevelID
	levelConfig := configs.Levels[currentLevelID]

	// Find or create level stats
	stats := findOrCreateLevelStats(&state.Persistent.LevelProgression, currentLevelID)

	// Update statistics
	if c.Success && c.Score > stats.BestScore {
		stats.BestScore = c.Score
	}
	if c.Success {
		stats.Wins++
	} else {
		stats.Losses++
	}

	// Update level progression
	updateLevelStats(&state.Persistent.LevelProgression, stats)

	// Advance level if successful
	if c.Success {
		if currentLevelID == state.Persistent.LevelProgression.CurrentLevel {
			state.Persistent.LevelProgression.CurrentLevel = currentLevelID + 1
		}
		state.Persistent.Energy.CurrentAmount += levelConfig.EnergyReward
	}

	// Clear current level
	state.Session.CurrentLevelID = nil

	return nil
}

// Helper functions for command execution

func canPlayLevel(currentLevel, targetLevel int) bool {
	return targetLevel <= currentLevel
}

func getPredictedEnergyAmount(energy Energy, now time.Time, config EnergyConfig) int {
	if energy.CurrentAmount >= config.MaxEnergy {
		return energy.CurrentAmount
	}

	timeSinceRecharge := now.Sub(energy.LastRechargeAt)
	rechargeIntervals := int(timeSinceRecharge / config.RechargeInterval())
	predictedAmount := energy.CurrentAmount + rechargeIntervals

	if predictedAmount > config.MaxEnergy {
		return config.MaxEnergy
	}
	return predictedAmount
}

func updateEnergy(energy *Energy, now time.Time, config EnergyConfig) {
	if energy.CurrentAmount >= config.MaxEnergy {
		energy.LastRechargeAt = now
		return
	}

	timeSinceRecharge := now.Sub(energy.LastRechargeAt)
	rechargeIntervals := int(timeSinceRecharge / config.RechargeInterval())

	if rechargeIntervals > 0 {
		energy.CurrentAmount += rechargeIntervals
		if energy.CurrentAmount > config.MaxEnergy {
			energy.CurrentAmount = config.MaxEnergy
		}
		energy.LastRechargeAt = energy.LastRechargeAt.Add(time.Duration(rechargeIntervals) * config.RechargeInterval())
	}
}

func findOrCreateLevelStats(progression *LevelProgression, levelID int) *LevelStats {
	for i := range progression.Statistics {
		if progression.Statistics[i].LevelID == levelID {
			return &progression.Statistics[i]
		}
	}

	// Create new stats
	stats := LevelStats{
		LevelID:   levelID,
		BestScore: 0,
		Wins:      0,
		Losses:    0,
	}
	progression.Statistics = append(progression.Statistics, stats)
	return &progression.Statistics[len(progression.Statistics)-1]
}

func updateLevelStats(progression *LevelProgression, stats *LevelStats) {
	for i := range progression.Statistics {
		if progression.Statistics[i].LevelID == stats.LevelID {
			progression.Statistics[i] = *stats
			return
		}
	}
}
