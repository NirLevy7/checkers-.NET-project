-- ================================================================
--  TEST DATA for CheckersCentralDb
--  How to run:
--    1. Open SSMS → connect to (LocalDB)\MSSQLLocalDB
--    2. Select database: CheckersCentralDb  (top-left dropdown)
--    3. Open this file and press F5
-- ================================================================

USE CheckersCentralDb;
GO

-- ── Step 1: Clear everything in the right FK order ───────────────
DELETE FROM GamePlayers;
PRINT 'Cleared GamePlayers: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

DELETE FROM Games;
PRINT 'Cleared Games: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

DELETE FROM Players;
PRINT 'Cleared Players: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

-- ── Step 2: Make sure Countries are seeded ────────────────────────
-- (EF migrations seed these, but if the DB was created manually they may be missing)
IF NOT EXISTS (SELECT 1 FROM Countries WHERE Id = 1)
BEGIN
    INSERT INTO Countries (Id, Name) VALUES
    (1, 'Israel'), (2, 'USA'),    (3, 'UK'),
    (4, 'France'), (5, 'Germany'),(6, 'Spain'),
    (7, 'Italy'),  (8, 'Russia');
    PRINT 'Countries seeded.';
END
ELSE
    PRINT 'Countries already exist — skipped.';

-- ── Step 3: Players ───────────────────────────────────────────────
-- Id: 1–1000 (player chooses their own ID)
-- Phone: exactly 10 digits
INSERT INTO Players (Id, FirstName, Phone, CountryId, RegisteredAt) VALUES
(1,  'Nir',   '0501234567', 1, '2025-12-01'),   -- Israel
(2,  'Avi',   '0521234567', 1, '2025-12-02'),   -- Israel
(3,  'Dana',  '0531234567', 1, '2025-12-03'),   -- Israel
(4,  'John',  '0541234567', 2, '2025-12-04'),   -- USA
(5,  'Emma',  '0551234567', 2, '2025-12-05'),   -- USA
(6,  'James', '0561234567', 3, '2025-12-06'),   -- UK
(7,  'Marie', '0571234567', 4, '2025-12-07'),   -- France
(8,  'Klaus', '0581234567', 5, '2025-12-08');   -- Germany — never plays (needed for §28 "did not play")
PRINT 'Players inserted: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

-- ── Step 4: Games ─────────────────────────────────────────────────
-- Status: 1=HumanWon  2=ServerWon
-- BoardJson: 64 zeros (32 cells × 2 values — queries never read board content)
DECLARE @board NVARCHAR(MAX) = N'[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]';

DECLARE @g1 INT, @g2 INT, @g3 INT, @g4 INT, @g5 INT;

INSERT INTO Games (StartTime, EndTime, TimePerMoveSec, Status, NumberOfPlayers, BoardJson, MovesCount, CurrentTurnIndex)
VALUES ('2026-01-01 10:00', '2026-01-01 10:15', 10, 1, 2, @board, 12, 0);
SET @g1 = SCOPE_IDENTITY();

INSERT INTO Games (StartTime, EndTime, TimePerMoveSec, Status, NumberOfPlayers, BoardJson, MovesCount, CurrentTurnIndex)
VALUES ('2026-01-05 14:00', '2026-01-05 14:20', 5,  2, 2, @board, 18, 0);
SET @g2 = SCOPE_IDENTITY();

INSERT INTO Games (StartTime, EndTime, TimePerMoveSec, Status, NumberOfPlayers, BoardJson, MovesCount, CurrentTurnIndex)
VALUES ('2026-01-10 09:00', '2026-01-10 09:10', 10, 1, 1, @board, 8,  0);
SET @g3 = SCOPE_IDENTITY();

INSERT INTO Games (StartTime, EndTime, TimePerMoveSec, Status, NumberOfPlayers, BoardJson, MovesCount, CurrentTurnIndex)
VALUES ('2026-01-15 16:00', '2026-01-15 16:25', 15, 2, 2, @board, 22, 0);
SET @g4 = SCOPE_IDENTITY();

INSERT INTO Games (StartTime, EndTime, TimePerMoveSec, Status, NumberOfPlayers, BoardJson, MovesCount, CurrentTurnIndex)
VALUES ('2026-01-20 11:00', '2026-01-20 11:30', 10, 1, 2, @board, 25, 0);
SET @g5 = SCOPE_IDENTITY();

PRINT 'Games inserted. IDs: ' +
    CAST(@g1 AS VARCHAR) + ', ' + CAST(@g2 AS VARCHAR) + ', ' +
    CAST(@g3 AS VARCHAR) + ', ' + CAST(@g4 AS VARCHAR) + ', ' + CAST(@g5 AS VARCHAR);

-- ── Step 5: GamePlayers ───────────────────────────────────────────
INSERT INTO GamePlayers (GameId, PlayerId, TurnOrder) VALUES
-- Game 1 (Jan 1):  Nir + Avi          → Israel × 2
(@g1, 1, 1), (@g1, 2, 2),
-- Game 2 (Jan 5):  Nir + Dana         → Israel × 2
(@g2, 1, 1), (@g2, 3, 2),
-- Game 3 (Jan 10): John alone         → USA × 1
(@g3, 4, 1),
-- Game 4 (Jan 15): Emma + James       → USA × 1, UK × 1
(@g4, 5, 1), (@g4, 6, 2),
-- Game 5 (Jan 20): Avi + Dana + Marie → Israel × 2, France × 1
(@g5, 2, 1), (@g5, 3, 2), (@g5, 7, 3);
PRINT 'GamePlayers inserted: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows';

-- ── Summary ───────────────────────────────────────────────────────
DECLARE @cC INT, @cP INT, @cG INT, @cGP INT;
SELECT @cC = COUNT(*) FROM Countries;
SELECT @cP = COUNT(*) FROM Players;
SELECT @cG = COUNT(*) FROM Games;
SELECT @cGP = COUNT(*) FROM GamePlayers;
PRINT '';
PRINT '=== Done! Final counts ===';
PRINT 'Countries  : ' + CAST(@cC  AS VARCHAR);
PRINT 'Players    : ' + CAST(@cP  AS VARCHAR);
PRINT 'Games      : ' + CAST(@cG  AS VARCHAR);
PRINT 'GamePlayers: ' + CAST(@cGP AS VARCHAR);
GO
