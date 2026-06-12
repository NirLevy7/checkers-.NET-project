-- Table: ReplayMoves  (database: CheckersReplayDb)
-- Stores every move of a saved game, in order
-- Generated from design view

CREATE TABLE [dbo].[ReplayMoves] (
    [Id]           INT            NOT NULL IDENTITY(1,1),
    [ReplayGameId] INT            NOT NULL,
    [MoveNumber]   INT            NOT NULL,
    [Side]         NVARCHAR(MAX)  NOT NULL,   -- "Human" or "Server"
    [FromRow]      INT            NOT NULL,
    [FromCol]      INT            NOT NULL,
    [ToRow]        INT            NOT NULL,
    [ToCol]        INT            NOT NULL,
    [CapturedRow]  INT            NULL,        -- NULL if not a capture
    [CapturedCol]  INT            NULL,
    [IsBackward]   BIT            NOT NULL,

    CONSTRAINT [PK_ReplayMoves] PRIMARY KEY ([Id]),

    -- Deleting a ReplayGame deletes all its moves
    CONSTRAINT [FK_ReplayMoves_ReplayGames_ReplayGameId]
        FOREIGN KEY ([ReplayGameId]) REFERENCES [dbo].[ReplayGames] ([Id])
        ON DELETE CASCADE
);
