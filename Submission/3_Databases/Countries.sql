-- Table: Countries  (database: CheckersCentralDb)
-- Generated from design view

CREATE TABLE [dbo].[Countries] (
    [Id]   INT            NOT NULL IDENTITY(1,1),
    [Name] NVARCHAR(450)  NOT NULL,

    CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Countries_Name] ON [dbo].[Countries] ([Name]);
