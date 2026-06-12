using CheckersGame.Data.Context;
using CheckersGame.Data.Entities;
using CheckersGame.Shared.Enums;

namespace CheckersGame.Web;

// Fills the database with sample data so all queries show results at the defense.
// Only runs if the Players table is empty — safe to call on every startup.
public static class DbSeeder
{
    public static void Seed(CheckersCentralDb db)
    {
        if (db.Players.Any()) return;  // already seeded

        // ── Players ───────────────────────────────────────────────────────
        var players = new List<Player>
        {
            new() { Id = 1,  FirstName = "Alice",   Phone = "0501111111", CountryId = 1, RegisteredAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = 2,  FirstName = "Bob",     Phone = "0502222222", CountryId = 2, RegisteredAt = DateTime.UtcNow.AddDays(-9)  },
            new() { Id = 3,  FirstName = "Charlie", Phone = "0503333333", CountryId = 3, RegisteredAt = DateTime.UtcNow.AddDays(-8)  },
            new() { Id = 4,  FirstName = "Diana",   Phone = "0504444444", CountryId = 4, RegisteredAt = DateTime.UtcNow.AddDays(-7)  },
            new() { Id = 5,  FirstName = "Eve",     Phone = "0505555555", CountryId = 1, RegisteredAt = DateTime.UtcNow.AddDays(-6)  },
            new() { Id = 6,  FirstName = "Frank",   Phone = "0506666666", CountryId = 2, RegisteredAt = DateTime.UtcNow.AddDays(-5)  },
            new() { Id = 7,  FirstName = "Grace",   Phone = "0507777777", CountryId = 5, RegisteredAt = DateTime.UtcNow.AddDays(-4)  },
            new() { Id = 8,  FirstName = "Henry",   Phone = "0508888888", CountryId = 1, RegisteredAt = DateTime.UtcNow.AddDays(-3)  },
            new() { Id = 9,  FirstName = "alice",   Phone = "0509999999", CountryId = 3, RegisteredAt = DateTime.UtcNow.AddDays(-2)  }, // same name as Id=1 (tests case-insensitive dedup in Q26)
            new() { Id = 10, FirstName = "Nir",     Phone = "0521234567", CountryId = 1, RegisteredAt = DateTime.UtcNow.AddDays(-1)  },
        };
        db.Players.AddRange(players);
        db.SaveChanges();

        // ── Games ─────────────────────────────────────────────────────────
        var games = new List<Game>
        {
            // Game 1: Alice vs Server — Human won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-9), EndTime = DateTime.UtcNow.AddDays(-9).AddMinutes(12),
                TimePerMoveSec = 10, Status = GameStatus.HumanWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 20, CurrentTurnIndex = 0
            },
            // Game 2: Bob vs Server — Server won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-8), EndTime = DateTime.UtcNow.AddDays(-8).AddMinutes(8),
                TimePerMoveSec = 5,  Status = GameStatus.ServerWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 14, CurrentTurnIndex = 0
            },
            // Game 3: Alice + Charlie sharing screen — Human won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-7), EndTime = DateTime.UtcNow.AddDays(-7).AddMinutes(20),
                TimePerMoveSec = 15, Status = GameStatus.HumanWon, NumberOfPlayers = 2,
                BoardJson = "", MovesCount = 30, CurrentTurnIndex = 0
            },
            // Game 4: Diana vs Server — Human won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-6), EndTime = DateTime.UtcNow.AddDays(-6).AddMinutes(10),
                TimePerMoveSec = 10, Status = GameStatus.HumanWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 18, CurrentTurnIndex = 0
            },
            // Game 5: Bob vs Server — Human won (Bob's second game)
            new() {
                StartTime = DateTime.UtcNow.AddDays(-5), EndTime = DateTime.UtcNow.AddDays(-5).AddMinutes(15),
                TimePerMoveSec = 10, Status = GameStatus.HumanWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 22, CurrentTurnIndex = 0
            },
            // Game 6: Frank vs Server — Server won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-4), EndTime = DateTime.UtcNow.AddDays(-4).AddMinutes(6),
                TimePerMoveSec = 2,  Status = GameStatus.ServerWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 10, CurrentTurnIndex = 0
            },
            // Game 7: Alice's third game — Server won
            new() {
                StartTime = DateTime.UtcNow.AddDays(-3), EndTime = DateTime.UtcNow.AddDays(-3).AddMinutes(9),
                TimePerMoveSec = 10, Status = GameStatus.ServerWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 16, CurrentTurnIndex = 0
            },
            // Game 8: Grace vs Server — Human won (Germany)
            new() {
                StartTime = DateTime.UtcNow.AddDays(-2), EndTime = DateTime.UtcNow.AddDays(-2).AddMinutes(11),
                TimePerMoveSec = 10, Status = GameStatus.HumanWon, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 19, CurrentTurnIndex = 0
            },
            // Game 9: still in progress — Nir
            new() {
                StartTime = DateTime.UtcNow.AddHours(-1), EndTime = null,
                TimePerMoveSec = 10, Status = GameStatus.InProgress, NumberOfPlayers = 1,
                BoardJson = "", MovesCount = 4, CurrentTurnIndex = 0
            },
        };
        db.Games.AddRange(games);
        db.SaveChanges();

        // ── GamePlayers (who played in each game) ─────────────────────────
        var gamePlayersList = new List<GamePlayer>
        {
            new() { GameId = games[0].Id, PlayerId = 1,  TurnOrder = 1 }, // Alice in game 1
            new() { GameId = games[1].Id, PlayerId = 2,  TurnOrder = 1 }, // Bob in game 2
            new() { GameId = games[2].Id, PlayerId = 1,  TurnOrder = 1 }, // Alice in game 3
            new() { GameId = games[2].Id, PlayerId = 3,  TurnOrder = 2 }, // Charlie in game 3
            new() { GameId = games[3].Id, PlayerId = 4,  TurnOrder = 1 }, // Diana in game 4
            new() { GameId = games[4].Id, PlayerId = 2,  TurnOrder = 1 }, // Bob in game 5
            new() { GameId = games[5].Id, PlayerId = 6,  TurnOrder = 1 }, // Frank in game 6
            new() { GameId = games[6].Id, PlayerId = 1,  TurnOrder = 1 }, // Alice in game 7
            new() { GameId = games[7].Id, PlayerId = 7,  TurnOrder = 1 }, // Grace in game 8
            new() { GameId = games[8].Id, PlayerId = 10, TurnOrder = 1 }, // Nir in game 9
        };
        // Note: Eve (5), Henry (8), alice (9) are registered but never played — shows in Q28 "Did not play" group
        db.GamePlayers.AddRange(gamePlayersList);
        db.SaveChanges();
    }
}
