-- Run against MasakingDB before using the member activity editors.
-- Existing general challenges remain unlinked; no recipe is guessed or reassigned.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.challenges', 'recipeID') IS NULL
    ALTER TABLE dbo.challenges ADD recipeID int NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_challenges_recipes')
    EXEC(N'ALTER TABLE dbo.challenges WITH CHECK ADD CONSTRAINT FK_challenges_recipes
        FOREIGN KEY(recipeID) REFERENCES dbo.recipes(recipeID)');
COMMIT;
