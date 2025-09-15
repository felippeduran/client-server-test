package commands

import (
	"testing"

	"technical-test-backend/internal/core"

	"github.com/stretchr/testify/assert"
)

func TestEndLevel_WithLevelInProgress_ShouldSucceed(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				Statistics: []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: false,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndSuccess_ShouldAddLevelProgression(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 2, playerState.Persistent.LevelProgression.CurrentLevel)
	assert.Len(t, playerState.Persistent.LevelProgression.Statistics, 1)
	assert.Equal(t, 8, playerState.Persistent.LevelProgression.Statistics[0].BestScore)
	assert.Equal(t, 1, playerState.Persistent.LevelProgression.Statistics[0].Wins)
	assert.Equal(t, 0, playerState.Persistent.LevelProgression.Statistics[0].Losses)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndFailure_ShouldAddLevelProgression(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: false,
		Score:   0,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 1, playerState.Persistent.LevelProgression.CurrentLevel) // Should not advance
	assert.Len(t, playerState.Persistent.LevelProgression.Statistics, 1)
	assert.Equal(t, 0, playerState.Persistent.LevelProgression.Statistics[0].BestScore)
	assert.Equal(t, 0, playerState.Persistent.LevelProgression.Statistics[0].Wins)
	assert.Equal(t, 1, playerState.Persistent.LevelProgression.Statistics[0].Losses)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndBetterScore_ShouldUpdateLevelProgression(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics: []core.LevelStats{
					{
						LevelID:   1,
						BestScore: 5,
						Wins:      1,
						Losses:    0,
					},
				},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 8, playerState.Persistent.LevelProgression.Statistics[0].BestScore)
	assert.Equal(t, 2, playerState.Persistent.LevelProgression.Statistics[0].Wins)
	assert.Equal(t, 0, playerState.Persistent.LevelProgression.Statistics[0].Losses)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndWorseScore_ShouldNotUpdateLevelProgression(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics: []core.LevelStats{
					{
						LevelID:   1,
						BestScore: 5,
						Wins:      1,
						Losses:    0,
					},
				},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   2,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 5, playerState.Persistent.LevelProgression.Statistics[0].BestScore)
	assert.Equal(t, 2, playerState.Persistent.LevelProgression.Statistics[0].Wins)
	assert.Equal(t, 0, playerState.Persistent.LevelProgression.Statistics[0].Losses)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndFailure_ShouldNotDeliverRewards(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount: 0,
			},
			LevelProgression: core.LevelProgression{
				Statistics: []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: false,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 0, playerState.Persistent.Energy.CurrentAmount)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndSuccess_ShouldDeliverRewards(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount: 0,
			},
			LevelProgression: core.LevelProgression{
				Statistics: []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, configs.Levels[1].EnergyReward, playerState.Persistent.Energy.CurrentAmount)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithPreviousLevelInProgress_ShouldNotChangeCurrentLevel(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 3,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 3, playerState.Persistent.LevelProgression.CurrentLevel)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_NoLevelInProgress_ShouldReturnError(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				Statistics: []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: nil,
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.Error(t, err)
	assert.Equal(t, "no level in progress", err.Error())
}

func TestEndLevel_WithMultipleLevelStats_ShouldUpdateCorrectLevel(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 2,
				Statistics: []core.LevelStats{
					{
						LevelID:   1,
						BestScore: 10,
						Wins:      2,
						Losses:    1,
					},
					{
						LevelID:   2,
						BestScore: 5,
						Wins:      1,
						Losses:    0,
					},
				},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(1),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   12,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Len(t, playerState.Persistent.LevelProgression.Statistics, 2)
	assert.Equal(t, 12, playerState.Persistent.LevelProgression.Statistics[0].BestScore)
	assert.Equal(t, 3, playerState.Persistent.LevelProgression.Statistics[0].Wins)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}

func TestEndLevel_WithLevelInProgressAndSuccess_ShouldNotExceedMaxLevel(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			LevelProgression: core.LevelProgression{
				CurrentLevel: 2,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{
			CurrentLevelID: intPtr(2),
		},
	}

	configs := getTestConfigs()

	command := &EndLevel{
		Success: true,
		Score:   8,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 2, playerState.Persistent.LevelProgression.CurrentLevel)
	assert.Nil(t, playerState.Session.CurrentLevelID)
}
