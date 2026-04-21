USE [EnglishCenterDB]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.Attendance', 'U') IS NULL
BEGIN
    PRINT 'Attendance table does not exist. Nothing to migrate.';
    RETURN;
END
GO

IF COL_LENGTH('dbo.Attendance', 'ClassID') IS NULL
BEGIN
    PRINT 'Attendance.ClassID has already been removed. Nothing to migrate.';
    RETURN;
END
GO

IF COL_LENGTH('dbo.Attendance', 'ScheduleID') IS NULL
BEGIN
    THROW 50010, 'Attendance.ScheduleID does not exist. Migration cannot continue.', 1;
END
GO

;WITH CandidateSchedules AS
(
    SELECT
        a.AttendanceID,
        s.ScheduleID,
        COUNT(*) OVER (PARTITION BY a.AttendanceID) AS MatchCount,
        ROW_NUMBER() OVER (PARTITION BY a.AttendanceID ORDER BY s.ScheduleID) AS MatchOrder
    FROM dbo.Attendance a
    INNER JOIN dbo.Schedules s ON s.ClassID = a.ClassID
    WHERE a.ScheduleID IS NULL
      AND (
            (s.ScheduleDate IS NOT NULL AND s.ScheduleDate = a.AttendanceDate)
         OR (
                s.ScheduleDate IS NULL
            AND s.DayOfWeek = (((DATEDIFF(DAY, '19000101', a.AttendanceDate) % 7 + 7) % 7) + 2)
         )
      )
)
UPDATE a
SET ScheduleID = c.ScheduleID
FROM dbo.Attendance a
INNER JOIN CandidateSchedules c
    ON c.AttendanceID = a.AttendanceID
WHERE a.ScheduleID IS NULL
  AND c.MatchCount = 1
  AND c.MatchOrder = 1;
GO

IF EXISTS (
    SELECT 1
    FROM dbo.Attendance
    WHERE ScheduleID IS NULL
)
BEGIN
    THROW 50011, 'Some Attendance rows still have NULL ScheduleID after deterministic backfill. Resolve them manually before dropping ClassID.', 1;
END
GO

IF EXISTS (
    SELECT 1
    FROM dbo.Attendance a
    INNER JOIN dbo.Schedules s ON s.ScheduleID = a.ScheduleID
    WHERE a.ClassID <> s.ClassID
)
BEGIN
    THROW 50012, 'Some Attendance rows have ClassID that does not match Schedules.ClassID. Migration aborted.', 1;
END
GO

IF EXISTS (
    SELECT 1
    FROM dbo.Attendance
    GROUP BY StudentID, ScheduleID, AttendanceDate
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 50013, 'Duplicate Attendance rows exist for (StudentID, ScheduleID, AttendanceDate). Migration aborted.', 1;
END
GO

DROP INDEX IF EXISTS IX_Attendance_ClassDateScheduleStudent ON dbo.Attendance;
GO

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Att_Class'
      AND parent_object_id = OBJECT_ID('dbo.Attendance')
)
BEGIN
    ALTER TABLE dbo.Attendance DROP CONSTRAINT FK_Att_Class;
END
GO

ALTER TABLE dbo.Attendance ALTER COLUMN ScheduleID int NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Attendance_Student_Schedule_Date'
      AND object_id = OBJECT_ID('dbo.Attendance')
)
BEGIN
    CREATE UNIQUE INDEX UX_Attendance_Student_Schedule_Date
        ON dbo.Attendance (StudentID, ScheduleID, AttendanceDate);
END
GO

ALTER TABLE dbo.Attendance DROP COLUMN ClassID;
GO