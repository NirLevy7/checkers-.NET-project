-- Table: Games  (database: CheckersCentralDb)
-- Generated from design view
--
-- Status values: 0=InProgress, 1=HumanWon, 2=ServerWon, 3=Abandoned

CREATE TABLE [dbo].[Games] (
    [Id]               INT            NOT NULL IDENTITY(1,1),
    [StartTime]        DATETIME2      NOT NULL,
    [EndTime]          DATETIME2      NULL,        -- NULL while game is in progress
    [TimePerMoveSec]   INT            NOT NULL,    -- seconds per turn (2 / 5 / 10 / 15)
    [Status]           INT            NOT NULL,    -- see Status values above
    [NumberOfPlayers]  INT            NOT NULL,    -- 1 or 2 humans sharing the screen
    [BoardJson]        NVARCHAR(MAX)  NOT NULL,    -- serialised board state
    [MovesCount]       INT            NOT NULL,
    [CurrentTurnIndex] INT            NOT NULL,    -- round-robin index for multi-player

    CONSTRAINT [PK_Games] PRIMARY KEY ([Id])
);
