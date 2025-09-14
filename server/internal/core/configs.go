package core

import "time"

type Configs struct {
	Levels []LevelConfig `json:"levels"`
	Energy EnergyConfig  `json:"energy"`
}

type EnergyConfig struct {
	MaxEnergy               int `json:"maxEnergy"`
	RechargeIntervalSeconds int `json:"rechargeIntervalSeconds"`
}

func (c *EnergyConfig) RechargeInterval() time.Duration {
	return time.Duration(c.RechargeIntervalSeconds) * time.Second
}

type LevelConfig struct {
	EnergyCost   int `json:"energyCost"`
	MaxRolls     int `json:"maxRolls"`
	TargetNumber int `json:"targetNumber"`
	EnergyReward int `json:"energyReward"`
}
