package commands

import (
	"fmt"
	"technical-test-backend/internal/core"
	"time"
)

type BeginLevel struct {
	LevelID int       `json:"levelId"`
	Now     time.Time `json:"now"`
}

func (c *BeginLevel) Execute(state *core.PlayerState, configs core.Configs) error {
	if !canPlayLevel(state.Persistent.LevelProgression.CurrentLevel, c.LevelID) {
		return fmt.Errorf("level not unlocked")
	}

	levelConfig := configs.Levels[c.LevelID]
	predictedEnergy := getPredictedEnergyAmount(state.Persistent.Energy, c.Now, configs.Energy)
	if predictedEnergy < levelConfig.EnergyCost {
		return fmt.Errorf("not enough energy")
	}

	updateEnergy(&state.Persistent.Energy, c.Now, configs.Energy)
	state.Persistent.Energy.CurrentAmount -= levelConfig.EnergyCost
	state.Session.CurrentLevelID = &c.LevelID

	return nil
}

func (c *BeginLevel) GetTimestamp() time.Time {
	return c.Now
}

func canPlayLevel(currentLevel, targetLevel int) bool {
	return targetLevel <= currentLevel
}

func getPredictedEnergyAmount(energy core.Energy, now time.Time, config core.EnergyConfig) int {
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

func updateEnergy(energy *core.Energy, now time.Time, config core.EnergyConfig) {
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
