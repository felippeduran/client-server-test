//go:build integration
// +build integration

package integration

import (
	"context"
	"net"
	"net/http"
	"technical-test-backend/internal/app"
	"technical-test-backend/internal/errors"
	"technical-test-backend/internal/session/memory"
	"technical-test-backend/internal/usecases/commands"
	"technical-test-backend/internal/usecases/configs"
	"testing"
	"time"

	"github.com/stretchr/testify/assert"
)

func TestAuthentication_Success(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("test", "test")
	assert.NoError(t, err)

	assert.NotEmpty(t, sessionID)
}

func TestAuthentication_InvalidAccountID(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("", "token")
	assert.Error(t, err)
	assert.Equal(t, err.(*httpError).StatusCode, http.StatusBadRequest)

	assert.Empty(t, sessionID)
}

func TestAuthentication_InvalidToken(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("account_id", "")
	assert.Error(t, err)
	assert.Equal(t, err.(*httpError).StatusCode, http.StatusBadRequest)

	assert.Empty(t, sessionID)
}

func TestGetPlayerState_Success(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("test", "test")
	assert.NoError(t, err)

	state, err := client.GetPlayerState(sessionID)
	assert.NoError(t, err)

	assert.NotEmpty(t, state)
}

func TestGetPlayerState_Unauthorized(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	state, err := client.GetPlayerState("invalid-session-id")
	assert.Empty(t, state)
	assert.Error(t, err)
	assert.Equal(t, err.(*httpError).StatusCode, http.StatusUnauthorized)
}

func TestBeginLevel_Success(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("test", "test")
	assert.NoError(t, err)

	err = client.BeginLevel(sessionID, 1)
	assert.NoError(t, err)
}

func TestBeginLevel_Unauthorized(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	err = client.BeginLevel("invalid-session-id", 1)
	assert.Error(t, err)
	assert.Equal(t, err.(*httpError).StatusCode, http.StatusUnauthorized)
}

func TestEndLevel_Success(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	sessionID, err := client.Authenticate("test", "test")
	assert.NoError(t, err)

	err = client.BeginLevel(sessionID, 1)
	assert.NoError(t, err)

	err = client.EndLevel(sessionID, true, 100)
	assert.NoError(t, err)
}

func TestEndLevel_Unauthorized(t *testing.T) {
	config, err := GetDefaultTestConfig()
	assert.NoError(t, err)

	_, stop := runServer(t, config)
	defer stop()

	client := NewTestClient(config.Port)

	err = client.EndLevel("invalid-session-id", true, 100)
	assert.Error(t, err)
	assert.Equal(t, err.(*httpError).StatusCode, http.StatusUnauthorized)
}

func runServer(t *testing.T, config app.Config) (*app.HTTP, func()) {
	server := app.NewHTTP(config)
	go server.Run()
	return server, func() {
		assert.NoError(t, server.Stop(context.Background()))
	}
}

func GetDefaultTestConfig() (app.Config, error) {
	port, err := getFreePort()
	if err != nil {
		panic(err)
	}

	return app.Config{
		Port: port,
		SessionPool: memory.SessionPoolConfig{
			TTL: 30 * time.Second,
		},
		ConfigProvider: configs.ProviderConfig{
			FilePath: "../config/game_config.json",
		},
		Commands: commands.Config{
			MaxTimeDifferenceSeconds: 1,
		},
	}, nil
}

func getFreePort() (int, error) {
	l, err := net.Listen("tcp", ":0")
	if err != nil {
		return 0, errors.Wrapf(err, "failed to listen on port")
	}
	defer l.Close()
	return l.Addr().(*net.TCPAddr).Port, nil
}
