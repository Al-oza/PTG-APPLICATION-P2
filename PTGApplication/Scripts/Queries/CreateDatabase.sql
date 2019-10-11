USE [master];

GO
DROP DATABASE IF EXISTS [UzimaRx];
CREATE DATABASE [UzimaRx];

GO
USE UzimaRx;

GO
CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [HomePharmacy]         NVARCHAR (MAX) NULL,
    [Username]             NVARCHAR (MAX) NULL,
	[Name]				   NVARCHAR (MAX) NOT NULL,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([Id] ASC);

CREATE TABLE [dbo].[__MigrationHistory] (
    [MigrationId]    NVARCHAR (150)  NOT NULL,
    [ContextKey]     NVARCHAR (300)  NOT NULL,
    [Model]          VARBINARY (MAX) NOT NULL,
    [ProductVersion] NVARCHAR (32)   NOT NULL,
    CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC, [ContextKey] ASC)
);

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([Name] ASC);

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserRoles]([UserId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[AspNetUserRoles]([RoleId] ASC);

GO
CREATE TABLE [dbo].[PharmacyDrug] (
    [Id]   INT   NOT NULL,
    [Barcode]  NVARCHAR (256) NULL,
    [Name]  NVARCHAR (256)  NOT NULL,
	[BrandName]   NVARCHAR (256)  NULL,
	[ApplicationNumber] NVARCHAR (256) NULL,
	[Manufacturer] NVARCHAR(256)   NOT NULL,
	[ManufacturerLocation] NVARCHAR(256) NULL,
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
    CONSTRAINT [FK_dbo.PharmacyBatch_dbo.PharmacyDrugBrand_DrugBrandId] FOREIGN KEY ([DrugBrandId]) REFERENCES [dbo].[PharmacyDrug] ([Id]) ON DELETE CASCADE
);
GO
CREATE TABLE [dbo].[PharmacySupplier] (
    [Id]   INT   NOT NULL,
    [Name]  NVARCHAR (256)  NOT NULL,
    [Address]  NVARCHAR (256)  NOT NULL,
    [Phone]  NVARCHAR (256)  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacySupplier] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE TABLE [dbo].[PharmacyLocation] (
    [Id]   INT   NOT NULL,
    [Name]  NVARCHAR (256)  NOT NULL,
    [UpstreamSupplier] INT  NULL,
    [IsHospital] BIT  NOT NULL,
    [IsClinic]  BIT  NOT NULL,
    [Address]  NVARCHAR (256)  NOT NULL,
    [Phone]  NVARCHAR (256)  NOT NULL,
    CONSTRAINT [PK_dbo.PharmacyLocation] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.PharmacyLocation] FOREIGN KEY ([UpstreamSupplier]) REFERENCES [dbo].[PharmacySupplier] ([Id]) ON DELETE CASCADE
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
CREATE TABLE [dbo].[PharmacyInventory] (
    [Id]  INT   NOT NULL,
    [DateOrdered] DATETIME NOT NULL,
    [UserId]  INT   NOT NULL, --use this as user or do the PK or AuthUser Table?
    [BarcodeId]  INT  NOT NULL,
    [StatusId]  INT  NOT NULL,
    [CurrentLocationId] INT   NOT NULL,
    [FutureLocationId] INT ,
    [ExpirationDate] DATETIME NOT NULL,--is this a FK to PharmacyBatch Table?
    CONSTRAINT [PK_dbo.PharmacyOrder] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyDrugBrand_BarcodeId] FOREIGN KEY ([BarcodeId]) REFERENCES [dbo].[PharmacyDrug] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[PharmacyStatus] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyLocation_CurrentLocationId] FOREIGN KEY ([CurrentLocationId]) REFERENCES [dbo].[PharmacyLocation] ([Id]) ON DELETE NO ACTION, --may cause cycles or multiple paths
    CONSTRAINT [FK_dbo.PharmacyOrder_dbo.PharmacyLocation_FutureLocationId] FOREIGN KEY ([FutureLocationId]) REFERENCES [dbo].[PharmacyLocation] ([Id]) ON DELETE NO ACTION --may cause cycles or multiple paths
);




