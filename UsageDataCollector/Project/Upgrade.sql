USE [UsageDataAnalysis]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE dbo.ExceptionGroups DROP COLUMN UserFixedInRevision;
ALTER TABLE dbo.ExceptionGroups ADD UserFixedInCommit int NULL;

/* New columns for Session: */
ALTER TABLE dbo.Sessions ADD CommitId [int] NULL
GO

ALTER TABLE dbo.[Sessions] ADD IsDebug BIT NOT NULL DEFAULT(0);
GO
UPDATE dbo.[Sessions] SET Sessions.IsDebug=1
WHERE EXISTS (SELECT * FROM EnvironmentData
			  JOIN EnvironmentDataNames ON EnvironmentData.EnvironmentDataNameId = EnvironmentDataNames.EnvironmentDataNameId
			  WHERE EnvironmentDataNames.EnvironmentDataName = 'debug'
			    AND EnvironmentData.SessionId = Sessions.SessionId
			 );
GO
ALTER TABLE dbo.[Sessions] ALTER COLUMN IsDebug BIT NOT NULL;
GO

ALTER TABLE dbo.[Sessions] ADD FirstException datetime NULL;
GO
UPDATE dbo.[Sessions] SET Sessions.FirstException=
 (SELECT MIN(ThrownAt)
  FROM Exceptions
  WHERE Exceptions.SessionId = Sessions.SessionId
 );
GO

ALTER TABLE dbo.[Sessions] ADD LastFeatureUse datetime NULL;
GO
UPDATE dbo.[Sessions] SET Sessions.LastFeatureUse=
 (SELECT MAX(UseTime)
  FROM FeatureUse
  WHERE FeatureUse.SessionId = Sessions.SessionId
 );
GO

/****** Object:  Table [dbo].[Commits]    Script Date: 01/07/2011 23:16:54 ******/
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
/****** Object:  Table [dbo].[CommitRelations]    Script Date: 01/07/2011 23:16:54 ******/
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

/****** Object:  Table [dbo].[TaggedCommits]    Script Date: 01/07/2011 23:16:54 ******/
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


DROP PROCEDURE [dbo].[DailyUsers];
GO

/****** Object:  StoredProcedure [dbo].[UserCount]    Script Date: 01/07/2011 23:16:57 ******/
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

/* Fix indices */

DROP INDEX [IX_EnvironmentData] ON [dbo].[EnvironmentData];
GO
CREATE NONCLUSTERED INDEX [IX_EnvironmentData] ON [dbo].[EnvironmentData] 
(
	[SessionId] ASC,
	[EnvironmentDataNameId] ASC,
	[EnvironmentDataValueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
