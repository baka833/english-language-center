USE [EnglishCenterDB]
GO

DECLARE @dropConstraints nvarchar(max) = N'';

SELECT @dropConstraints = STRING_AGG(
    N'ALTER TABLE dbo.Attendance DROP CONSTRAINT [' + cc.name + N']',
    N';'
)
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON t.object_id = cc.parent_object_id
WHERE t.name = 'Attendance'
  AND cc.definition LIKE '%[Status]%';

IF @dropConstraints IS NOT NULL AND LEN(@dropConstraints) > 0
BEGIN
    EXEC sp_executesql @dropConstraints;
END
GO

ALTER TABLE dbo.Attendance WITH CHECK
ADD CONSTRAINT CK_Attendance_Status
CHECK ([Status] IN ('Present', 'Absent', 'Late', 'Excused'));
GO

ALTER TABLE dbo.Attendance CHECK CONSTRAINT CK_Attendance_Status;
GO