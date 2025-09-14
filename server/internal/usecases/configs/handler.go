package configs

import (
	"fmt"

	"technical-test-backend/internal/core"
)

// GetConfigsArgs represents get configs request
type GetConfigsArgs struct{}

// GetConfigsRes represents get configs response
type GetConfigsRes struct {
	Configs core.Configs `json:"configs"`
}

// Handler handles configuration requests
type Handler struct {
	configs *Provider
}

// Handler creates a new config handler
func NewHandler(configs *Provider) *Handler {
	return &Handler{
		configs: configs,
	}
}

// GetConfigs retrieves game configuration
func (h *Handler) GetConfigs(args *GetConfigsArgs) (*GetConfigsRes, error) {
	configs, err := h.configs.GetConfigs()
	if err != nil {
		return nil, fmt.Errorf("failed to load configs: %v", err)
	}

	return &GetConfigsRes{
		Configs: configs,
	}, nil
}
