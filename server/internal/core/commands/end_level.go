package commands

import (
	"fmt"
	"technical-test-backend/internal/core"
)

type EndLevel struct {
	Success bool `json:"success"`
	Score   int  `json:"score"`
}

func (c *EndLevel) Execute(state *core.PlayerState, configs core.Configs) error {
	if state.Session.CurrentLevelID == nil {
		return fmt.Errorf("no level in progress")
	}

	currentLevelID := *state.Session.CurrentLevelID
	levelConfig := configs.Levels[currentLevelID]

	stats := findOrCreateLevelStats(&state.Persistent.LevelProgression, currentLevelID)

	if c.Success && c.Score > stats.BestScore {
		stats.BestScore = c.Score
	}
	if c.Success {
		stats.Wins++
	} else {
		stats.Losses++
	}

	updateLevelStats(&state.Persistent.LevelProgression, stats)

	if c.Success {
		if currentLevelID == state.Persistent.LevelProgression.CurrentLevel {
			state.Persistent.LevelProgression.CurrentLevel = currentLevelID + 1
		}
		state.Persistent.Energy.CurrentAmount += levelConfig.EnergyReward
	}

	state.Session.CurrentLevelID = nil

	return nil
}

func findOrCreateLevelStats(progression *core.LevelProgression, levelID int) *core.LevelStats {
	for i := range progression.Statistics {
		if progression.Statistics[i].LevelID == levelID {
			return &progression.Statistics[i]
		}
	}

	stats := core.LevelStats{
		LevelID:   levelID,
		BestScore: 0,
		Wins:      0,
		Losses:    0,
	}
	progression.Statistics = append(progression.Statistics, stats)
	return &progression.Statistics[len(progression.Statistics)-1]
}

func updateLevelStats(progression *core.LevelProgression, stats *core.LevelStats) {
	for i := range progression.Statistics {
		if progression.Statistics[i].LevelID == stats.LevelID {
			progression.Statistics[i] = *stats
			return
		}
	}
}
