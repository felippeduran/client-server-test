package main

import (
	"fmt"
	"technical-test-backend/internal/configs"
	"technical-test-backend/internal/core"
	"technical-test-backend/internal/session"
)

// GetConfigsArgs represents get configs request
type GetConfigsArgs struct{}

// GetConfigsRes represents get configs response
type GetConfigsRes struct {
	Configs core.Configs `json:"configs"`
}

// ConfigHandler handles configuration requests
type ConfigHandler struct {
	sessionPool session.Pool
	configs     *configs.Provider
}

// NewConfigHandler creates a new config handler
func NewConfigHandler(sessionPool session.Pool, configs *configs.Provider) *ConfigHandler {
	return &ConfigHandler{
		sessionPool: sessionPool,
		configs:     configs,
	}
}

// GetConfigs retrieves game configuration
func (h *ConfigHandler) GetConfigs(sessionID string, args *GetConfigsArgs) (*GetConfigsRes, error) {
	// Check authentication
	_, authenticated := h.sessionPool.GetAccountID(sessionID)
	if !authenticated {
		return nil, fmt.Errorf("connection not authenticated")
	}

	// Get configs
	configs, err := h.configs.GetConfigs()
	if err != nil {
		return nil, fmt.Errorf("failed to load configs: %v", err)
	}

	// Update activity
	h.sessionPool.UpdateActivity(sessionID)

	return &GetConfigsRes{
		Configs: configs,
	}, nil
}
