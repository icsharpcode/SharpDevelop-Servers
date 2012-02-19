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
