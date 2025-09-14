package commands

import (
	"testing"
	"time"

	"technical-test-backend/internal/core"

	"github.com/stretchr/testify/assert"
)

func TestBeginLevel_WithEnergyAndLevelUnlocked_ShouldSucceed(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount:  1,
				LastRechargeAt: time.Now(),
			},
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{},
	}

	configs := getTestConfigs()

	command := &BeginLevel{
		LevelID: 1,
		Now:     time.Now(),
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, 0, playerState.Persistent.Energy.CurrentAmount)
	assert.Equal(t, 1, *playerState.Session.CurrentLevelID)
}

func TestBeginLevel_WithoutEnergy_ShouldReturnError(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount:  0,
				LastRechargeAt: time.Now(),
			},
			LevelProgression: core.LevelProgression{
				CurrentLevel: 1,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{},
	}

	configs := getTestConfigs()

	command := &BeginLevel{
		LevelID: 1,
		Now:     time.Now(),
	}

	err := command.Execute(playerState, configs)

	assert.Error(t, err)
	assert.Equal(t, "not enough energy", err.Error())
}

func TestBeginLevel_LevelNotUnlocked_ShouldReturnError(t *testing.T) {
	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount: 1,
			},
			LevelProgression: core.LevelProgression{
				CurrentLevel: 0,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{},
	}

	configs := getTestConfigs()

	command := &BeginLevel{
		LevelID: 1,
		Now:     time.Now(),
	}

	err := command.Execute(playerState, configs)

	assert.Error(t, err)
	assert.Equal(t, "level not unlocked", err.Error())
}

func TestBeginLevel_WithEnergyRecharge_ShouldSucceed(t *testing.T) {
	now := time.Now()
	configs := getTestConfigs()

	currentLevel := 1
	energyRecharge := 2
	expectedEnergyTime := time.Duration(energyRecharge) * configs.Energy.RechargeInterval()
	expectedEnergy := energyRecharge - configs.Levels[currentLevel].EnergyCost

	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount:  0,
				LastRechargeAt: now.Add(-expectedEnergyTime),
			},
			LevelProgression: core.LevelProgression{
				CurrentLevel: currentLevel,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{},
	}

	command := &BeginLevel{
		LevelID: currentLevel,
		Now:     now,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, expectedEnergy, playerState.Persistent.Energy.CurrentAmount)
	assert.Equal(t, 1, *playerState.Session.CurrentLevelID)
}

func TestBeginLevel_WithMaxEnergy_ShouldSucceed(t *testing.T) {
	now := time.Now()
	configs := getTestConfigs()

	currentLevel := 1
	expectedEnergy := configs.Energy.MaxEnergy - configs.Levels[currentLevel].EnergyCost

	playerState := &core.PlayerState{
		Persistent: &core.PersistentState{
			Energy: core.Energy{
				CurrentAmount:  configs.Energy.MaxEnergy, // Max energy
				LastRechargeAt: now.Add(-5 * time.Second),
			},
			LevelProgression: core.LevelProgression{
				CurrentLevel: currentLevel,
				Statistics:   []core.LevelStats{},
			},
		},
		Session: &core.SessionState{},
	}

	command := &BeginLevel{
		LevelID: currentLevel,
		Now:     now,
	}

	err := command.Execute(playerState, configs)

	assert.NoError(t, err)
	assert.Equal(t, expectedEnergy, playerState.Persistent.Energy.CurrentAmount)
	assert.Equal(t, 1, *playerState.Session.CurrentLevelID)
}

func TestBeginLevel_GetTimestamp_ShouldReturnNow(t *testing.T) {
	now := time.Now()
	command := &BeginLevel{
		LevelID: 1,
		Now:     now,
	}

	assert.Equal(t, now, command.GetTimestamp())
}
