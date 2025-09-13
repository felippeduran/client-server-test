package main

import (
	"fmt"
	"sync"
	"technical-test-backend/internal/core"
)

// DAL (Data Access Layer) manages persistent data in memory
type DAL struct {
	accounts map[string]AccountData // accountID -> account data
	mutex    sync.RWMutex
}

// AccountData contains all persistent data for an account
type AccountData struct {
	Account         Account              `json:"account"`
	PersistentState core.PersistentState `json:"persistentState"`
}

// NewDAL creates a new data access layer
func NewDAL() *DAL {
	return &DAL{
		accounts: make(map[string]AccountData),
	}
}

// CreateAccount creates a new account with initial state
func (d *DAL) CreateAccount(account Account, state core.PersistentState) error {
	d.mutex.Lock()
	defer d.mutex.Unlock()

	// Check if account already exists
	if _, exists := d.accounts[account.ID]; exists {
		return fmt.Errorf("account already exists")
	}

	// Create account data
	accountData := AccountData{
		Account:         account,
		PersistentState: state,
	}

	d.accounts[account.ID] = accountData
	return nil
}

// GetAccount retrieves account data by ID
func (d *DAL) GetAccount(accountID string) (AccountData, error) {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return AccountData{}, fmt.Errorf("account not found")
	}

	return accountData, nil
}

// GetAccessToken retrieves the access token for an account
func (d *DAL) GetAccessToken(accountID string) (string, error) {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return "", fmt.Errorf("account not found")
	}

	return accountData.Account.AccessToken, nil
}

// GetPersistentState retrieves the persistent state for an account
func (d *DAL) GetPersistentState(accountID string) (core.PersistentState, error) {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return core.PersistentState{}, fmt.Errorf("account not found")
	}

	// Return a copy to prevent external modifications
	state := accountData.PersistentState
	return state, nil
}

// SetPersistentState updates the persistent state for an account
func (d *DAL) SetPersistentState(accountID string, state core.PersistentState) error {
	d.mutex.Lock()
	defer d.mutex.Unlock()

	accountData, exists := d.accounts[accountID]
	if !exists {
		return fmt.Errorf("account not found")
	}

	accountData.PersistentState = state
	return nil
}

// AccountExists checks if an account exists
func (d *DAL) AccountExists(accountID string) bool {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	_, exists := d.accounts[accountID]
	return exists
}

// GetAllAccounts returns all accounts (for debugging/admin purposes)
func (d *DAL) GetAllAccounts() map[string]AccountData {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	// Return a copy to prevent external modifications
	result := make(map[string]AccountData)
	for id, data := range d.accounts {
		result[id] = data
	}
	return result
}

// GetAccountCount returns the number of accounts
func (d *DAL) GetAccountCount() int {
	d.mutex.RLock()
	defer d.mutex.RUnlock()

	return len(d.accounts)
}
