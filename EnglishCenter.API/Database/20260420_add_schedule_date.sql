IF COL_LENGTH('dbo.Schedules', 'ScheduleDate') IS NULL
BEGIN
    ALTER TABLE dbo.Schedules
    ADD ScheduleDate date NULL;
END
GO