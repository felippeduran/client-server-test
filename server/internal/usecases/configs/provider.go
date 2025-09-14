package configs

import (
	"encoding/json"
	"fmt"
	"os"
	"technical-test-backend/internal/core"
)

// ProviderConfig defines the configuration for the config provider
type ProviderConfig struct {
	FilePath string
}

// Provider provides game configuration
type Provider struct {
	configPath string
}

// NewProvider creates a new configs provider with custom config
func NewProvider(config ProviderConfig) *Provider {
	return &Provider{
		configPath: config.FilePath,
	}
}

// GetConfigs loads the game configuration from JSON file
func (p *Provider) GetConfigs() (core.Configs, error) {
	// Read the JSON file
	data, err := os.ReadFile(p.configPath)
	if err != nil {
		return core.Configs{}, fmt.Errorf("failed to read config file %s: %w", p.configPath, err)
	}

	// Parse JSON into a temporary struct that matches the JSON structure
	configs := core.Configs{}
	if err := json.Unmarshal(data, &configs); err != nil {
		return core.Configs{}, fmt.Errorf("failed to parse config JSON: %w", err)
	}

	return configs, nil
}
