USE [FinancialERP1]
GO

/****** Object:  Table [dbo].[AccountTreeHrDeduction]    Script Date: 10/25/2026 12:35:09 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AccountTreeHrDeduction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccTreeId] [int] NULL,
	[DeductionIds] [nvarchar](50) NULL,
 CONSTRAINT [PK_AccountTreeHrDeduction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[AccountTreeHrDeduction]  WITH CHECK ADD  CONSTRAINT [FK_AccountTreeHrDeduction_AccountTree] FOREIGN KEY([AccTreeId])
REFERENCES [dbo].[AccountTree] ([Id])
GO

ALTER TABLE [dbo].[AccountTreeHrDeduction] CHECK CONSTRAINT [FK_AccountTreeHrDeduction_AccountTree]
GO

