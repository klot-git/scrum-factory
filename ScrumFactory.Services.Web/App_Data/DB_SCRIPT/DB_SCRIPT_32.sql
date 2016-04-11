/****** Object:  Table [dbo].[CalendarDay]    Script Date: 03/05/2014 07:48:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [factory].[CalendarDay](
	[Day] [int] NOT NULL,
	[Month] [int] NOT NULL,
	[Year] [int] NOT NULL,
	[HolidayDescription] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_CalendarDay] PRIMARY KEY CLUSTERED 
(
	[Day] ASC,
	[Month] ASC,
	[Year] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [factory].[TaskTag](
	[TagUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_TaskTag] PRIMARY KEY CLUSTERED 
(
	[TagUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [factory].[Task] ADD [TagUId] [char](36) NULL 

GO


CREATE TABLE [factory].[ProjectConstraint](
	[ConstraintUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[ConstraintId] [varchar](5) NOT NULL,
	[Constraint] [nvarchar](500) NOT NULL,
	[ConstraintGroup] [smallint] NOT NULL,
	[AdjustPointFactor] [float] NOT NULL,
 CONSTRAINT [PK_ProjectConstraint] PRIMARY KEY CLUSTERED 
(
	[ConstraintUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [factory].[Project] ADD [Baseline] [int] NULL 
GO

ALTER TABLE [factory].[Proposal] ADD CurrencyRate [decimal](16, 2) NULL 
GO


DROP INDEX [IX_Task] ON [factory].[Task]
GO

ALTER TABLE [factory].[Task] ALTER COLUMN PlannedHours decimal(6,2)
GO
ALTER TABLE [factory].[Task] ALTER COLUMN EffectiveHours decimal(6,2)
GO
CREATE NONCLUSTERED INDEX [IX_Task] ON [factory].[Task]
(
	[ProjectUId] ASC,
	[Status] ASC,
	[EndDate] ASC
)
INCLUDE ( 	[TaskUId],
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
	[IsAccounting]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
