-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IdentityDb')
    CREATE DATABASE IdentityDb;
GO

USE IdentityDb;
GO

-- Create table
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
    CREATE INDEX IX_PasswordResetRequests_Email ON PasswordResetRequests(Email);
    PRINT 'Table created';
END
