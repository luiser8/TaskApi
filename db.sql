USE [Tasks]
GO
/****** Object:  Table [dbo].[Tasks]    Script Date: 2/2/2021 9:03:15 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Tasks](
	[IdTask] [int] IDENTITY(1,1) NOT NULL,
	[IdUser] [int] NOT NULL,
	[Name] [varchar](125) NOT NULL,
	[Priority] [varchar](15) NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[Status] [tinyint] NULL,
	[CreateTask] [datetime] NOT NULL,
 CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED 
(
	[IdTask] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2/2/2021 9:03:15 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Users](
	[IdUser] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](125) NOT NULL,
	[LastName] [varchar](125) NOT NULL,
	[Email] [varchar](95) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[Status] [tinyint] NULL,
	[CreateUser] [datetime] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[IdUser] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[Tasks] ADD  CONSTRAINT [DF__Tasks__CreateTas__1367E606]  DEFAULT (getdate()) FOR [CreateTask]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__CreateUse__108B795B]  DEFAULT (getdate()) FOR [CreateUser]
GO
ALTER TABLE [dbo].[Tasks]  WITH CHECK ADD  CONSTRAINT [FK__Tasks__IdUser__145C0A3F] FOREIGN KEY([IdUser])
REFERENCES [dbo].[Users] ([IdUser])
GO
ALTER TABLE [dbo].[Tasks] CHECK CONSTRAINT [FK__Tasks__IdUser__145C0A3F]
GO
