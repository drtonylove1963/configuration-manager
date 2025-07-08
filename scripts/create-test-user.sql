-- Create test user for authentication testing
-- Password: "password123" (BCrypt hashed)

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

USE ConfigurationManager;

-- Check if test user already exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'testuser')
BEGIN
    -- Create test user
    INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, IsActive, IsEmailConfirmed, CreatedAt, CreatedBy, IsDeleted)
    VALUES (
        NEWID(),
        'testuser',
        'testuser@configmanager.local',
        '$2a$11$rQZJKAx8nZKJ8yF8yF8yFOeKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK', -- BCrypt hash for "password123"
        'Test',
        'User',
        1, -- IsActive
        1, -- IsEmailConfirmed
        GETUTCDATE(),
        'system',
        0 -- IsDeleted
    );

    PRINT 'Test user created successfully';
    PRINT 'Username: testuser';
    PRINT 'Password: password123';
END
ELSE
BEGIN
    PRINT 'Test user already exists';
END

-- Create admin user
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    -- Create admin user
    INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, IsActive, IsEmailConfirmed, CreatedAt, CreatedBy, IsDeleted)
    VALUES (
        NEWID(),
        'admin',
        'admin@configmanager.local',
        '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdQXvbVxrQ5/Zo9137axEXNdwi8qG', -- BCrypt hash for "password123"
        'Admin',
        'User',
        1, -- IsActive
        1, -- IsEmailConfirmed
        GETUTCDATE(),
        'system',
        0 -- IsDeleted
    );

    PRINT 'Admin user created successfully';
    PRINT 'Username: admin';
    PRINT 'Password: password123';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- Show created users
SELECT Username, Email, FirstName, LastName, IsActive, CreatedAt 
FROM Users 
WHERE Username IN ('testuser', 'admin');
