--USE [ScrumFactory]
--GO

/****** Object:  Table [factory].[Artifact]    Script Date: 02/09/2013 17:29:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [factory].[Artifact](
	[ArtifactUId] [char](36) NOT NULL,
	[ProjectUId] [char](36) NOT NULL,
	[ContextUId] [char](36) NOT NULL,
	[ArtifactName] [nvarchar](255) NULL,
	[ArtifactPath] [nvarchar](500) NULL,
	[ArtifactContext] [smallint] NOT NULL,
 CONSTRAINT [PK_Artifact] PRIMARY KEY CLUSTERED 
(
	[ArtifactUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [factory].[AuthorizationInfo](
	[MemberUId] [varchar](255) NOT NULL,
	[Token] [varchar](500) NOT NULL,
	[ProviderName] [varchar](50) NOT NULL,
	[IssueDate] [datetime] NOT NULL,
 CONSTRAINT [PK_AuhtoriztionInfo] PRIMARY KEY CLUSTERED 
(
	[MemberUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO



SET ANSI_PADDING ON
GO

CREATE TABLE [factory].[SizeIdealHour](
	[IdealHourUId] [char](36) NOT NULL,
	[ItemSizeUId] [char](36) NOT NULL,
	[RoleShortName] [varchar](15) NOT NULL,
	[Hours] [decimal](6, 1) NULL,
 CONSTRAINT [PK_IdealHour] PRIMARY KEY CLUSTERED 
(
	[IdealHourUId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [factory].[SizeIdealHour]  WITH CHECK ADD  CONSTRAINT [FK_IdealHour_ItemSize] FOREIGN KEY([ItemSizeUId])
REFERENCES [factory].[ItemSize] ([ItemSizeUId])
GO

ALTER TABLE [factory].[SizeIdealHour] CHECK CONSTRAINT [FK_IdealHour_ItemSize]
GO


ALTER TABLE factory.BacklogItem ADD ArtifactCount int NULL;
GO


ALTER TABLE factory.Task ADD ArtifactCount int NULL;
GO

ALTER TABLE factory.MemberProfile ADD TeamCode varchar(50) NULL;
GO