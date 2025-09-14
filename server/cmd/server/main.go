package main

import (
	"technical-test-backend/internal/app"
	"technical-test-backend/internal/session/memory"
	"technical-test-backend/internal/usecases/commands"
	"technical-test-backend/internal/usecases/configs"
	"time"
)

func main() {
	app := app.NewHTTP(app.Config{
		Port: 8080,
		SessionPool: memory.SessionPoolConfig{
			TTL: 30 * time.Second,
		},
		ConfigProvider: configs.ProviderConfig{
			FilePath: "../../config/game_config.json",
		},
		Commands: commands.Config{
			MaxTimeDifferenceSeconds: 1,
		},
	})

	app.Run()
}
