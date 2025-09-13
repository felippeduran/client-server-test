package main

import "time"

// ConfigsProvider provides game configuration
type ConfigsProvider struct{}

// NewConfigsProvider creates a new configs provider
func NewConfigsProvider() *ConfigsProvider {
	return &ConfigsProvider{}
}

// GetHardcodedConfigs returns the hardcoded game configuration
// This matches the C# ConfigsProvider.GetHardcodedConfigs() implementation
func (cp *ConfigsProvider) GetHardcodedConfigs() *Configs {
	return &Configs{
		Energy: EnergyConfig{
			MaxEnergy:        50,
			RechargeInterval: 10 * time.Second,
		},
		Levels: []LevelConfig{
			{}, // Level 0 - empty config
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 1, EnergyReward: 2}, // Level 1
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 2, EnergyReward: 2}, // Level 2
			{EnergyCost: 1, MaxRolls: 5, TargetNumber: 2, EnergyReward: 4},  // Level 3
			{EnergyCost: 1, MaxRolls: 3, TargetNumber: 2, EnergyReward: 7},  // Level 4
			{EnergyCost: 1, MaxRolls: 1, TargetNumber: 2, EnergyReward: 10}, // Level 5
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 3, EnergyReward: 2}, // Level 6
			{EnergyCost: 1, MaxRolls: 5, TargetNumber: 3, EnergyReward: 4},  // Level 7
			{EnergyCost: 1, MaxRolls: 3, TargetNumber: 3, EnergyReward: 7},  // Level 8
			{EnergyCost: 1, MaxRolls: 1, TargetNumber: 3, EnergyReward: 10}, // Level 9
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 4, EnergyReward: 2}, // Level 10
			{EnergyCost: 1, MaxRolls: 5, TargetNumber: 4, EnergyReward: 4},  // Level 11
			{EnergyCost: 1, MaxRolls: 3, TargetNumber: 4, EnergyReward: 7},  // Level 12
			{EnergyCost: 1, MaxRolls: 1, TargetNumber: 4, EnergyReward: 10}, // Level 13
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 5, EnergyReward: 2}, // Level 14
			{EnergyCost: 1, MaxRolls: 5, TargetNumber: 5, EnergyReward: 4},  // Level 15
			{EnergyCost: 1, MaxRolls: 3, TargetNumber: 5, EnergyReward: 7},  // Level 16
			{EnergyCost: 1, MaxRolls: 1, TargetNumber: 5, EnergyReward: 10}, // Level 17
			{EnergyCost: 1, MaxRolls: 10, TargetNumber: 6, EnergyReward: 2}, // Level 18
			{EnergyCost: 1, MaxRolls: 5, TargetNumber: 6, EnergyReward: 4},  // Level 19
			{EnergyCost: 1, MaxRolls: 3, TargetNumber: 6, EnergyReward: 7},  // Level 20
			{EnergyCost: 1, MaxRolls: 1, TargetNumber: 6, EnergyReward: 10}, // Level 21
		},
	}
}
