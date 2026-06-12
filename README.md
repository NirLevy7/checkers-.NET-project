# Checkers .NET — Networked Checkers Game

A full-stack networked checkers game built in C# / .NET 8 as a semester final project.  
Covers WinForms desktop client, ASP.NET Core Web API, Razor Pages website, Entity Framework Core, and SQL Server.

---

## Architecture

The solution is split into 5 projects:

| Project | Type | Role |
|---|---|---|
| `CheckersGame.Client` | WinForms | Desktop game client — board UI, timers, animations, local replay DB |
| `CheckersGame.Api` | ASP.NET Core Web API | Game engine — move validation, server AI, game state |
| `CheckersGame.Web` | ASP.NET Core Razor Pages | Registration website, player management, query pages |
| `CheckersGame.Data` | Class Library | EF Core DbContext, entities, migrations — shared by API and Web |
| `CheckersGame.Shared` | Class Library | DTOs, BoardModel, BoardSerializer, enums — shared by API and Client |

---

## Features

- **4×8 checkers board** — pieces on dark squares only, custom movement rules
- **Special backward move** — each piece may move backward once (tracked per piece)
- **Capture jumps** — jump diagonally over an enemy piece to capture it
- **Server AI** — picks a random legal move, preferring captures
- **Multiple human players** — round-robin turn system (1–10 players)
- **Per-turn countdown timer** — configurable (2 / 5 / 10 / 15 seconds)
- **Win animation** — winning pieces blink green for 5 seconds
- **Free drawing overlay** — right-click drag to draw on the board
- **Game replay** — all games saved locally to SQL Server LocalDB, viewable anytime
- **Player registration** — register on the website before playing
- **Query pages** — 9 different data queries (top countries, games per player, etc.)

---

## Tech Stack

- **C# / .NET 8**
- **WinForms** — desktop UI, GDI+ graphics, custom animations
- **ASP.NET Core Web API** — RESTful game endpoints
- **ASP.NET Core Razor Pages** — server-rendered website
- **Entity Framework Core** — ORM with code-first migrations
- **SQL Server** — central game database
- **SQL Server LocalDB** — client-side replay database
- **System.Text.Json** — board serialization

---

## How It Works

```
Browser  →  Razor Pages Website  →  Central SQL Server DB
                                           ↑
                               ASP.NET Core Web API
                                           ↑
WinForms Client  ──────────────────────────┘
       ↓
   LocalDB (replays)
```

1. Players **register** on the website (choose ID 1–1000, name, phone, country)
2. Players open the **WinForms client**, enter their IDs, choose time limit, start game
3. Client verifies IDs against the API, then starts a game session
4. Each human move is **sent to the API** → validated → server responds with its own move
5. Game state (board) is serialized to JSON and persisted in the DB between every move
6. Finished games are saved to the **local replay DB** for later viewing

---

## Project Structure

```
src/
├── CheckersGame.Client/
│   ├── Forms/          # BoardForm, GameSetupForm, ReplayListForm, ReplayBoardForm
│   ├── ApiClient/      # HTTP wrapper around the Web API
│   ├── GameEngine/     # Client-side move validator (UI highlighting only)
│   └── ReplayDb/       # EF Core LocalDB context + entities
├── CheckersGame.Api/
│   ├── Controllers/    # GamesController, PlayersController
│   ├── Services/       # GameService — core game logic
│   └── GameEngine/     # MoveValidator — authoritative rules engine
├── CheckersGame.Web/
│   └── Pages/          # Register, EditPlayer, Delete + 9 query pages
├── CheckersGame.Data/
│   ├── Context/        # CheckersCentralDb (EF Core DbContext)
│   ├── Entities/       # Player, Game, GamePlayer, Country
│   └── Migrations/     # EF Core migration history
└── CheckersGame.Shared/
    ├── Models/         # BoardModel
    ├── Dtos/           # Request/Response DTOs
    ├── Enums/          # Side, GameStatus
    └── BoardSerializer.cs
```

---

## Running the Project

1. **Start SQL Server** (LocalDB works for development)
2. **Apply migrations** — run in `CheckersGame.Data`:
   ```
   dotnet ef database update
   ```
3. **Run the API** — `CheckersGame.Api` (default: `https://localhost:7025`)
4. **Run the Website** — `CheckersGame.Web`
5. **Register players** on the website
6. **Run the Client** — `CheckersGame.Client`, enter player IDs, start game
