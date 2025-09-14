package configs

import (
	"encoding/json"
	"fmt"
	"os"
	"technical-test-backend/internal/core"
)

type ProviderConfig struct {
	FilePath string
}

type Provider struct {
	configPath string
}

func NewProvider(config ProviderConfig) *Provider {
	return &Provider{
		configPath: config.FilePath,
	}
}

func (p *Provider) GetConfigs() (core.Configs, error) {
	data, err := os.ReadFile(p.configPath)
	if err != nil {
		return core.Configs{}, fmt.Errorf("failed to read config file %s: %w", p.configPath, err)
	}

	configs := core.Configs{}
	if err := json.Unmarshal(data, &configs); err != nil {
		return core.Configs{}, fmt.Errorf("failed to parse config JSON: %w", err)
	}

	return configs, nil
}
