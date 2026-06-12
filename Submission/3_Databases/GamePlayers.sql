-- Table: GamePlayers  (database: CheckersCentralDb)
-- Join table: links Players to Games (many-to-many)
-- Generated from design view

CREATE TABLE [dbo].[GamePlayers] (
    [Id]        INT  NOT NULL IDENTITY(1,1),
    [GameId]    INT  NOT NULL,
    [PlayerId]  INT  NOT NULL,
    [TurnOrder] INT  NOT NULL,   -- 1-based; used for round-robin turn rotation

    CONSTRAINT [PK_GamePlayers] PRIMARY KEY ([Id]),

    -- A player can only appear once per game
    CONSTRAINT [UQ_GamePlayers_GameId_PlayerId]
        UNIQUE ([GameId], [PlayerId]),

    -- Deleting a game automatically deletes its GamePlayers rows
    CONSTRAINT [FK_GamePlayers_Games_GameId]
        FOREIGN KEY ([GameId]) REFERENCES [dbo].[Games] ([Id])
        ON DELETE CASCADE,

    -- Deleting a player automatically removes them from all games
    CONSTRAINT [FK_GamePlayers_Players_PlayerId]
        FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([Id])
        ON DELETE CASCADE
);
