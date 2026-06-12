-- Table: Players  (database: CheckersCentralDb)
-- Generated from design view

CREATE TABLE [dbo].[Players] (
    [Id]           INT            NOT NULL,   -- user-chosen (NOT auto-generated)
    [FirstName]    NVARCHAR(MAX)  NOT NULL,
    [Phone]        NVARCHAR(MAX)  NOT NULL,
    [CountryId]    INT            NOT NULL,
    [RegisteredAt] DATETIME2      NOT NULL,

    CONSTRAINT [PK_Players] PRIMARY KEY ([Id]),

    -- ID must be between 1 and 1000
    CONSTRAINT [CK_Player_Id]
        CHECK ([Id] BETWEEN 1 AND 1000),

    -- Phone must be exactly 10 digits, no letters
    CONSTRAINT [CK_Player_Phone]
        CHECK (LEN([Phone]) = 10 AND [Phone] NOT LIKE '%[^0-9]%'),

    -- Each player belongs to one country; cannot delete a country that has players
    CONSTRAINT [FK_Players_Countries_CountryId]
        FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Countries] ([Id])
        ON DELETE NO ACTION
);
