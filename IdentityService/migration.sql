USE IdentityDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetRequests')
BEGIN
    CREATE TABLE PasswordResetRequests (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(255) NOT NULL,
        ResetCode NVARCHAR(100) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        UsedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1
    );
    
    CREATE INDEX IX_PasswordResetRequests_Email 
    ON PasswordResetRequests(Email);
    
    PRINT '✅ PasswordResetRequests table created';
END
ELSE
BEGIN
    PRINT '✅ PasswordResetRequests table already exists';
END
GO
-- Add migration history
IF EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20251222182731_AddPasswordResetRequestTable')
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES ('20251222182731_AddPasswordResetRequestTable', '8.0.22');
        PRINT '✅ Migration history added';
    END
END
GO
