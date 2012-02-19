USE [UsageDataAnalysis]
GO
/****** Object:  Table [dbo].[TaggedCommits]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaggedCommits](
	[TagId] [int] IDENTITY(1,1) NOT NULL,
	[CommitId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[IsRelease] [bit] NOT NULL,
 CONSTRAINT [PK_TaggedVersions] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sessions]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sessions](
	[SessionId] [int] IDENTITY(1,1) NOT NULL,
	[ClientSessionId] [bigint] NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[UserId] [int] NOT NULL,
	[AppVersionMajor] [int] NULL,
	[AppVersionMinor] [int] NULL,
	[AppVersionBuild] [int] NULL,
	[AppVersionRevision] [int] NULL,
	[CommitId] [int] NULL,
	[IsDebug] [bit] NOT NULL,
	[FirstException] [datetime] NULL,
	[LastFeatureUse] [datetime] NULL,
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[SessionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [idxOptimizerRecommended_Sessions] ON [dbo].[Sessions] 
(
	[UserId] ASC,
	[ClientSessionId] ASC
)
INCLUDE ( [StartTime]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeatureUse]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeatureUse](
	[FeatureUseId] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [int] NOT NULL,
	[UseTime] [datetime] NOT NULL,
	[EndTime] [datetime] NULL,
	[FeatureId] [int] NOT NULL,
	[ActivationMethodId] [int] NOT NULL,
 CONSTRAINT [PK_FeatureUse] PRIMARY KEY CLUSTERED 
(
	[FeatureUseId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_FeatureUse] ON [dbo].[FeatureUse] 
(
	[SessionId] ASC,
	[ActivationMethodId] ASC,
	[FeatureId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Features]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Features](
	[FeatureId] [int] IDENTITY(1,1) NOT NULL,
	[FeatureName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Features] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Features] ON [dbo].[Features] 
(
	[FeatureName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Exceptions]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exceptions](
	[ExceptionId] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [int] NOT NULL,
	[ExceptionGroupId] [int] NOT NULL,
	[ThrownAt] [datetime] NOT NULL,
	[Stacktrace] [nvarchar](max) NOT NULL,
	[IsFirstInSession] [bit] NOT NULL,
 CONSTRAINT [PK_Exceptions] PRIMARY KEY CLUSTERED 
(
	[ExceptionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_Exceptions] ON [dbo].[Exceptions] 
(
	[ExceptionGroupId] ASC,
	[IsFirstInSession] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionGroups]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionGroups](
	[ExceptionGroupId] [int] IDENTITY(1,1) NOT NULL,
	[TypeFingerprintSha256Hash] [nvarchar](128) NOT NULL,
	[ExceptionType] [nvarchar](255) NOT NULL,
	[ExceptionFingerprint] [nvarchar](max) NOT NULL,
	[ExceptionLocation] [nvarchar](max) NOT NULL,
	[UserComment] [nvarchar](max) NULL,
	[UserFixedInCommit] [int] NULL,
 CONSTRAINT [PK_ExceptionTypes] PRIMARY KEY CLUSTERED 
(
	[ExceptionGroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ExceptionTypes] ON [dbo].[ExceptionGroups] 
(
	[TypeFingerprintSha256Hash] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EnvironmentDataValues]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EnvironmentDataValues](
	[EnvironmentDataValueId] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentDataValue] [nvarchar](255) NULL,
 CONSTRAINT [PK_EnvironmentDataValues] PRIMARY KEY CLUSTERED 
(
	[EnvironmentDataValueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EnvironmentDataValues] ON [dbo].[EnvironmentDataValues] 
(
	[EnvironmentDataValue] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EnvironmentDataNames]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EnvironmentDataNames](
	[EnvironmentDataNameId] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentDataName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_EnvironmentDataNames] PRIMARY KEY CLUSTERED 
(
	[EnvironmentDataNameId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EnvironmentDataNames] ON [dbo].[EnvironmentDataNames] 
(
	[EnvironmentDataName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EnvironmentData]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EnvironmentData](
	[EnvironmentDataId] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [int] NOT NULL,
	[EnvironmentDataNameId] [int] NOT NULL,
	[EnvironmentDataValueId] [int] NOT NULL,
 CONSTRAINT [PK_EnvironmentData] PRIMARY KEY CLUSTERED 
(
	[EnvironmentDataId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_EnvironmentData] ON [dbo].[EnvironmentData] 
(
	[SessionId] ASC,
	[EnvironmentDataNameId] ASC,
	[EnvironmentDataValueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Commits]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commits](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Hash] [nvarchar](50) NOT NULL,
	[CommitDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Versions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Versions] ON [dbo].[Commits] 
(
	[Hash] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommitRelations]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommitRelations](
	[ParentCommit] [int] NOT NULL,
	[ChildCommit] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ParentCommit] ASC,
	[ChildCommit] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ActivationMethods]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivationMethods](
	[ActivationMethodId] [int] IDENTITY(1,1) NOT NULL,
	[ActivationMethodName] [nvarchar](255) NULL,
 CONSTRAINT [PK_ActivationMethods] PRIMARY KEY CLUSTERED 
(
	[ActivationMethodId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ActivationMethods] ON [dbo].[ActivationMethods] 
(
	[ActivationMethodName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01/09/2011 14:13:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[AssociatedGuid] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users] ON [dbo].[Users] 
(
	[AssociatedGuid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[UserCount]    Script Date: 01/09/2011 14:14:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UserCount]
	-- Add the parameters for the stored procedure here
	@startDate datetime2, 
	@endDate datetime2,
	@mode int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
  IF @mode = 0
	 SELECT DATEADD(day, DATEDIFF(day,0,StartTime), 0) AS Day, TaggedCommits.Name, COUNT(DISTINCT UserId) 
                      AS UserCount
		FROM     dbo.Sessions 
		LEFT JOIN  dbo.TaggedCommits ON TaggedCommits.CommitId = Sessions.CommitId
		WHERE (TaggedCommits.IsRelease IS NULL OR TaggedCommits.IsRelease = 1)
		  AND StartTime BETWEEN @startDate AND @endDate
		GROUP BY DATEADD(day, DATEDIFF(day,0,StartTime), 0), TaggedCommits.Name
		ORDER BY Day, TaggedCommits.Name;
   ELSE IF @mode = 1
	 SELECT DATEADD(week, DATEDIFF(week,0,StartTime), 0) AS Week, TaggedCommits.Name, COUNT(DISTINCT UserId) 
                      AS UserCount
		FROM     dbo.Sessions 
		LEFT JOIN  dbo.TaggedCommits ON TaggedCommits.CommitId = Sessions.CommitId
		WHERE (TaggedCommits.IsRelease IS NULL OR TaggedCommits.IsRelease = 1)
		  AND StartTime BETWEEN @startDate AND @endDate
		GROUP BY DATEADD(week, DATEDIFF(week,0,StartTime), 0), TaggedCommits.Name
		ORDER BY Week, TaggedCommits.Name;
   ELSE
	 SELECT DATEADD(month, DATEDIFF(month,0,StartTime), 0) AS Month, TaggedCommits.Name, COUNT(DISTINCT UserId) 
                      AS UserCount
		FROM     dbo.Sessions 
		LEFT JOIN  dbo.TaggedCommits ON TaggedCommits.CommitId = Sessions.CommitId
		WHERE (TaggedCommits.IsRelease IS NULL OR TaggedCommits.IsRelease = 1)
		  AND StartTime BETWEEN @startDate AND @endDate
		GROUP BY DATEADD(month, DATEDIFF(month,0,StartTime), 0), TaggedCommits.Name
		ORDER BY Month, TaggedCommits.Name;
END
GO
/****** Object:  StoredProcedure [dbo].[EnvironmentByWeek]    Script Date: 01/09/2011 14:14:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EnvironmentByWeek]
	@envDataNameId int,
	@startDate datetime2, 
	@endDate datetime2
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT
		( SELECT EnvironmentDataValues.EnvironmentDataValue
		  FROM EnvironmentDataValues
		  WHERE EnvironmentDataValues.EnvironmentDataValueId = i.EnvironmentDataValueId
		 ) AS Value,
		 Week,
		 c
	FROM (
		SELECT DATEADD(week, DATEDIFF(week,0,Day), 0) AS Week, data.EnvironmentDataValueId, COUNT(*) AS c
		FROM (
			-- take 1 session from each user and day
			SELECT MIN(SessionId) AS SessionId, UserId, DATEADD(day, DATEDIFF(day, 0, StartTime), 0) AS Day
			FROM         dbo.Sessions
			WHERE StartTime BETWEEN @startDate AND @endDate
			GROUP BY UserId, DATEADD(day, DATEDIFF(day, 0, StartTime), 0)
		) AS session
		LEFT JOIN (
			-- join with the environment data for the requested ID
			SELECT EnvironmentDataValueId,SessionId FROM EnvironmentData WHERE EnvironmentDataNameId = @envDataNameId
		) AS data ON data.SessionId = session.SessionId
		-- group into weeks
		GROUP BY DATEADD(week, DATEDIFF(week,0,Day), 0), data.EnvironmentDataValueId
	) AS i
	ORDER BY Value, Week;
END
GO
USE [UsageDataAnalysis]
GO

/****** Object:  StoredProcedure [dbo].[InstabilityForException]    Script Date: 02/19/2012 16:18:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	Daniel Grunwald
-- Create date: 19.02.2012
-- Description:	Calculates the instability for a single exception group
-- =============================================
CREATE PROCEDURE [dbo].[InstabilityForException]
    @exceptionGroup int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT TotalQuery.VersionName, TotalUserDays, UserDaysWithCrash
	FROM (
		SELECT VersionName, COUNT(*) as TotalUserDays
		FROM (
			SELECT UserId, 
				   DATEADD(day, DATEDIFF(day,0,StartTime), 0) as [Date], 
				   TaggedCommits.Name as [VersionName]
			FROM [Sessions]
			JOIN [TaggedCommits] ON [Sessions].CommitID = [TaggedCommits].CommitId
			Where ClientSessionId != 0 AND [Sessions].IsDebug = 0 AND [TaggedCommits].IsRelease = 1
			GROUP BY UserId, DATEADD(day, DATEDIFF(day,0,StartTime), 0), TaggedCommits.Name
		) AS X
		GROUP BY VersionName
	) AS TotalQuery,
	(
		SELECT VersionName, COUNT(*) As UserDaysWithCrash
		FROM (
			SELECT UserId, 
				   DATEADD(day, DATEDIFF(day,0,StartTime), 0) as [Date], 
				   TaggedCommits.Name as [VersionName]
			FROM [Sessions]
			JOIN [Exceptions] ON [Exceptions].SessionId = [Sessions].SessionId
			JOIN [TaggedCommits] ON [Sessions].CommitID = [TaggedCommits].CommitId
			Where ClientSessionId != 0 AND [Sessions].IsDebug = 0 AND [TaggedCommits].IsRelease = 1 AND ExceptionGroupId = @exceptionGroup
			GROUP BY UserId, DATEADD(day, DATEDIFF(day,0,StartTime), 0), TaggedCommits.Name
		) AS X
		GROUP BY VersionName
	) As CrashQuery
	WHERE TotalQuery.VersionName = CrashQuery.VersionName;
END

GO
/****** Object:  StoredProcedure [dbo].[FeatureIndex]    Script Date: 02/19/2012 21:07:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Daniel Grunwald
-- Create date: 19.02.2012
-- Description:	Index of Feature Uses
-- =============================================
CREATE PROCEDURE [dbo].[FeatureIndex]
	-- Add the parameters for the stored procedure here
	@featureNamePattern varchar(255),
	@commitID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT
		[Features].FeatureId, FeatureName, TotalUseCount, UserDays
	FROM (
		SELECT
			FeatureId,
			SUM(UseCount) AS TotalUseCount,
			COUNT(1) AS UserDays
		FROM (
			SELECT FeatureUse.FeatureId AS FeatureId, COUNT(1) as UseCount
			FROM FeatureUse
			INNER JOIN [Sessions] ON [Sessions].SessionId = FeatureUse.SessionId
			INNER JOIN Features ON [Features].FeatureId = FeatureUse.FeatureId
			WHERE [Sessions].CommitId = @commitID
			  AND FeatureName LIKE @featureNamePattern
			GROUP BY UserId, DATEADD(day, DATEDIFF(day,0,StartTime), 0), FeatureUse.FeatureId
		) AS X
		GROUP BY FeatureId
	) As Y
	INNER JOIN [Features] ON [Features].FeatureId = Y.FeatureId
	ORDER BY UserDays DESC, TotalUseCount DESC
END

GO



/****** Object:  Default [DF__Sessions__IsDebu__1273C1CD]    Script Date: 01/09/2011 14:13:58 ******/
ALTER TABLE [dbo].[Sessions] ADD  DEFAULT ((0)) FOR [IsDebug]
GO
