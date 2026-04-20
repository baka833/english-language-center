IF COL_LENGTH('dbo.Users', 'CanManageGradeComponents') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD CanManageGradeComponents bit NOT NULL
        CONSTRAINT DF_Users_CanManageGradeComponents DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.Classes', 'AllowTeacherGradeComponentManagement') IS NULL
BEGIN
    ALTER TABLE dbo.Classes
    ADD AllowTeacherGradeComponentManagement bit NOT NULL
        CONSTRAINT DF_Classes_AllowTeacherGradeComponentManagement DEFAULT (0);
END
GO