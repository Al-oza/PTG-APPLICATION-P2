Use [UzimaRx]

GO
CREATE TABLE [dbo].[AuthGroupPermissions] (
    [Id]  INT  NOT NULL,
    [GroupId]    INT  NOT NULL,
    [PermissionId] INT  NOT NULL,
    CONSTRAINT [PK_dbo.AuthGroupPermissions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[AuthGroup] (
    [Id]   INT   NOT NULL,
    [Permission] INT   NOT NULL,
    CONSTRAINT [PK_dbo.AuthGroup] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[AuthUser] (
    [Id]  INT  NOT NULL,
    [Password]  NVARCHAR (256)  NOT NULL,
    [LastLogin]  DATETIME NULL,
    [IsSuperUser] BIT  NOT NULL,
    [Username]  NVARCHAR (256)  NOT NULL,
    [FirstName]  NVARCHAR (256)  NOT NULL,
    [LastName]  NVARCHAR (256)  NOT NULL,
    [Email]   NVARCHAR (256)  NOT NULL,
    [IsStaff]   BIT  NOT NULL,
    [IsActive]  BIT  NOT NULL,
    [DateJoined] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.AuthUser] PRIMARY KEY CLUSTERED ([Id] ASC)
);



GO
CREATE TABLE [dbo].[PharmacyDrug] (
    [Id]   INT   NOT NULL,
    [Barcode]  NVARCHAR (256) NULL,
    [Name]  NVARCHAR (256)  NOT NULL,
	[BrandName]   NVARCHAR (256)  NULL,
	[ApplicationNumber] NVARCHAR (256) NULL,
	[Manufacturer] NVARCHAR(256)   NOT NULL,
	[Manufacturer Location] NVARCHAR(256) NULL,
	[ApprovalNumber] NVARChar (256) NULL,
	[Schedule]   NVARCHAR(256)  NULL,
    [License]  NVARCHAR (256) NULL,
	[Ingredients] NVARCHAR (256) NULL,
    [PackSize]  NVARCHAR(256)  NULL,
    CONSTRAINT [PK_dbo.PharmacyDrugBrand] PRIMARY KEY CLUSTERED ([Id] ASC),
);

GO
CREATE TABLE [dbo].[PharmacyBatch] (
    [Id]  INT   NOT NULL,
    [ExpirationDate] DATETIME NOT NULL,
    [BatchSize]  INT  NOT NULL,
    [DrugBrandId] INT  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacyBatch] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.PharmacyBatch_dbo.PharmacyDrugBrand_DrugBrandId] FOREIGN KEY ([DrugBrandId]) REFERENCES [dbo].[PharmacyDrugBrand] ([Id]) ON DELETE CASCADE
);

GO
CREATE TABLE [dbo].[PharmacyLocation] (
    [Id]   INT   NOT NULL,
    [Name]  NVARCHAR (256)  NOT NULL,
    [UpstremSupplier] INT  NULL,
    [IsHospital] BIT  NOT NULL,
    [IsClinic]  BIT  NOT NULL,
    [Address]  NVARCHAR (256)  NOT NULL,
    [Phone]  NVARCHAR (256)  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacyLocation] PRIMARY KEY CLUSTERED ([Id] ASC)
    --add FK for upstream supplier?
);

GO
CREATE TABLE [dbo].[PharmacyBatchLocation] (
    [Id]  INT  NOT NULL,
    [Count]  INT  NULL,
    [BatchId]  INT  NOT NULL,
    [LocationId] INT  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacyBatchLocation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.PharmacyBatchLocation_dbo.PharmacyBatch_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[PharmacyBatch] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PharmacyBatchLocation_dbo.PharmacyLocation_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[PharmacyLocation] ([Id]) ON DELETE CASCADE
);

GO
CREATE TABLE [dbo].[PharmacyStatus] (
    [Id]  INT  NOT NULL,
    [Status]  NVARCHAR (256)  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacyStatus] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[Item] (
    [Id]  INT   NOT NULL,
    [DateOrdered] DATETIME NOT NULL,
    [UserId]  INT   NOT NULL, --use this as user or do the PK or AuthUser Table?
    [BarcodeId]  INT  NOT NULL,
    [StatusId]  INT  NOT NULL,
    [CurrentLocationId] INT   NOT NULL,
    [FutureLocationId] INT ,
    [ExpirationDate] DATETIME NOT NULL,--is this a FK to PharmacyBatch Table?
    CONSTRAINT [PK_dbo.PharmacyOrder] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyDrugBrand_BarcodeId] FOREIGN KEY ([BarcodeId]) REFERENCES [dbo].[PharmacyDrugBrand] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[PharmacyStatus] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyLocation_CurrentLocationId] FOREIGN KEY ([CurrentLocationId]) REFERENCES [dbo].[PharmacyLocation] ([Id]) ON DELETE NO ACTION, --may cause cycles or multiple paths
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyLocation_FutureLocationId] FOREIGN KEY ([FutureLocationId]) REFERENCES [dbo].[PharmacyLocation] ([Id]) ON DELETE NO ACTION --may cause cycles or multiple paths
);
