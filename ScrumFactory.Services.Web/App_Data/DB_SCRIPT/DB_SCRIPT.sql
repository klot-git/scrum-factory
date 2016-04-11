--IF NOT EXISTS (SELECT * FROM sys.databases where name='[ScrumFactory]')
--CREATE DATABASE [ScrumFactory]
--GO

If not Exists (select loginname from master.dbo.syslogins 
    where name = 'sf_db_user' and dbname = 'ScrumFactory')
CREATE LOGIN [sf_db_user] WITH PASSWORD=N'sf', DEFAULT_DATABASE=[ScrumFactory], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

/***** DB STARTS HERE ***************/

--USE [ScrumFactory]
--GO

CREATE USER [sf_db_user] FOR LOGIN [sf_db_user] WITH DEFAULT_SCHEMA=[dbo]
GO

/****** Object:  Schema [factory]    Script Date: 04/05/2012 16:38:56 ******/
CREATE SCHEMA [factory] AUTHORIZATION [sf_db_user]
GO
/****** Object:  Table [factory].[Proposal]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Proposal](
	[ProposalUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[ProposalName] [nvarchar](100) NOT NULL,
	[ProposalStatus] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[EstimatedStartDate] [datetime] NOT NULL,
	[EstimatedEndDate] [datetime] NOT NULL,
	[CurrencySymbol] [varchar](5) NOT NULL,
	[TotalValue] [decimal](16, 2) NOT NULL,
	[Discount] [decimal](9, 2) NOT NULL,
	[UseCalcPrice] [bit] NOT NULL,
	[TemplateName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](300) NULL,
	[ApprovalDate] [datetime] NULL,
	[ApprovedBy] [varchar](80) NULL,
	[RejectReason] [smallint] NULL,
 CONSTRAINT [PK_Proposal] PRIMARY KEY CLUSTERED 
(
	[ProposalUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[Task]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Task](
	[TaskUId] [char](36) NOT NULL,
	[TaskNumber] [int] IDENTITY(1,1) NOT NULL,
	[TaskName] [nvarchar](300) NOT NULL,
	[TaskOwnerUId] [varchar](80) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[BacklogItemUId] [char](36) NOT NULL,
	[Priority] [smallint] NOT NULL,
	[TaskType] [smallint] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Status] [smallint] NOT NULL,
	[PlannedHours] [decimal](6, 1) NOT NULL,
	[EffectiveHours] [decimal](6, 1) NOT NULL,
	[TaskAssigneeUId] [varchar](80) NULL,
	[RoleUId] [char](36) NULL,
	[EndDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[IsAccounting] [bit] NOT NULL	
 CONSTRAINT [PK_Task] PRIMARY KEY NONCLUSTERED 
(
	[TaskUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_TaskNumber] UNIQUE NONCLUSTERED 
(
	[TaskNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[MemberProfile]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[MemberProfile](
	[MemberUId] [varchar](255) NOT NULL,
	[EmailAccount] [varchar](150) NULL,
	[FullName] [nvarchar](255) NOT NULL,
	[IsFactoryOwner] [bit] NOT NULL,
	[CanSeeProposalValues] [bit] NOT NULL,
	[CompanyName] [nvarchar](255) NULL,
	[Skills] [nvarchar](255) NULL,
	[ContactData] [nvarchar](255) NULL,
	[AuthorizationProvider] [varchar](50) NULL,
	[CreateBy] [varchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_MemberProfile_1] PRIMARY KEY CLUSTERED 
(
	[MemberUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[Project]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Project](
	[ProjectUId] [char](36) NOT NULL,
	[ProjectNumber] [int] IDENTITY(1,1) NOT NULL,
	[ProjectName] [nvarchar](200) NOT NULL,
	[ClientName] [nvarchar](50) NOT NULL,
	[Status] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateBy] [varchar](80) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[Platform] [nvarchar](100) NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CodeRepositoryPath] [varchar](300) NULL,
	[DocRepositoryPath] [varchar](300) NULL,
	[IsSuspended] [bit] NOT NULL,
	[AnyoneCanJoin] [bit] NOT NULL,
	[ProjectType] [smallint] NOT NULL,
	[ProjectParentUId] [char](36) NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[ProjectUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ItemSize]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ItemSize](
	[ItemSizeUId] [char](36) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[Size] [int] NOT NULL,
	[OccurrenceConstraint] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_ItemSize] PRIMARY KEY CLUSTERED 
(
	[ItemSizeUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[RoleHourCost]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[RoleHourCost](
	[RoleUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[Cost] [decimal](10, 2) NULL,
	[Price] [decimal](10, 2) NULL,
 CONSTRAINT [PK_RoleHourCost_1] PRIMARY KEY CLUSTERED 
(
	[RoleUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[Role]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Role](
	[RoleUId] [char](36) NOT NULL,
	[RoleShortName] [varchar](15) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[RoleName] [nvarchar](50) NOT NULL,
	[RoleDescription] [nvarchar](255) NULL,
	[PermissionSet] [smallint] NOT NULL,
	[IsPlanned] [bit] NOT NULL,
	[IsDefaultRole] [bit] NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[RoleUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[Risk]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Risk](
	[RiskUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[RiskDescription] [nvarchar](300) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[Impact] [smallint] NOT NULL,
	[Probability] [smallint] NOT NULL,
	[IsPrivate] [bit] NOT NULL,
	[RiskAction] [nvarchar](500) NULL,
	[UpdatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Risk] PRIMARY KEY CLUSTERED 
(
	[RiskUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ProposalItem]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ProposalItem](
	[ProposalUId] [char](36) NOT NULL,
	[BacklogItemUId] [char](36) NOT NULL,
 CONSTRAINT [PK_ProposalItem] PRIMARY KEY CLUSTERED 
(
	[ProposalUId] ASC,
	[BacklogItemUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ProposalFixedCost]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ProposalFixedCost](
	[ProposalFixedCostUId] [char](36) NOT NULL,
	[ProposalUId] [char](36) NOT NULL,
	[CostDescription] [nvarchar](255) NULL,
	[Cost] [decimal](10, 3) NOT NULL,
	[RepassToClient] [bit] NOT NULL,
 CONSTRAINT [PK_ProposalFixedCost] PRIMARY KEY CLUSTERED 
(
	[ProposalFixedCostUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ProposalDocument]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ProposalDocument](
	[ProposalUId] [char](36) NOT NULL,
	[ProposalXAML] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ProposalDocument] PRIMARY KEY CLUSTERED 
(
	[ProposalUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ProposalClause]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ProposalClause](
	[ProposalUId] [char](36) NOT NULL,
	[ClauseOrder] [int] NOT NULL,
	[ClauseName] [nvarchar](100) NULL,
	[ClauseText] [nvarchar](500) NULL,
 CONSTRAINT [PK_ProposalClause] PRIMARY KEY CLUSTERED 
(
	[ProposalUId] ASC,
	[ClauseOrder] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[BacklogItemGroup]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[BacklogItemGroup](
	[GroupUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[GroupName] [nvarchar](50) NOT NULL,
	[GroupColor] [varchar](20) NOT NULL,
	[DefaultGroup] [smallint] NOT NULL,
 CONSTRAINT [PK_BacklogItemGroup] PRIMARY KEY CLUSTERED 
(
	[GroupUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[TaskDetail]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[TaskDetail](
	[TaskUId] [char](36) NOT NULL,
	[Detail] [nvarchar](4000) NULL,
 CONSTRAINT [PK_TaskDetail] PRIMARY KEY CLUSTERED 
(
	[TaskUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[MemberAvatar]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[MemberAvatar](
	[MemberUId] [varchar](255) NOT NULL,
	[AvatarImage] [image] NOT NULL,
 CONSTRAINT [PK_MemberAvatar] PRIMARY KEY CLUSTERED 
(
	[MemberUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO

/****** Object:  Table [factory].[Sprint]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[Sprint](
	[SprintUId] [char](36) NOT NULL,
	[SprintNumber] [int] NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
 CONSTRAINT [PK_Sprint] PRIMARY KEY CLUSTERED 
(
	[SprintUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[ProjectMembership]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[ProjectMembership](
	[ProjectUId] [char](36) NOT NULL,
	[MemberUId] [varchar](80) NOT NULL,
	[RoleUId] [char](36) NOT NULL,
	[DayAllocation] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[InactiveSince] [datetime] NULL,
 CONSTRAINT [PK_ProjectMembership_1] PRIMARY KEY CLUSTERED 
(
	[ProjectUId] ASC,
	[MemberUId] ASC,
	[RoleUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [factory].[BacklogItemEffectiveHours]    Script Date: 04/05/2012 16:38:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE view [factory].[BacklogItemEffectiveHours] as 
select
	ProjectUId,
	BacklogItemUId,
	ISNULL(SprintNumber, 0) as SprintNumber,
	sum(EffectiveHours) as EffectiveHours
from (
select
	ProjectUId,
	BacklogItemUId,
	EffectiveHours,
	(select top 1 SprintNumber from factory.Sprint s where s.ProjectUId = t.ProjectUId and s.StartDate <= t.CreatedAt and s.EndDate >= t.CreatedAt) as SprintNumber
 from factory.Task t
 ) as tt
 group by ProjectUId, BacklogItemUId, SprintNumber
GO
/****** Object:  Table [factory].[BacklogItem]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[BacklogItem](
	[BacklogItemUId] [char](36) NOT NULL,
	[BacklogItemNumber] [int] NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Status] [smallint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[DeliveryDate] [datetime] NULL,
	[StartedAt] [datetime] NULL,
	[FinishedAt] [datetime] NULL,
	[BusinessPriority] [int] NOT NULL,
	[Size] [int] NULL,
	[SizeFactor] [int] NOT NULL,
	[ItemSizeUId] [char](36) NULL,
	[OccurrenceConstraint] [int] NOT NULL,
	[GroupUId] [char](36) NULL,
	[IssueType] [smallint] NOT NULL,
	[CancelReason] [smallint] NULL	
 CONSTRAINT [PK_BacklogItem] PRIMARY KEY CLUSTERED 
(
	[BacklogItemUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [IX_BacklogItemNumber] UNIQUE NONCLUSTERED 
(
	[ProjectUId] ASC,
	[BacklogItemNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [factory].[PlannedHours]    Script Date: 04/05/2012 16:38:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [factory].[PlannedHours](
	[BacklogItemUId] [char](36) NOT NULL,
	[RoleUId] [char](36) NOT NULL,
	[PlanningNumber] [int] NOT NULL,
	[SprintNumber] [int] NULL,
	[Hours] [decimal](6, 1) NULL,
 CONSTRAINT [PK_PlannedHours] PRIMARY KEY CLUSTERED 
(
	[BacklogItemUId] ASC,
	[RoleUId] ASC,
	[PlanningNumber] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [factory].[TaskInfo]    Script Date: 04/05/2012 16:38:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [factory].[TaskInfo] as 
select
	t.TaskUId,	
	p.ProjectNumber,
	p.ProjectName,
	i.BacklogItemNumber,
	i.Name as BacklogItemName
from
	factory.Task t
inner join
	factory.Project	p on p.ProjectUId = t.ProjectUId
inner join
	factory.BacklogItem i on i.BacklogItemUId = t.BacklogItemUId
GO
/****** Object:  StoredProcedure [factory].[sp_DeleteProject]    Script Date: 04/05/2012 16:38:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [factory].[sp_DeleteProject] 			
	@pNumber int
AS
BEGIN	

DECLARE @id CHAR(36)
SELECT @id=ProjectUid FROM factory.Project WHERE ProjectNumber = @pNumber

DELETE factory.PlannedHours WHERE factory.PlannedHours.BacklogItemUId IN (SELECT BacklogItemUId FROM factory.BacklogItem WHERE ProjectUId = @id)

DELETE factory.BacklogItem WHERE ProjectUId = @id
DELETE factory.BacklogItemGroup WHERE ProjectUId = @id

DELETE factory.ProjectMembership WHERE ProjectUId = @id
DELETE factory.[RoleHourCost] WHERE ProjectUId = @id
DELETE factory.[Role] WHERE ProjectUId = @id

DELETE factory.[Risk] WHERE ProjectUId = @id

DELETE factory.Sprint WHERE ProjectUId = @id

DELETE factory.ProposalClause WHERE factory.ProposalClause.ProposalUId IN (SELECT ProposalUId FROM factory.Proposal WHERE ProjectUId=@id)
DELETE factory.ProposalDocument WHERE factory.ProposalDocument.ProposalUId IN (SELECT ProposalUId FROM factory.Proposal WHERE ProjectUId=@id)
DELETE factory.ProposalItem WHERE factory.ProposalItem.ProposalUId IN (SELECT ProposalUId FROM factory.Proposal WHERE ProjectUId=@id)
DELETE factory.ProposalFixedCost WHERE factory.ProposalFixedCost.ProposalUId IN (SELECT ProposalUId FROM factory.Proposal WHERE ProjectUId=@id)

DELETE factory.Proposal WHERE ProjectUId = @id


DELETE factory.Project WHERE ProjectUId = @id
END
GO
/****** Object:  Default [DF_BacklogItem_CreateDate]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[BacklogItem] ADD  CONSTRAINT [DF_BacklogItem_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
/****** Object:  Default [DF_Task_IsAccounting]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[Task] ADD  CONSTRAINT [DF_Task_IsAccounting]  DEFAULT ((1)) FOR [IsAccounting]
GO
/****** Object:  ForeignKey [FK_BacklogItem_BacklogItemGroup]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[BacklogItem]  WITH CHECK ADD  CONSTRAINT [FK_BacklogItem_BacklogItemGroup] FOREIGN KEY([GroupUId])
REFERENCES [factory].[BacklogItemGroup] ([GroupUId])
GO
ALTER TABLE [factory].[BacklogItem] CHECK CONSTRAINT [FK_BacklogItem_BacklogItemGroup]
GO
/****** Object:  ForeignKey [FK_BacklogItem_Project]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[BacklogItem]  WITH CHECK ADD  CONSTRAINT [FK_BacklogItem_Project] FOREIGN KEY([ProjectUId])
REFERENCES [factory].[Project] ([ProjectUId])
GO
ALTER TABLE [factory].[BacklogItem] CHECK CONSTRAINT [FK_BacklogItem_Project]
GO
/****** Object:  ForeignKey [FK_BacklogItemGroup_Project]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[BacklogItemGroup]  WITH CHECK ADD  CONSTRAINT [FK_BacklogItemGroup_Project] FOREIGN KEY([ProjectUId])
REFERENCES [factory].[Project] ([ProjectUId])
GO
ALTER TABLE [factory].[BacklogItemGroup] CHECK CONSTRAINT [FK_BacklogItemGroup_Project]
GO
/****** Object:  ForeignKey [FK_MemberAvatar_MemberProfile]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[MemberAvatar]  WITH CHECK ADD  CONSTRAINT [FK_MemberAvatar_MemberProfile] FOREIGN KEY([MemberUId])
REFERENCES [factory].[MemberProfile] ([MemberUId])
GO
ALTER TABLE [factory].[MemberAvatar] CHECK CONSTRAINT [FK_MemberAvatar_MemberProfile]
GO
/****** Object:  ForeignKey [FK_PlannedHours_BacklogItem]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[PlannedHours]  WITH CHECK ADD  CONSTRAINT [FK_PlannedHours_BacklogItem] FOREIGN KEY([BacklogItemUId])
REFERENCES [factory].[BacklogItem] ([BacklogItemUId])
GO
ALTER TABLE [factory].[PlannedHours] CHECK CONSTRAINT [FK_PlannedHours_BacklogItem]
GO
/****** Object:  ForeignKey [FK_PlannedHours_Role]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[PlannedHours]  WITH CHECK ADD  CONSTRAINT [FK_PlannedHours_Role] FOREIGN KEY([RoleUId])
REFERENCES [factory].[Role] ([RoleUId])
GO
ALTER TABLE [factory].[PlannedHours] CHECK CONSTRAINT [FK_PlannedHours_Role]
GO
/****** Object:  ForeignKey [FK_ProjectMembership_Role]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[ProjectMembership]  WITH CHECK ADD  CONSTRAINT [FK_ProjectMembership_Role] FOREIGN KEY([RoleUId])
REFERENCES [factory].[Role] ([RoleUId])
GO
ALTER TABLE [factory].[ProjectMembership] CHECK CONSTRAINT [FK_ProjectMembership_Role]
GO
/****** Object:  ForeignKey [FK_ProposalClause_Proposal]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[ProposalClause]  WITH CHECK ADD  CONSTRAINT [FK_ProposalClause_Proposal] FOREIGN KEY([ProposalUId])
REFERENCES [factory].[Proposal] ([ProposalUId])
GO
ALTER TABLE [factory].[ProposalClause] CHECK CONSTRAINT [FK_ProposalClause_Proposal]
GO
/****** Object:  ForeignKey [FK_ProposalDocument_Proposal]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[ProposalDocument]  WITH CHECK ADD  CONSTRAINT [FK_ProposalDocument_Proposal] FOREIGN KEY([ProposalUId])
REFERENCES [factory].[Proposal] ([ProposalUId])
GO
ALTER TABLE [factory].[ProposalDocument] CHECK CONSTRAINT [FK_ProposalDocument_Proposal]
GO
/****** Object:  ForeignKey [FK_ProposalFixedCost_Proposal]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[ProposalFixedCost]  WITH CHECK ADD  CONSTRAINT [FK_ProposalFixedCost_Proposal] FOREIGN KEY([ProposalUId])
REFERENCES [factory].[Proposal] ([ProposalUId])
GO
ALTER TABLE [factory].[ProposalFixedCost] CHECK CONSTRAINT [FK_ProposalFixedCost_Proposal]
GO
/****** Object:  ForeignKey [FK_ProposalItem_Proposal]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[ProposalItem]  WITH CHECK ADD  CONSTRAINT [FK_ProposalItem_Proposal] FOREIGN KEY([ProposalUId])
REFERENCES [factory].[Proposal] ([ProposalUId])
GO
ALTER TABLE [factory].[ProposalItem] CHECK CONSTRAINT [FK_ProposalItem_Proposal]
GO
/****** Object:  ForeignKey [FK_Risk_Project]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[Risk]  WITH CHECK ADD  CONSTRAINT [FK_Risk_Project] FOREIGN KEY([ProjectUId])
REFERENCES [factory].[Project] ([ProjectUId])
GO
ALTER TABLE [factory].[Risk] CHECK CONSTRAINT [FK_Risk_Project]
GO
/****** Object:  ForeignKey [FK_Role_Project]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_Project] FOREIGN KEY([ProjectUId])
REFERENCES [factory].[Project] ([ProjectUId])
GO
ALTER TABLE [factory].[Role] CHECK CONSTRAINT [FK_Role_Project]
GO
/****** Object:  ForeignKey [FK_Sprint_Project]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[Sprint]  WITH CHECK ADD  CONSTRAINT [FK_Sprint_Project] FOREIGN KEY([ProjectUId])
REFERENCES [factory].[Project] ([ProjectUId])
GO
ALTER TABLE [factory].[Sprint] CHECK CONSTRAINT [FK_Sprint_Project]
GO
/****** Object:  ForeignKey [FK_TaskDetail_Task]    Script Date: 04/05/2012 16:38:40 ******/
ALTER TABLE [factory].[TaskDetail]  WITH CHECK ADD  CONSTRAINT [FK_TaskDetail_Task] FOREIGN KEY([TaskUId])
REFERENCES [factory].[Task] ([TaskUId])
GO
ALTER TABLE [factory].[TaskDetail] CHECK CONSTRAINT [FK_TaskDetail_Task]
GO

/****** INDEX HERE **********************/

CREATE NONCLUSTERED INDEX [IX_Task] ON [factory].[Task] 
(
	[ProjectUId] ASC,
	[Status] ASC,
	[EndDate] ASC
)
INCLUDE ( [TaskUId],
[TaskNumber],
[TaskName],
[TaskOwnerUId],
[BacklogItemUId],
[Priority],
[TaskType],
[CreatedAt],
[PlannedHours],
[EffectiveHours],
[TaskAssigneeUId],
[RoleUId],
[StartDate],
[IsAccounting]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/************** INSERT HERE *****************/

INSERT [factory].[ItemSize] ([ItemSizeUId], [Name], [Description], [Size], [OccurrenceConstraint], [IsActive]) VALUES (N'42c390e2-fdb4-46e2-b942-374cc1c3f2b8', N'Sprint Planning & Review', N'Review the previous sprint and plan the next one.', 8, 0, 1)
GO
INSERT [factory].[ItemSize] ([ItemSizeUId], [Name], [Description], [Size], [OccurrenceConstraint], [IsActive]) VALUES (N'd5166047-9c0d-4bea-935b-301829f1425e', N'Sprint Delivery', N'Delivery and install sprint items.', 8, 2, 1)
GO

