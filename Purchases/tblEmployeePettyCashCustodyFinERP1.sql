USE [FinancialERP1]
GO

/****** Object:  Table [dbo].[EmployeePettyCashCustody]    Script Date: 10/25/2026 12:35:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmployeePettyCashCustody](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NULL,
	[TotalDeductionAmount] [decimal](18, 2) NULL,
	[PaiedAmount] [decimal](18, 2) NULL,
	[AccTreeId] [int] NULL,
	[FinancialCycleId] [int] NULL,
 CONSTRAINT [PK_EmployeePettyCashCustody] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[EmployeePettyCashCustody]  WITH CHECK ADD  CONSTRAINT [FK_EmployeePettyCashCustody_AccountTree] FOREIGN KEY([AccTreeId])
REFERENCES [dbo].[AccountTree] ([Id])
GO

ALTER TABLE [dbo].[EmployeePettyCashCustody] CHECK CONSTRAINT [FK_EmployeePettyCashCustody_AccountTree]
GO

ALTER TABLE [dbo].[EmployeePettyCashCustody]  WITH CHECK ADD  CONSTRAINT [FK_EmployeePettyCashCustody_FinancialCycle] FOREIGN KEY([FinancialCycleId])
REFERENCES [dbo].[FinancialCycle] ([Id])
GO

ALTER TABLE [dbo].[EmployeePettyCashCustody] CHECK CONSTRAINT [FK_EmployeePettyCashCustody_FinancialCycle]
GO

