/****** Object:  Table [dbo].[TBL_Commerce]    Script Date: 02/07/2025 06:49:41 p. m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TBL_Commerce](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Updated] [datetime] NULL,
	[LegalId] [nvarchar](12) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Phone] [nvarchar](8) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Latitude] [decimal](10, 8) NOT NULL,
	[Longitude] [decimal](11, 8) NOT NULL,
	[Status] [nvarchar](15) NOT NULL,
	[IBAN] [nvarchar](22) NOT NULL,
	[CommissionRate] [decimal](5, 2) NULL,
 CONSTRAINT [PK_TBL_Commerce] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

