package commands

import "technical-test-backend/internal/core"

func getTestConfigs() core.Configs {
	return core.Configs{
		Energy: core.EnergyConfig{
			MaxEnergy:               10,
			RechargeIntervalSeconds: 10,
		},
		Levels: []core.LevelConfig{
			{}, // Level 0 (unused)
			{
				EnergyCost:   1,
				MaxRolls:     10,
				TargetNumber: 1,
				EnergyReward: 2,
			},
			{
				EnergyCost:   1,
				MaxRolls:     10,
				TargetNumber: 1,
				EnergyReward: 2,
			},
		},
	}
}

// Helper function to create int pointer
func intPtr(i int) *int {
	return &i
}
