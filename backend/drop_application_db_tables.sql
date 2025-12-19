-- =============================================
-- DROP ALL TABLES FROM APPLICATION DATABASE
-- Database: mohamedsayed24_ApplicationDB
-- =============================================
-- Run this in SQL Server Management Studio
-- WARNING: This will delete ALL data!
-- =============================================

USE [mohamedsayed24_ApplicationDB];
GO

-- Drop all foreign key constraints first
DECLARE @dropConstraintsSql NVARCHAR(MAX) = N'';

SELECT @dropConstraintsSql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
               ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys;

IF LEN(@dropConstraintsSql) > 0
BEGIN
    EXEC sp_executesql @dropConstraintsSql;
    PRINT 'Foreign key constraints dropped.';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints found.';
END

-- Drop all tables
DECLARE @dropTablesSql NVARCHAR(MAX) = N''; 

SELECT @dropTablesSql += 'DROP TABLE IF EXISTS ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.tables
WHERE type = 'U' -- User tables only    
  AND name != '__EFMigrationsHistory'; -- Optionally keep migration history

IF LEN(@dropTablesSql) > 0
BEGIN
    EXEC sp_executesql @dropTablesSql;
    PRINT 'All tables dropped from mohamedsayed24_ApplicationDB!';
END
ELSE
BEGIN   
    PRINT 'No tables found to drop.';
END

PRINT 'Database is now empty and ready for migrations.';
PRINT 'Now run: dotnet ef database update --context SaasDbContext';
GO
