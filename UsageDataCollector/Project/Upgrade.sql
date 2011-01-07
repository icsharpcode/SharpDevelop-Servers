USE [UsageDataAnalysis]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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

/****** Object:  Table [dbo].[Commits]    Script Date: 01/07/2011 17:03:46 ******/
CREATE TABLE [dbo].[Commits](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Hash] [nchar](40) NOT NULL,
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
/****** Object:  Table [dbo].[CommitRelations]    Script Date: 01/07/2011 17:21:14 ******/
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


/****** Object:  Table [dbo].[TaggedCommits]    Script Date: 01/07/2011 17:03:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaggedCommits](
	[TagId] [int] IDENTITY(1,1) NOT NULL,
	[CommitId] [int] NOT NULL,
	[Name] [nchar](20) NOT NULL,
	[IsRelease] [bit] NOT NULL,
 CONSTRAINT [PK_TaggedVersions] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO






/* AFTER RUNNING ImportGitRepository ONCE, RUN THIS STATEMENT TO FIX UP THE SD4.0-BETA1 RELEASE */
UPDATE Sessions SET CommitId = (SELECT Id FROM Commits WHERE Hash='r5948') WHERE AppVersionRevision=5949;
