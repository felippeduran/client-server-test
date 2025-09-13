package main

import (
	"fmt"
	"log"
	"time"
)

// Example usage of the core functionality
func main() {
	// Create dependencies
	sessionPool := NewSessionPool()
	dal := NewDAL()
	configs := NewConfigsProvider()

	// Create handlers
	authHandler := NewAuthenticationHandler(sessionPool, dal)
	stateHandler := NewPlayerStateHandler(sessionPool, dal)
	configHandler := NewConfigHandler(sessionPool, configs)
	commandHandler := NewCommandHandler(sessionPool, dal, configs)

	// Example: Create a session
	session, err := sessionPool.CreateSession()
	if err != nil {
		log.Fatalf("Failed to create session: %v", err)
	}
	fmt.Printf("Created session: %s\n", session.ID)

	// Example: Authenticate
	authArgs := &AuthenticateArgs{
		AccountID:   "test-account-123",
		AccessToken: "test-token-456",
	}

	authRes, authErr := authHandler.Authenticate(session.ID, authArgs)
	if authErr != nil {
		log.Fatalf("Authentication failed: %v", authErr)
	}
	fmt.Printf("Authentication successful: %+v\n", authRes)

	// Example: Get player state
	playerStateRes, playerStateErr := stateHandler.GetPlayerState(session.ID, &GetPlayerStateArgs{})
	if playerStateErr != nil {
		log.Fatalf("Get player state failed: %v", playerStateErr)
	}
	fmt.Printf("Player state: Level %d, Energy %d\n",
		playerStateRes.PlayerState.Persistent.LevelProgression.CurrentLevel,
		playerStateRes.PlayerState.Persistent.Energy.CurrentAmount)

	// Example: Get configs
	configsRes, configsErr := configHandler.GetConfigs(session.ID, &GetConfigsArgs{})
	if configsErr != nil {
		log.Fatalf("Get configs failed: %v", configsErr)
	}
	fmt.Printf("Configs loaded: %d levels, Max Energy %d\n",
		len(configsRes.Configs.Levels),
		configsRes.Configs.Energy.MaxEnergy)

	// Example: Begin level command
	beginLevelCmd := &BeginLevelCommand{
		LevelID: 1,
		Now:     time.Now(),
	}

	beginErr := commandHandler.HandleCommand(session.ID, beginLevelCmd)
	if beginErr != nil {
		log.Fatalf("Begin level command failed: %v", beginErr)
	}
	fmt.Println("Begin level command executed successfully")

	// Example: End level command
	endLevelCmd := &EndLevelCommand{
		Success: true,
		Score:   100,
	}

	endErr := commandHandler.HandleCommand(session.ID, endLevelCmd)
	if endErr != nil {
		log.Fatalf("End level command failed: %v", endErr)
	}
	fmt.Println("End level command executed successfully")

	// Example: Get updated player state
	updatedPlayerStateRes, updatedPlayerStateErr := stateHandler.GetPlayerState(session.ID, &GetPlayerStateArgs{})
	if updatedPlayerStateErr != nil {
		log.Fatalf("Get updated player state failed: %v", updatedPlayerStateErr)
	}
	fmt.Printf("Updated player state: Level %d, Energy %d\n",
		updatedPlayerStateRes.PlayerState.Persistent.LevelProgression.CurrentLevel,
		updatedPlayerStateRes.PlayerState.Persistent.Energy.CurrentAmount)

	// Example: Check session pool stats
	fmt.Printf("Session pool stats: %d active sessions, %d accounts\n",
		sessionPool.GetSessionCount(),
		dal.GetAccountCount())

	// Cleanup
	sessionPool.Stop()
	fmt.Println("Server stopped")
}
