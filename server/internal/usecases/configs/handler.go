package configs

import (
	"fmt"

	"technical-test-backend/internal/core"
)

type GetConfigsArgs struct{}

type GetConfigsRes struct {
	Configs core.Configs `json:"configs"`
}

type Handler struct {
	configs *Provider
}

func NewHandler(configs *Provider) *Handler {
	return &Handler{
		configs: configs,
	}
}

func (h *Handler) GetConfigs(args *GetConfigsArgs) (*GetConfigsRes, error) {
	configs, err := h.configs.GetConfigs()
	if err != nil {
		return nil, fmt.Errorf("failed to load configs: %v", err)
	}

	return &GetConfigsRes{
		Configs: configs,
	}, nil
}
