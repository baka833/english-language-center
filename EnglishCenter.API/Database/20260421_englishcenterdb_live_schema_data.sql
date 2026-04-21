/* Live export of selected EnglishCenterDB application tables */
/* Source: LAPTOP-O0IHPN5P\MSSQLSERVER01 / EnglishCenterDB */
/* Generated: 2026-04-21 21:18:05 */

USE [EnglishCenterDB]
GO

/****** Schema ******/
/****** Object:  Table [dbo].[Courses]    Script Date: 21/04/2026 21:18:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Courses](
	[CourseID] [int] IDENTITY(1,1) NOT NULL,
	[CourseName] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Description] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Price] [decimal](18, 2) NULL,
	[TotalSlots] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[CourseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

/****** Object:  Table [dbo].[Users]    Script Date: 21/04/2026 21:18:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PasswordHash] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Fullname] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Dob] [date] NULL,
	[Gender] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Email] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Role] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[IsActive] [bit] NULL,
	[CreatedAt] [datetime] NULL,
	[CanManageGradeComponents] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_CanManageGradeComponents]  DEFAULT ((0)) FOR [CanManageGradeComponents]
GO

ALTER TABLE [dbo].[Users]  WITH CHECK ADD CHECK  (([Role]='Student' OR [Role]='Teacher' OR [Role]='Admin'))
GO

/****** Object:  Table [dbo].[Classes]    Script Date: 21/04/2026 21:18:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Classes](
	[ClassID] [int] IDENTITY(1,1) NOT NULL,
	[ClassName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CourseID] [int] NOT NULL,
	[TeacherID] [int] NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[Status] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AllowTeacherGradeComponentManagement] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ClassID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Classes] ADD  DEFAULT ('Opening') FOR [Status]
GO

ALTER TABLE [dbo].[Classes] ADD  CONSTRAINT [DF_Classes_AllowTeacherGradeComponentManagement]  DEFAULT ((0)) FOR [AllowTeacherGradeComponentManagement]
GO

ALTER TABLE [dbo].[Classes]  WITH CHECK ADD  CONSTRAINT [FK_Class_Course] FOREIGN KEY([CourseID])
REFERENCES [dbo].[Courses] ([CourseID])
GO

ALTER TABLE [dbo].[Classes] CHECK CONSTRAINT [FK_Class_Course]
GO

ALTER TABLE [dbo].[Classes]  WITH CHECK ADD  CONSTRAINT [FK_Class_Teacher] FOREIGN KEY([TeacherID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[Classes] CHECK CONSTRAINT [FK_Class_Teacher]
GO

/****** Object:  Table [dbo].[Applications]    Script Date: 21/04/2026 21:18:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Applications](
	[AppID] [int] IDENTITY(1,1) NOT NULL,
	[SenderID] [int] NOT NULL,
	[Title] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Content] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Type] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Status] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[AdminResponse] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[AppID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Applications] ADD  DEFAULT ('Pending') FOR [Status]
GO

ALTER TABLE [dbo].[Applications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK_App_Sender] FOREIGN KEY([SenderID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK_App_Sender]
GO

ALTER TABLE [dbo].[Applications]  WITH CHECK ADD CHECK  (([Status]='Rejected' OR [Status]='Approved' OR [Status]='Pending'))
GO

/****** Object:  Table [dbo].[Class_Students]    Script Date: 21/04/2026 21:18:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Class_Students](
	[ClassID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[EnrollmentDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ClassID] ASC,
	[StudentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Class_Students] ADD  DEFAULT (getdate()) FOR [EnrollmentDate]
GO

ALTER TABLE [dbo].[Class_Students]  WITH CHECK ADD  CONSTRAINT [FK_CS_Class] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Class_Students] CHECK CONSTRAINT [FK_CS_Class]
GO

ALTER TABLE [dbo].[Class_Students]  WITH CHECK ADD  CONSTRAINT [FK_CS_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[Class_Students] CHECK CONSTRAINT [FK_CS_Student]
GO

/****** Object:  Table [dbo].[GradeComponents]    Script Date: 21/04/2026 21:18:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GradeComponents](
	[ComponentID] [int] IDENTITY(1,1) NOT NULL,
	[ClassID] [int] NOT NULL,
	[ComponentName] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Weight] [decimal](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ComponentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[GradeComponents]  WITH CHECK ADD  CONSTRAINT [FK_GradeComp_Class] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[GradeComponents] CHECK CONSTRAINT [FK_GradeComp_Class]
GO

/****** Object:  Table [dbo].[Schedules]    Script Date: 21/04/2026 21:18:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Schedules](
	[ScheduleID] [int] IDENTITY(1,1) NOT NULL,
	[ClassID] [int] NOT NULL,
	[DayOfWeek] [int] NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[Room] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ScheduleDate] [date] NULL,
PRIMARY KEY CLUSTERED 
(
	[ScheduleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Schedules]  WITH CHECK ADD  CONSTRAINT [FK_Schedule_Class] FOREIGN KEY([ClassID])
REFERENCES [dbo].[Classes] ([ClassID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Schedules] CHECK CONSTRAINT [FK_Schedule_Class]
GO

ALTER TABLE [dbo].[Schedules]  WITH CHECK ADD CHECK  (([DayOfWeek]>=(2) AND [DayOfWeek]<=(8)))
GO

/****** Object:  Table [dbo].[Grades]    Script Date: 21/04/2026 21:18:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Grades](
	[GradeID] [int] IDENTITY(1,1) NOT NULL,
	[ComponentID] [int] NOT NULL,
	[StudentID] [int] NOT NULL,
	[GradeValue] [decimal](5, 2) NULL,
	[TeacherComment] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[GradeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

ALTER TABLE [dbo].[Grades] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO

ALTER TABLE [dbo].[Grades]  WITH CHECK ADD  CONSTRAINT [FK_Grades_Component] FOREIGN KEY([ComponentID])
REFERENCES [dbo].[GradeComponents] ([ComponentID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Grades] CHECK CONSTRAINT [FK_Grades_Component]
GO

ALTER TABLE [dbo].[Grades]  WITH CHECK ADD  CONSTRAINT [FK_Grades_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[Grades] CHECK CONSTRAINT [FK_Grades_Student]
GO

ALTER TABLE [dbo].[Grades]  WITH CHECK ADD CHECK  (([GradeValue]>=(0) AND [GradeValue]<=(10)))
GO

/****** Object:  Table [dbo].[Attendance]    Script Date: 21/04/2026 21:18:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Attendance](
	[AttendanceID] [int] IDENTITY(1,1) NOT NULL,
	[StudentID] [int] NOT NULL,
	[AttendanceDate] [date] NOT NULL,
	[Status] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Note] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ScheduleID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AttendanceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

/****** Object:  Index [UX_Attendance_Student_Schedule_Date]    Script Date: 21/04/2026 21:18:07 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Attendance_Student_Schedule_Date] ON [dbo].[Attendance]
(
	[StudentID] ASC,
	[ScheduleID] ASC,
	[AttendanceDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO

ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_Att_Schedule] FOREIGN KEY([ScheduleID])
REFERENCES [dbo].[Schedules] ([ScheduleID])
GO

ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_Att_Schedule]
GO

ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [FK_Att_Student] FOREIGN KEY([StudentID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [FK_Att_Student]
GO

ALTER TABLE [dbo].[Attendance]  WITH CHECK ADD  CONSTRAINT [CK_Attendance_Status] CHECK  (([Status]='Excused' OR [Status]='Late' OR [Status]='Absent' OR [Status]='Present'))
GO

ALTER TABLE [dbo].[Attendance] CHECK CONSTRAINT [CK_Attendance_Status]
GO

/****** Object:  Table [dbo].[TeacherCheckIns]    Script Date: 21/04/2026 21:18:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TeacherCheckIns](
	[CheckInID] [int] IDENTITY(1,1) NOT NULL,
	[TeacherID] [int] NOT NULL,
	[ScheduleID] [int] NOT NULL,
	[AttendanceDate] [date] NOT NULL,
	[CheckedInAt] [datetime] NOT NULL,
 CONSTRAINT [PK_TeacherCheckIn] PRIMARY KEY CLUSTERED 
(
	[CheckInID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
GO

/****** Object:  Index [UX_TeacherCheckIns_Teacher_Schedule_Date]    Script Date: 21/04/2026 21:18:07 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_TeacherCheckIns_Teacher_Schedule_Date] ON [dbo].[TeacherCheckIns]
(
	[TeacherID] ASC,
	[ScheduleID] ASC,
	[AttendanceDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
GO

ALTER TABLE [dbo].[TeacherCheckIns] ADD  CONSTRAINT [DF_TeacherCheckIns_CheckedInAt]  DEFAULT (getutcdate()) FOR [CheckedInAt]
GO

ALTER TABLE [dbo].[TeacherCheckIns]  WITH CHECK ADD  CONSTRAINT [FK_TeacherCheckIn_Schedule] FOREIGN KEY([ScheduleID])
REFERENCES [dbo].[Schedules] ([ScheduleID])
GO

ALTER TABLE [dbo].[TeacherCheckIns] CHECK CONSTRAINT [FK_TeacherCheckIn_Schedule]
GO

ALTER TABLE [dbo].[TeacherCheckIns]  WITH CHECK ADD  CONSTRAINT [FK_TeacherCheckIn_Teacher] FOREIGN KEY([TeacherID])
REFERENCES [dbo].[Users] ([UserID])
GO

ALTER TABLE [dbo].[TeacherCheckIns] CHECK CONSTRAINT [FK_TeacherCheckIn_Teacher]
GO

/****** Data ******/
SET IDENTITY_INSERT [dbo].[Courses] ON
GO

INSERT INTO [dbo].[Courses] ([CourseID], [CourseName], [Description], [Price], [TotalSlots]) VALUES (1, N'IELTS Breakthrough 5.0-6.5', N'Khóa học cam kết đầu ra 6.5', 8500000.00, 48);
GO

INSERT INTO [dbo].[Courses] ([CourseID], [CourseName], [Description], [Price], [TotalSlots]) VALUES (2, N'TOEIC 500+', N'Luyện thi TOEIC cấp tốc', 3500000.00, 24);
GO

INSERT INTO [dbo].[Courses] ([CourseID], [CourseName], [Description], [Price], [TotalSlots]) VALUES (3, N'English for Beginners', N'Tiếng Anh cho người mất gốc', 2000000.00, 12);
GO

INSERT INTO [dbo].[Courses] ([CourseID], [CourseName], [Description], [Price], [TotalSlots]) VALUES (4, N'English For Business', N'Tiếng anh cho người ko rảnh', 4000000.00, 24);
GO

SET IDENTITY_INSERT [dbo].[Courses] OFF
GO

SET IDENTITY_INSERT [dbo].[Users] ON
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1, N'admin01', N'HASHED_PW_HERE', N'Nguyễn Quản Trị', '1985-05-20', N'Nam', N'admin@center.com', N'Admin', 1, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (2, N'teacher.an', N'HASHED_PW_HERE', N'Lê Văn An', '1990-01-15', N'Nam', N'an.le@center.com', N'Teacher', 1, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (3, N'teacher.hoa', N'HASHED_PW_HERE', N'Trần Thị Hoa', '1993-03-10', N'Nữ', N'hoa.tran@center.com', N'Teacher', 1, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (4, N'student.dung', N'HASHED_PW_HERE', N'Phạm Tuấn Dũng', '2005-07-22', N'Nam', N'dung.pham@gmail.com', N'Student', 1, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (5, N'student.linh', N'AQAAAAIAAYagAAAAEAIb7DJKIA0fgtwJ6N5B0vNXqUxAFc6NRgcU4hiIgjSObVEh9BPfbYp3q8fr5T3tSA==', N'Hoàng Thùy Linh', '2006-11-02', N'Nữ', N'linh.hoang@gmail.com', N'Student', 0, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (6, N'student.bach', N'HASHED_PW_HERE', N'Ngô Xuân Bách', '2005-12-30', N'Nam', N'bach.ngo@gmail.com', N'Student', 1, '2026-04-20T10:10:19.530', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (7, N'Student.Nam12', N'AQAAAAIAAYagAAAAEIXkTHc7J6NAnlPdbx3mzq045PdAkrqNjFFeITaxUGDUoz8fuyUqRZzo+6V+DlXMmw==', N'Hoang Van Nam', '2004-04-20', N'Male', N'Nam@gmail.com', N'Student', 1, '2026-04-20T03:16:00.150', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1007, N'admin', N'AQAAAAIAAYagAAAAEHLKejZm5S/GAk0GgJAYGlrujVzSeQijcKw0ReBsixgzOkAtQXDa38BfdDUeoZXp6g==', N'System Admin', '1985-01-01', N'Male', N'admin@englishcenter.com', N'Admin', 1, '2026-04-20T11:19:12.673', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1008, N'teacher1', N'AQAAAAIAAYagAAAAEEYQPFFXslZuQzv6tB2SR7huHinfx4Nozj+pJTrDcuOX9hewKrSdyWo8Jz6c0hKX+A==', N'Nguyen Van A', '1990-05-15', N'Male', N'teacher1@englishcenter.com', N'Teacher', 1, '2026-04-20T11:19:12.730', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1009, N'teacher2', N'AQAAAAIAAYagAAAAEIUbWCVcOBw5jPzSTZr+Wjc2v1BkIhfzFffLYXFHVZEgWfCgfMn0k6CK9gubE3ggIA==', N'Tran Thi B', '1992-08-20', N'Female', N'teacher2@englishcenter.com', N'Teacher', 1, '2026-04-20T11:19:12.787', 1);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1010, N'student1', N'AQAAAAIAAYagAAAAEE0nBbNF7dbH3p32akzRVNPvWO1ZZfbTxF7nOJArTWlGnvfsHESTpRKm+kOn+W67dg==', N'Le Van C', '2002-03-10', N'Male', N'student1@example.com', N'Student', 1, '2026-04-20T11:19:12.843', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1011, N'student2', N'AQAAAAIAAYagAAAAEB4dnZaMf/kKSjh+OQvtO7wyHoN0HiUYsHkaDfF42JGGu/hjgWWiiHvySZ7MsJFAZg==', N'Pham Thi D', '2003-07-25', N'Female', N'student2@example.com', N'Student', 1, '2026-04-20T11:19:12.903', 0);
GO

INSERT INTO [dbo].[Users] ([UserID], [Username], [PasswordHash], [Fullname], [Dob], [Gender], [Email], [Role], [IsActive], [CreatedAt], [CanManageGradeComponents]) VALUES (1012, N'student3', N'AQAAAAIAAYagAAAAEGBwdqLgrS9YV3rl0RkuR7FwSu0hG5AVtsE0emQr8B3XoeOG0RWjSLvqcoVmpEx0yg==', N'Hoang Van E', '2001-11-05', N'Male', N'student3@example.com', N'Student', 1, '2026-04-20T11:19:12.957', 0);
GO

SET IDENTITY_INSERT [dbo].[Users] OFF
GO

SET IDENTITY_INSERT [dbo].[Classes] ON
GO

INSERT INTO [dbo].[Classes] ([ClassID], [ClassName], [CourseID], [TeacherID], [StartDate], [EndDate], [Status], [AllowTeacherGradeComponentManagement]) VALUES (1, N'Lớp IELTS - Sáng T2-4-6', 1, 2, '2024-05-01', '2024-08-30', N'Ongoing', 0);
GO

INSERT INTO [dbo].[Classes] ([ClassID], [ClassName], [CourseID], [TeacherID], [StartDate], [EndDate], [Status], [AllowTeacherGradeComponentManagement]) VALUES (2, N'Lớp TOEIC - Tối T3-5', 2, 3, '2024-06-01', '2024-09-01', N'Opening', 0);
GO

INSERT INTO [dbo].[Classes] ([ClassID], [ClassName], [CourseID], [TeacherID], [StartDate], [EndDate], [Status], [AllowTeacherGradeComponentManagement]) VALUES (3, N'Lop Tiếng Anh Bussiness', 4, 3, '2026-04-20', '2026-06-20', N'Opening', 0);
GO

INSERT INTO [dbo].[Classes] ([ClassID], [ClassName], [CourseID], [TeacherID], [StartDate], [EndDate], [Status], [AllowTeacherGradeComponentManagement]) VALUES (1003, N'Lớp TO2102', 2, 1009, '2026-04-20', '2026-07-20', N'Opening', 0);
GO

SET IDENTITY_INSERT [dbo].[Classes] OFF
GO

SET IDENTITY_INSERT [dbo].[Applications] ON
GO

INSERT INTO [dbo].[Applications] ([AppID], [SenderID], [Title], [Content], [Type], [Status], [AdminResponse], [CreatedAt]) VALUES (1, 4, N'Xin nghỉ học buổi 10/05', N'Em bận việc gia đình nên xin nghỉ buổi sáng thứ 6', N'Nghỉ học', N'Approved', N'Đã ghi nhận, em xem lại bài giảng nhé.', '2026-04-20T10:10:19.653');
GO

INSERT INTO [dbo].[Applications] ([AppID], [SenderID], [Title], [Content], [Type], [Status], [AdminResponse], [CreatedAt]) VALUES (2, 5, N'Khiếu nại điểm giữa kỳ', N'Em thấy điểm hơi thấp so với bài làm', N'Khiếu nại', N'Pending', NULL, '2026-04-20T10:10:19.653');
GO

SET IDENTITY_INSERT [dbo].[Applications] OFF
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (1, 4, '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (1, 5, '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (2, 5, '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (2, 6, '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (3, 4, '2026-04-20T04:53:31.697');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (3, 6, '2026-04-20T10:02:27.683');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (3, 7, '2026-04-20T10:02:27.683');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (1003, 1010, '2026-04-20T11:46:02.320');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (1003, 1011, '2026-04-20T11:46:02.320');
GO

INSERT INTO [dbo].[Class_Students] ([ClassID], [StudentID], [EnrollmentDate]) VALUES (1003, 1012, '2026-04-20T11:46:02.320');
GO

SET IDENTITY_INSERT [dbo].[GradeComponents] ON
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (1, 1, N'Chuyên cần', 0.10);
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (2, 1, N'Giữa kỳ', 0.30);
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (3, 1, N'Cuối kỳ', 0.60);
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (4, 2, N'Test Online', 1.00);
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (1002, 1003, N'Giữa Kì', 40.00);
GO

INSERT INTO [dbo].[GradeComponents] ([ComponentID], [ClassID], [ComponentName], [Weight]) VALUES (1003, 1003, N'Cuối Kì', 60.00);
GO

SET IDENTITY_INSERT [dbo].[GradeComponents] OFF
GO

SET IDENTITY_INSERT [dbo].[Schedules] ON
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (4, 2, 3, '18:30:00', '20:30:00', N'Room 202', NULL);
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (5, 2, 5, '18:30:00', '20:30:00', N'Room 202', NULL);
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (12, 1, 2, '08:00:00', '10:00:00', N'Room 101', NULL);
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (13, 1, 4, '08:00:00', '10:00:00', N'Room 101', NULL);
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (14, 1, 6, '08:00:00', '10:00:00', N'Room 101', NULL);
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (17, 3, 2, '07:30:00', '09:30:00', N'Room 101', '2026-04-20');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (18, 3, 4, '07:30:00', '09:30:00', N'Room 101', '2026-04-22');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (19, 3, 6, '07:30:00', '09:30:00', N'Room 101', '2026-04-24');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (20, 3, 2, '10:00:00', '12:00:00', N'Room 101', '2026-04-20');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (21, 3, 4, '10:00:00', '12:00:00', N'Room 101', '2026-04-22');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (22, 3, 6, '10:00:00', '12:00:00', N'Room 101', '2026-04-24');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (55, 1003, 2, '07:30:00', '09:30:00', N'T2.101', '2026-04-27');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (56, 1003, 4, '07:30:00', '09:30:00', N'T2.101', '2026-04-29');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (57, 1003, 6, '07:30:00', '09:30:00', N'T2.101', '2026-05-01');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (58, 1003, 8, '07:30:00', '09:30:00', N'T2.101', '2026-05-03');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (59, 1003, 2, '10:00:00', '12:00:00', N'T2.101', '2026-04-27');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (60, 1003, 4, '10:00:00', '12:00:00', N'T2.101', '2026-04-29');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (61, 1003, 6, '10:00:00', '12:00:00', N'T2.101', '2026-05-01');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (62, 1003, 8, '10:00:00', '12:00:00', N'T2.101', '2026-05-03');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (63, 1003, 2, '07:30:00', '09:30:00', N'T2.101', '2026-04-20');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (64, 1003, 4, '07:30:00', '09:30:00', N'T2.101', '2026-04-22');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (65, 1003, 6, '07:30:00', '09:30:00', N'T2.101', '2026-04-24');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (66, 1003, 8, '07:30:00', '09:30:00', N'T2.101', '2026-04-26');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (67, 1003, 2, '10:00:00', '12:00:00', N'T2.101', '2026-04-20');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (68, 1003, 3, '10:00:00', '12:00:00', N'T2.101', '2026-04-21');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (69, 1003, 4, '10:00:00', '12:00:00', N'T2.101', '2026-04-22');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (70, 1003, 6, '10:00:00', '12:00:00', N'T2.101', '2026-04-24');
GO

INSERT INTO [dbo].[Schedules] ([ScheduleID], [ClassID], [DayOfWeek], [StartTime], [EndTime], [Room], [ScheduleDate]) VALUES (71, 1003, 8, '10:00:00', '12:00:00', N'T2.101', '2026-04-26');
GO

SET IDENTITY_INSERT [dbo].[Schedules] OFF
GO

SET IDENTITY_INSERT [dbo].[Grades] ON
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (1, 1, 4, 10.00, N'Đi học đầy đủ', '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (2, 2, 4, 7.50, N'Làm bài tốt nhưng cần chú ý Speaking', '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (3, 3, 4, 8.00, N'Tiến bộ rõ rệt', '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (4, 1, 5, 5.00, N'Nghỉ học nhiều', '2026-04-20T10:10:19.650');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (1002, 1002, 1012, 4.00, N'Cần Cải Thiện', '2026-04-21T03:40:33.380');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (1003, 1002, 1010, 8.00, N'Tốt', '2026-04-21T03:40:33.457');
GO

INSERT INTO [dbo].[Grades] ([GradeID], [ComponentID], [StudentID], [GradeValue], [TeacherComment], [UpdatedAt]) VALUES (1004, 1002, 1011, 9.00, N'Xuất sắc', '2026-04-21T03:40:33.480');
GO

SET IDENTITY_INSERT [dbo].[Grades] OFF
GO

SET IDENTITY_INSERT [dbo].[Attendance] ON
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (1, 4, '2024-05-06', N'Present', NULL, 12);
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (2, 5, '2024-05-06', N'Absent', N'Có phép', 12);
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (3, 6, '2024-06-04', N'Present', NULL, 4);
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (1002, 1012, '2026-04-21', N'Absent', NULL, 68);
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (1003, 1010, '2026-04-21', N'Present', NULL, 68);
GO

INSERT INTO [dbo].[Attendance] ([AttendanceID], [StudentID], [AttendanceDate], [Status], [Note], [ScheduleID]) VALUES (1004, 1011, '2026-04-21', N'Present', NULL, 68);
GO

SET IDENTITY_INSERT [dbo].[Attendance] OFF
GO

SET IDENTITY_INSERT [dbo].[TeacherCheckIns] ON
GO

INSERT INTO [dbo].[TeacherCheckIns] ([CheckInID], [TeacherID], [ScheduleID], [AttendanceDate], [CheckedInAt]) VALUES (1, 1009, 68, '2026-04-21', '2026-04-21T03:35:38.593');
GO

SET IDENTITY_INSERT [dbo].[TeacherCheckIns] OFF
GO

