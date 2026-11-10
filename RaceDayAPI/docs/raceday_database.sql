-- ============================================================
-- RaceDay Database Script - Part 1 Section C
-- SQL Server 2022 / SSMS Compatible
-- NOTE: User seed data uses BCrypt-hashed passwords.
--       Hash shown = BCrypt.HashPassword("Password1!")
-- ============================================================

-- Step 1: Drop and recreate database
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'RaceDay')
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- ============================================================
-- TABLE 1: Users
-- ============================================================
CREATE TABLE Users (
    UserID    INT           IDENTITY(1,1) PRIMARY KEY,
    FullName  NVARCHAR(100) NOT NULL,
    Email     NVARCHAR(150) NOT NULL UNIQUE,
    Password  NVARCHAR(255) NOT NULL,
    Role      NVARCHAR(20)  NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- TABLE 2: Categories
-- ============================================================
CREATE TABLE Categories (
    CategoryID   INT           IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description  NVARCHAR(500) NULL
);
GO

-- ============================================================
-- TABLE 3: Events
-- EventType: 'run', 'walk', 'cycle'
-- ============================================================
CREATE TABLE Events (
    EventID         INT            IDENTITY(1,1) PRIMARY KEY,
    EventName       NVARCHAR(200)  NOT NULL,
    Description     NVARCHAR(1000) NULL,
    EventDate       DATETIME       NOT NULL,
    Location        NVARCHAR(300)  NOT NULL,
    Distance        DECIMAL(6,2)   NOT NULL,
    MaxParticipants INT            NOT NULL DEFAULT 100,
    EventType       NVARCHAR(20)   NOT NULL DEFAULT 'run'
                        CHECK (EventType IN ('run', 'walk', 'cycle')),
    BannerImageUrl  NVARCHAR(500)  NULL,
    CategoryID      INT            NOT NULL,
    OrganiserID     INT            NOT NULL,
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Category  FOREIGN KEY (CategoryID)  REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO

-- ============================================================
-- TABLE 4: Enrolments
-- ============================================================
CREATE TABLE Enrolments (
    EnrolmentID   INT          IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT          NOT NULL,
    EventID       INT          NOT NULL,
    EnrolledAt    DATETIME     NOT NULL DEFAULT GETDATE(),
    Status        NVARCHAR(20) NOT NULL DEFAULT 'Registered'
                      CHECK (Status IN ('Registered', 'Cancelled')),

    CONSTRAINT UQ_Enrolment              UNIQUE (ParticipantID, EventID),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Event       FOREIGN KEY (EventID)       REFERENCES Events(EventID)
);
GO

-- ============================================================
-- TABLE 5: Results
-- ============================================================
CREATE TABLE Results (
    ResultID    INT           IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT           NOT NULL UNIQUE,
    FinishTime  NVARCHAR(20)  NOT NULL,
    Position    INT           NULL,
    Notes       NVARCHAR(500) NULL,

    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO

-- ============================================================
-- TABLE 6: Routes
-- ============================================================
CREATE TABLE Routes (
    RouteID       INT            IDENTITY(1,1) PRIMARY KEY,
    EventID       INT            NOT NULL UNIQUE,
    RouteMapURL   NVARCHAR(500)  NULL,
    StartPoint    NVARCHAR(200)  NOT NULL,
    EndPoint      NVARCHAR(200)  NOT NULL,
    ElevationGain DECIMAL(6,2)   NULL,
    Description   NVARCHAR(1000) NULL,

    CONSTRAINT FK_Routes_Event FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

-- ============================================================
-- SEED: 4 Categories
-- ============================================================
INSERT INTO Categories (CategoryName, Description) VALUES
('Road Running',  'Running events on tarred roads, ranging from 5km to full marathon'),
('Cycling',       'Road and trail cycling events for all fitness levels'),
('Walking',       'Casual and competitive walking events, great for all ages'),
('Trail Running', 'Off-road running on natural terrain and mountain paths');
GO

-- ============================================================
-- SEED: Users (BCrypt hashed "Password1!" — work factor 11)
-- To log in with these accounts use password: Password1!
-- ============================================================
INSERT INTO Users (FullName, Email, Password, Role) VALUES
('Sarah Dlamini',  'sarah@raceday.co.za',
 '$2a$11$KyR2.LqQRJeVHfzBc9RNlOxUBhvKoiHAuX3s7q8uT5YWmvZtP.5m6', 'Organiser'),
('James Nkosi',    'james@raceday.co.za',
 '$2a$11$KyR2.LqQRJeVHfzBc9RNlOxUBhvKoiHAuX3s7q8uT5YWmvZtP.5m6', 'Organiser'),
('Lerato Mokoena', 'lerato@gmail.com',
 '$2a$11$KyR2.LqQRJeVHfzBc9RNlOxUBhvKoiHAuX3s7q8uT5YWmvZtP.5m6', 'Participant'),
('Thabo Sithole',  'thabo@gmail.com',
 '$2a$11$KyR2.LqQRJeVHfzBc9RNlOxUBhvKoiHAuX3s7q8uT5YWmvZtP.5m6', 'Participant');
GO

-- ============================================================
-- SEED: 3 Events (UserID 1=Sarah, 2=James; CategoryID 1=Road Running, 2=Cycling, 3=Walking)
-- ============================================================
INSERT INTO Events (EventName, Description, EventDate, Location, Distance, MaxParticipants, EventType, CategoryID, OrganiserID) VALUES
('Cape Town Half Marathon',
 'A scenic 21km run through the heart of Cape Town ending at the Waterfront.',
 '2026-11-15 07:00:00', 'Cape Town CBD, Western Cape', 21.10, 500, 'run', 1, 1),

('Soweto Cycle Challenge',
 'A 94km cycling event through Soweto and surrounds.',
 '2026-10-25 06:30:00', 'Soweto, Johannesburg, Gauteng', 94.00, 1000, 'cycle', 2, 2),

('Durban Fun Walk',
 'A 10km community walk along the Durban beachfront. Suitable for all ages.',
 '2026-09-20 08:00:00', 'Durban Beachfront, KwaZulu-Natal', 10.00, 300, 'walk', 3, 1);
GO

-- ============================================================
-- SEED: Enrolments (UserID 3=Lerato, 4=Thabo)
-- ============================================================
INSERT INTO Enrolments (ParticipantID, EventID, Status) VALUES
(3, 1, 'Registered'),
(4, 1, 'Registered'),
(3, 2, 'Registered');
GO

-- ============================================================
-- SEED: Results
-- ============================================================
INSERT INTO Results (EnrolmentID, FinishTime, Position, Notes) VALUES
(1, '01:52:34', 1, 'Excellent performance, strong finish'),
(2, '02:05:12', 2, 'Consistent pace throughout');
GO

-- ============================================================
-- SEED: Routes
-- ============================================================
INSERT INTO Routes (EventID, RouteMapURL, StartPoint, EndPoint, ElevationGain, Description) VALUES
(1, 'https://raceday.co.za/routes/ctmarathon-map.png',
 'Grand Parade, Cape Town', 'V&A Waterfront, Cape Town', 145.50,
 'Start at Grand Parade, through the CBD, along Sea Point promenade, finishing at the V&A Waterfront.'),
(2, 'https://raceday.co.za/routes/sowetocycle-map.png',
 'FNB Stadium, Soweto', 'FNB Stadium, Soweto', 320.00,
 'Circular route starting and ending at FNB Stadium, passing through Diepkloof and Orlando.');
GO

-- ============================================================
-- VERIFY row counts
-- Expected: Users=4, Categories=4, Events=3, Enrolments=3, Results=2, Routes=2
-- ============================================================
SELECT 'Users'       AS TableName, COUNT(*) AS Rows FROM Users
UNION ALL SELECT 'Categories',     COUNT(*) FROM Categories
UNION ALL SELECT 'Events',         COUNT(*) FROM Events
UNION ALL SELECT 'Enrolments',     COUNT(*) FROM Enrolments
UNION ALL SELECT 'Results',        COUNT(*) FROM Results
UNION ALL SELECT 'Routes',         COUNT(*) FROM Routes;
GO
-- ============================================================
-- END OF SCRIPT  (login with any seeded user: Password1!)
-- ============================================================
