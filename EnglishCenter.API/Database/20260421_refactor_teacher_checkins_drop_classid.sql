USE [EnglishCenterDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.TeacherCheckIns', 'U') IS NULL
BEGIN
    PRINT 'TeacherCheckIns table does not exist. Nothing to migrate.';
    RETURN;
END
GO

IF COL_LENGTH('dbo.TeacherCheckIns', 'ClassID') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.TeacherCheckIns tc
        INNER JOIN dbo.Schedules s ON s.ScheduleID = tc.ScheduleID
        WHERE tc.ClassID <> s.ClassID
    )
    BEGIN
        THROW 50001, 'TeacherCheckIns contains ClassID values that do not match Schedules.ClassID. Migration aborted.', 1;
    END
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_TeacherCheckIns_Teacher_Class_Schedule_Date'
      AND object_id = OBJECT_ID('dbo.TeacherCheckIns')
)
BEGIN
    DROP INDEX UX_TeacherCheckIns_Teacher_Class_Schedule_Date ON dbo.TeacherCheckIns;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TeacherCheckIn_Class'
      AND parent_object_id = OBJECT_ID('dbo.TeacherCheckIns')
)
BEGIN
    ALTER TABLE dbo.TeacherCheckIns DROP CONSTRAINT FK_TeacherCheckIn_Class;
END
GO

IF COL_LENGTH('dbo.TeacherCheckIns', 'ClassID') IS NOT NULL
BEGIN
    ALTER TABLE dbo.TeacherCheckIns DROP COLUMN ClassID;
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