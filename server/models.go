package main

// Core data structures matching the C# client models

// Account represents a user account
type Account struct {
	ID          string `json:"id"`
	AccessToken string `json:"accessToken"`
}
