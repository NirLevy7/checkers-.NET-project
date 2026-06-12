-- Table: ReplayGames  (database: CheckersReplayDb)
-- Local client-side database — stores finished games for replay playback
-- Generated from design view

CREATE TABLE [dbo].[ReplayGames] (
    [Id]             INT            NOT NULL IDENTITY(1,1),
    [ServerGameId]   INT            NOT NULL,   -- ID of the game on the central server
    [PlayersJson]    NVARCHAR(MAX)  NOT NULL,   -- JSON array of player first names
    [StartTime]      DATETIME2      NOT NULL,
    [DurationSec]    INT            NOT NULL,
    [TimePerMoveSec] INT            NOT NULL,
    [WinnerSide]     NVARCHAR(MAX)  NOT NULL,   -- "Human" or "Server"

    CONSTRAINT [PK_ReplayGames] PRIMARY KEY ([Id])
);
