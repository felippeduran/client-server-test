Notes

It's not yet clear if I should use a websocket or HTTP API for the server.

The pros of using a websocket are:
- It's easier to implement session management
- Supports bi-directional communication

The cons of using a websocket are:
- It's more complex to implement message handling

The pros of using a HTTP API are:
- It's easier to implement message handling

The cons of using a HTTP API are:
- Only supports uni-directional communication
- It's more complex to implement session management (sliding windows management can be a problem if we refresh it too often)

I'm leaning towards using a websocket.




Tasks:
* Implement UI navigation
* Implement data-binding for UI
* Implement player actions
* Implement core framework to trigger requests and execute client-side changes
* Implement connection management
* Implement server setup
* Implement server authentication/account creation
* Implement player actions endpoints and in-memory player state



Endpoints
* CreateAccount
* Authenticate
* GetPlayerState
* GetConfigs
* StartLevel
* CompleteLevel




All interactions:

