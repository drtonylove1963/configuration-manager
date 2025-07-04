-- Migration script to move user preferences from Configurations table to UserSettings table
-- This script should be run after the AddUserManagementTables migration has been applied

USE [ConfigurationManagerTest]
GO

-- Step 1: Create default users for existing user preferences
-- Extract unique usernames from existing UserPreferences configurations
DECLARE @UserPreferencesEnvId UNIQUEIDENTIFIER
SELECT @UserPreferencesEnvId = Id FROM Environments WHERE Name = 'UserPreferences'

IF @UserPreferencesEnvId IS NOT NULL
BEGIN
    PRINT 'Found UserPreferences environment: ' + CAST(@UserPreferencesEnvId AS VARCHAR(36))

    -- Create users for each unique username found in user preferences
    INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, IsActive, IsEmailConfirmed, CreatedAt, CreatedBy, IsDeleted)
    SELECT
        NEWID() as Id,
        SUBSTRING(c.[Key], 17, CHARINDEX(':', c.[Key], 17) - 17) as Username, -- Extract username from "UserPreferences:username:key"
        SUBSTRING(c.[Key], 17, CHARINDEX(':', c.[Key], 17) - 17) + '@configmanager.local' as Email,
        '$2a$11$placeholder.hash.for.migration.user.account' as PasswordHash, -- Placeholder hash
        'User' as FirstName,
        SUBSTRING(c.[Key], 17, CHARINDEX(':', c.[Key], 17) - 17) as LastName,
        1 as IsActive,
        0 as IsEmailConfirmed,
        GETUTCDATE() as CreatedAt,
        'migration-script' as CreatedBy,
        0 as IsDeleted
    FROM Configurations c
    WHERE c.EnvironmentId = @UserPreferencesEnvId
        AND c.[Key] LIKE 'UserPreferences:%:%'
        AND c.IsDeleted = 0
    GROUP BY SUBSTRING(c.[Key], 17, CHARINDEX(':', c.[Key], 17) - 17)

    PRINT 'Created users for existing user preferences'

    -- Step 2: Create UserSettings records for each user
    INSERT INTO UserSettings (Id, UserId, ThemeMode, PageSize, DefaultLanguage, EnableNotifications, EnableEmailNotifications, CreatedAt, CreatedBy, IsDeleted)
    SELECT
        NEWID() as Id,
        u.Id as UserId,
        'light' as ThemeMode, -- Default theme
        25 as PageSize, -- Default page size
        'en' as DefaultLanguage,
        1 as EnableNotifications,
        0 as EnableEmailNotifications,
        GETUTCDATE() as CreatedAt,
        'migration-script' as CreatedBy,
        0 as IsDeleted
    FROM Users u
    WHERE u.CreatedBy = 'migration-script'

    PRINT 'Created default UserSettings records'

    -- Step 3: Update UserSettings with actual theme preferences from Configurations
    UPDATE us
    SET ThemeMode = CASE
        WHEN c.Value = 'dark' THEN 'dark'
        ELSE 'light'
    END,
    UpdatedAt = GETUTCDATE(),
    UpdatedBy = 'migration-script'
    FROM UserSettings us
    INNER JOIN Users u ON us.UserId = u.Id
    INNER JOIN Configurations c ON c.[Key] = 'UserPreferences:' + u.Username + ':ThemeMode'
    WHERE c.EnvironmentId = @UserPreferencesEnvId
        AND c.IsDeleted = 0
        AND u.CreatedBy = 'migration-script'

    PRINT 'Updated theme preferences from existing configurations'

    -- Step 4: Clean up - Mark old user preference configurations as deleted
    UPDATE Configurations
    SET IsDeleted = 1,
        DeletedAt = GETUTCDATE(),
        DeletedBy = 'migration-script'
    WHERE EnvironmentId = @UserPreferencesEnvId
        AND [Key] LIKE 'UserPreferences:%:%'
        AND IsDeleted = 0

    PRINT 'Marked old user preference configurations as deleted'

    -- Step 5: Display migration summary
    SELECT
        'Migration Summary' as [Step],
        COUNT(*) as [Count]
    FROM Users
    WHERE CreatedBy = 'migration-script'

    UNION ALL

    SELECT
        'UserSettings Created' as [Step],
        COUNT(*) as [Count]
    FROM UserSettings us
    INNER JOIN Users u ON us.UserId = u.Id
    WHERE u.CreatedBy = 'migration-script'

    UNION ALL

    SELECT
        'Old Configs Deleted' as [Step],
        COUNT(*) as [Count]
    FROM Configurations
    WHERE EnvironmentId = @UserPreferencesEnvId
        AND [Key] LIKE 'UserPreferences:%:%'
        AND IsDeleted = 1
        AND DeletedBy = 'migration-script'

END
ELSE
BEGIN
    PRINT 'No UserPreferences environment found - no migration needed'
END

PRINT 'User preferences migration completed'