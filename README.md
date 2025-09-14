# Client-Server Technical Test

The project is divided into two main folders, client and server.

TODO:
- FIX BUG DE ENERGIA!!

## Client Project

The `Bootstrap.scene` and `Bootstrap.cs` are the entry point for the project. To run the game you can just hit play with this scene open.

The game can be run in two different modes: with or without a fake server. You can configure this in the `Bootstrap` game object inspector. The game also contains a simulated throttling feature to help out development and test, and is configured in the same place.

The project uses `Unity 6000.0.26f1`.

For running the Golang server, refer to [Server Project](#server-project).

### General Structure

The client project code follows the original template of a layers-first folder structure. Additionally, each layer might be broken down into Runtime, Tests and Editor assemblies.

```
Scripts/
├── Application/
│   ├── Editor/
│   │   └── Application.Editor.asmdef
│   └── Runtime/
│       └── Application.Runtime.asmdef
├── Core/
│   ├── Runtime/
│   │   └── Core.Runtime.asmdef
│   └── Tests/
│       └── Core.Tests.asmdef
├── LocalServer/
├── Networking/
│   ├── Runtime/
│   │   ├── Fake/
│   │   └── Http/
│   └── Tests/
├── Presentation/
├── Services/
├── UseCases/
└── Utilities/
```

Here's a brief explanation of different layers:
* **Application**: this layer configures initializes the application. Depends on most other assemblies due to that.
* **Presentation**: as originally defined by the template, this layer is the only one that interacts directly with UnityEngine API and is responsible for defining view/screens behavior.
* **Core**: responsible for the business logic of different player actions and metagame features like managing energy and level progression.
* **LocalServer**: is a C# fake server implementation of the client-server API that leverages the Fake client implementation from the **Networking** layer.
* **Networking**: provides the `IClient` interface for communication with the server API and has two different implementations: HTTP and Fake.
* **Services**: implements the different API actions/endpoints without knowledge of the underlying communication protocol.
* **UseCases**: implements the general high-level application flow. 

Following a layered architecture approach, dependencies point inwards, with the **Presentation** layer depending on **Core** and **Use Case** layers, for example. The **Core**, being one of the inner-most layers, doesn't have any external dependencies.

## Server Project

The Golang project has a `Makefile` for running the server, tests and linting. You can check available commands with `make help`, but to start the server, simple run `make run`.

### General structure

Differently from the client project, apart from the root folders, the server uses a feature-first structure for folders/packages:

```
cmd/
└── server/
config/
integration/
internal/
├── app/
├── core/
├── errors/
├── http/
├── sessions/
│   └── memory/
└── usecases/
    ├── authentication/
    │   └── http/
    ├── players/
    │   ├── dal/
    │   │   └── memory/
    │   └── http/
    └── ...
└── worker/
```

* `cmd/server`: is the `main` package for the server application.
* `config`: contains the config files for the project (`game_config.json`).
* `integration`: integration tests.
* `internal/app`: contains the http server initialization, with endpoints and handlers setup.
* `internal/core`: contains the core business logic and command implementations.
* `internal/errors`: utilities for wrapping and formatting errors.
* `internal/http`: HTTP middleware and utilities.
* `internal/sessions`: session management interfaces and implementations.
* `internal/usecases`: feature-specific use cases organized by domain.
* `internal/worker`: background worker implementation (used by `internal/sessions`). 

## Metagame Architecture Design

The metagame was implemented following a client-side prediction model that allows for the general game code assume that player actions are all synchronous. Every game action, like `BeginLevel` or `EndLevel`, is an `ICommand` implementation that get executed immediately in the client and also gets placed in an `ExecutionQueue`. Then, an independent `CommandSender` polls from the queue and sends pending commands to the server, which in turn execute the same actions server-side. If then a connection or server error happens, the `CommandSender` flags the issue and the game handles the error in a centralized way. Currently, the behavior is to restart and resynchronize the game.

In order to ensure the game state is exactly the same in the client and server, the game has two main characteristics. First, for time-dependent features, like the **Energy system**, the local state doesn't change periodically. Instead, the state uses timestamps, and the "actual/predicted" values can be obtained through helper methods.

Considering the variability of latency and possibility of cheating, the client also synchronizes its clock with a reference server timestamp through the `OffsetClock` implementation. This clock is then used to execute `ITimedCommand`s, which carry the timestamp for the server to use when executing its code. With this approach, it's possible to guarantee that the state will be exactly the same on client and server. Finally, to avoid any cheating possibility, the server also validates whether received timestamps are within latency limits of its current time.

The main disadvantage of this design is that, given client and server use different languages, commands need to be implemented twice.


## Communication Protocol Implementation

Different protocols like gRPC, HTTP and a custom one on top of TCP were considered here. Given the existing game specification didn't require duplex communication, and was quite latency tolerant, it was favored a simple implementation on top of HTTP. However, the high-level interface (IClient) and current code architecture still allows for other implementations.

Considering an HTTP API doesn't hold connections, a session id/token is generated during authentication and returned as a `X-Session-Id` header. This header is then used for later calls to associate them to the same "session". In the server, a mapping between the session and account ids is then used to identify the player.

In order to keep the session alive, the server expects the client to send heartbeats or other requests every 10 seconds. If no requests arrive, the session is then removed and upcoming calls will be rejected. If another client authenticates using the same account, any existing sessions for that account are removed, effectively "kicking" older clients.


## API Definition

The server exposes an RPC API over HTTP with the following endpoints:

### Base URL
- **Development**: `http://localhost:8080`
- **Health Check**: `GET /health`

### Endpoints

#### 1. Authentication
- **URL**: `POST /AuthenticationHandler/Authenticate`
- **Authentication**: None required
- **Description**: Authenticates a player and creates a session
- **Request Body**: Account id and access token
- **Response Headers**:
  - `X-Session-ID`: Session identifier for subsequent requests
- **Response Body**: Empty object

#### 2. Get Player State
- **URL**: `POST /InitializationHandler/GetPlayerState`
- **Authentication**: Required (`X-Session-ID`)
- **Description**: Retrieves the current player state including energy, level progression, and session data
- **Request Body**: Empty object
- **Response**: Persistent and session state (i.e.: is playing a level)

#### 3. Get Game Configs
- **URL**: `POST /InitializationHandler/GetConfigs`
- **Authentication**: Required (`X-Session-ID`)
- **Description**: Retrieves game configuration including energy settings and level configurations
- **Request Body**: Empty object
- **Response**: Configuration data

#### 4. Execute Command
- **URL**: `POST /CommandHandler/HandleCommand`
- **Authentication**: Required (`X-Session-ID`)
- **Description**: Executes game commands (BeginLevel, EndLevel)
- **Request Body**: Command name and payload
- **Response**: Empty object on success

#### 5. Heartbeat
- **URL**: `POST /HeartbeatHandler/Heartbeat`
- **Authentication**: Required (`X-Session-ID`)
- **Description**: Keeps the session alive and updates last activity time
- **Request Body**: Empty object
- **Response**: Empty object

### Error Handling

All endpoints return JSON responses with the following error format:
```json
{ "message": "error description" }
```

**Common HTTP Status Codes**:
- `200 OK`: Success
- `401 Unauthorized`: Invalid/expired session id 
- `400 Bad Request`: Invalid request data or missing required fields
- `500 Internal Server Error`: Server-side error


## Potential Improvements

Here's a small list of potential improvements for the project:

* Replay of queue commands in case of connectivity issues.
* Better, more granular handling of specific errors, i.e.: for "no energy" or "level not unlock".
* Implementation of gRPC or custom protocol for duplex communication.
* Make `LocalServer` from the client project share the same config from the server.
* Implement server-side command execution in C# to avoid duplicate implementation.
* Add compression support to player state and configs endpoints.
* Consider using other serialization formats, like Protobuf.
* Add support for HTTPS.