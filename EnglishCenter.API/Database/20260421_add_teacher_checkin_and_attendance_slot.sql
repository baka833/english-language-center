USE [EnglishCenterDB]
GO

IF COL_LENGTH('dbo.Attendance', 'ScheduleID') IS NULL
BEGIN
    ALTER TABLE dbo.Attendance ADD ScheduleID int NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Att_Schedule'
      AND parent_object_id = OBJECT_ID('dbo.Attendance')
)
BEGIN
    ALTER TABLE dbo.Attendance WITH CHECK
    ADD CONSTRAINT FK_Att_Schedule
        FOREIGN KEY (ScheduleID) REFERENCES dbo.Schedules(ScheduleID);
END
GO

IF OBJECT_ID('dbo.TeacherCheckIns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TeacherCheckIns
    (
        CheckInID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeacherCheckIn PRIMARY KEY,
        TeacherID int NOT NULL,
        ScheduleID int NOT NULL,
        AttendanceDate date NOT NULL,
        CheckedInAt datetime NOT NULL CONSTRAINT DF_TeacherCheckIns_CheckedInAt DEFAULT (GETUTCDATE())
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_TeacherCheckIns_Teacher_Schedule_Date'
      AND object_id = OBJECT_ID('dbo.TeacherCheckIns')
)
BEGIN
    CREATE UNIQUE INDEX UX_TeacherCheckIns_Teacher_Schedule_Date
        ON dbo.TeacherCheckIns (TeacherID, ScheduleID, AttendanceDate);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TeacherCheckIn_Teacher'
      AND parent_object_id = OBJECT_ID('dbo.TeacherCheckIns')
)
BEGIN
    ALTER TABLE dbo.TeacherCheckIns WITH CHECK
    ADD CONSTRAINT FK_TeacherCheckIn_Teacher
        FOREIGN KEY (TeacherID) REFERENCES dbo.Users(UserID);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TeacherCheckIn_Schedule'
      AND parent_object_id = OBJECT_ID('dbo.TeacherCheckIns')
)
BEGIN
    ALTER TABLE dbo.TeacherCheckIns WITH CHECK
    ADD CONSTRAINT FK_TeacherCheckIn_Schedule
        FOREIGN KEY (ScheduleID) REFERENCES dbo.Schedules(ScheduleID);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Attendance_ClassDateScheduleStudent'
      AND object_id = OBJECT_ID('dbo.Attendance')
)
BEGIN
    CREATE UNIQUE INDEX IX_Attendance_ClassDateScheduleStudent
        ON dbo.Attendance (ClassID, AttendanceDate, ScheduleID, StudentID)
        WHERE ScheduleID IS NOT NULL;
END
GO
