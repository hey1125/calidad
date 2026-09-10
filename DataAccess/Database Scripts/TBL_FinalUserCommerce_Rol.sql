/****** Object:  Table [dbo].[TBL_FinalUserCommerce_Rol]    Script Date: 14/07/2025 11:33:36 a. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TBL_FinalUserCommerce_Rol](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[UserId] [int] NOT NULL,
	[CommerceId] [int] NOT NULL,
	[RolId] [int] NOT NULL,
 CONSTRAINT [PK_TBL_FinalUserCommerce_Rol] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol] ADD  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol]  WITH CHECK ADD  CONSTRAINT [FK_Commerce] FOREIGN KEY([CommerceId])
REFERENCES [dbo].[TBL_Commerce] ([Id])
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol] CHECK CONSTRAINT [FK_Commerce]
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol]  WITH CHECK ADD  CONSTRAINT [FK_Rol] FOREIGN KEY([RolId])
REFERENCES [dbo].[TBL_Rol] ([Id])
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol] CHECK CONSTRAINT [FK_Rol]
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol]  WITH CHECK ADD  CONSTRAINT [FK_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[TBL_FinalUser] ([Id])
GO

ALTER TABLE [dbo].[TBL_FinalUserCommerce_Rol] CHECK CONSTRAINT [FK_User]
GO



