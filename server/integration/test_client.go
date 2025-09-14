package integration

import (
	"bytes"
	"encoding/json"
	"fmt"
	"net/http"
	"technical-test-backend/internal/core/commands"
	usecasesauthentication "technical-test-backend/internal/usecases/authentication"
	usecasescommands "technical-test-backend/internal/usecases/commands"
	usecasesconfigs "technical-test-backend/internal/usecases/configs"
	usecasesplayers "technical-test-backend/internal/usecases/players"
	"time"
)

type TestClient struct {
	Client  *http.Client
	BaseURL string
}

func NewTestClient(port int) *TestClient {
	return &TestClient{
		Client:  &http.Client{},
		BaseURL: fmt.Sprintf("http://localhost:%d", port),
	}
}

type errorResponse struct {
	Message string `json:"message"`
}

func parseErrorResponse(resp *http.Response) error {
	defer resp.Body.Close()
	var errResp errorResponse
	if err := json.NewDecoder(resp.Body).Decode(&errResp); err == nil && errResp.Message != "" {
		return &httpError{StatusCode: resp.StatusCode, Message: errResp.Message}
	}
	return &httpError{StatusCode: resp.StatusCode, Message: ""}
}

func (tc *TestClient) Authenticate(accountID, accessToken string) (string, error) {
	req := usecasesauthentication.AuthenticateArgs{
		AccountID:   accountID,
		AccessToken: accessToken,
	}

	reqBody, _ := json.Marshal(req)
	resp, err := tc.Client.Post(tc.BaseURL+"/AuthenticationHandler/Authenticate",
		"application/json", bytes.NewBuffer(reqBody))
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", parseErrorResponse(resp)
	}

	return resp.Header.Get("X-Session-ID"), nil
}

func (tc *TestClient) GetPlayerState(sessionID string) (usecasesplayers.GetPlayerStateRes, error) {
	reqBody := []byte("{}")
	req, _ := http.NewRequest("POST", tc.BaseURL+"/InitializationHandler/GetPlayerState",
		bytes.NewBuffer(reqBody))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-Session-ID", sessionID)

	resp, err := tc.Client.Do(req)
	if err != nil {
		return usecasesplayers.GetPlayerStateRes{}, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return usecasesplayers.GetPlayerStateRes{}, parseErrorResponse(resp)
	}

	var stateResp usecasesplayers.GetPlayerStateRes
	if err := json.NewDecoder(resp.Body).Decode(&stateResp); err != nil {
		return usecasesplayers.GetPlayerStateRes{}, err
	}

	return stateResp, nil
}

func (tc *TestClient) GetConfigs(sessionID string) (usecasesconfigs.GetConfigsRes, error) {
	reqBody := []byte("{}")
	req, _ := http.NewRequest("POST", tc.BaseURL+"/InitializationHandler/GetConfigs",
		bytes.NewBuffer(reqBody))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-Session-ID", sessionID)

	resp, err := tc.Client.Do(req)
	if err != nil {
		return usecasesconfigs.GetConfigsRes{}, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return usecasesconfigs.GetConfigsRes{}, parseErrorResponse(resp)
	}

	var configsResp usecasesconfigs.GetConfigsRes
	if err := json.NewDecoder(resp.Body).Decode(&configsResp); err != nil {
		return usecasesconfigs.GetConfigsRes{}, err
	}

	return configsResp, nil
}

func (tc *TestClient) BeginLevel(sessionID string, levelID int) error {
	beginCmd := commands.BeginLevel{
		LevelID: levelID,
		Now:     time.Now(),
	}

	beginData, _ := json.Marshal(beginCmd)

	cmd := usecasescommands.CommandArgs{
		Command: "BeginLevel",
		Data:    beginData,
	}

	reqBody, _ := json.Marshal(cmd)
	req, _ := http.NewRequest("POST", tc.BaseURL+"/CommandHandler/HandleCommand",
		bytes.NewBuffer(reqBody))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-Session-ID", sessionID)

	resp, err := tc.Client.Do(req)
	if err != nil {
		return err
	}
	if resp.StatusCode != http.StatusOK {
		return parseErrorResponse(resp)
	}
	return nil
}

func (tc *TestClient) EndLevel(sessionID string, success bool, score int) error {
	endCmd := commands.EndLevel{
		Success: success,
		Score:   score,
	}

	endData, _ := json.Marshal(endCmd)

	cmd := usecasescommands.CommandArgs{
		Command: "EndLevel",
		Data:    endData,
	}

	reqBody, _ := json.Marshal(cmd)
	req, _ := http.NewRequest("POST", tc.BaseURL+"/CommandHandler/HandleCommand",
		bytes.NewBuffer(reqBody))
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-Session-ID", sessionID)

	resp, err := tc.Client.Do(req)
	if err != nil {
		return err
	}
	if resp.StatusCode != http.StatusOK {
		return parseErrorResponse(resp)
	}
	return nil
}

type httpError struct {
	StatusCode int
	Message    string
}

func (e *httpError) Error() string {
	return e.Message
}
