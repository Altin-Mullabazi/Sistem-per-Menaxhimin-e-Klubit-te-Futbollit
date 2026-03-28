-- Seed Users (passwords hashed with SHA256 in Base64)
INSERT INTO [Users] (Id, Username, Email, PasswordHash, Role, CreatedAt, IsActive)
VALUES 
    (NEWID(), 'admin', 'admin@footballclub.com', 'Q2fpF0UjGcCfMqfGJLPQgKGNFV/wWPGGZS1c3R10Q5A=', 'Admin', GETUTCDATE(), 1),
    (NEWID(), 'coach', 'coach@footballclub.com', '3nPjKAkN5V0M+MhY8TrEQjRlMnPJF7QK8W9xS3B2Q4Q=', 'Coach', GETUTCDATE(), 1),
    (NEWID(), 'manager', 'manager@footballclub.com', 'bVqGHPqmvpLkQ8tRjN2xQ3r9sT0K1pY5L8mQ2F6N9W5=', 'Manager', GETUTCDATE(), 1);

-- Seed Players
INSERT INTO [Players] (FirstName, LastName, Age, Position, ClubName, CreatedAt, UpdatedAt)
VALUES 
    ('Cristiano', 'Ronaldo', 38, 'Forward', 'Al Nassr', GETUTCDATE(), GETUTCDATE()),
    ('Lionel', 'Messi', 36, 'Forward', 'Inter Miami', GETUTCDATE(), GETUTCDATE()),
    ('Kylian', 'Mbappé', 25, 'Forward', 'Real Madrid', GETUTCDATE(), GETUTCDATE()),
    ('Vinicius', 'Junior', 24, 'Left Winger', 'Real Madrid', GETUTCDATE(), GETUTCDATE()),
    ('Jude', 'Bellingham', 21, 'Midfielder', 'Real Madrid', GETUTCDATE(), GETUTCDATE());

-- Verify data
SELECT 'Users:' as [Table];
SELECT * FROM [Users];
SELECT 'Players:' as [Table];
SELECT * FROM [Players];
