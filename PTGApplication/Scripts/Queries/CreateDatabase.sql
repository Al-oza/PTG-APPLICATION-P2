USE [master];

GO
DROP DATABASE IF EXISTS [UzimaRx];
CREATE DATABASE [UzimaRx];

GO
USE [UzimaRx];

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
	[IsActive]			   BIT			  NOT NULL,
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

CREATE TABLE [dbo].[UzimaDrug] (
    [Id]   INT   NOT NULL,
    [Barcode]  NVARCHAR (MAX) NULL,
    [DrugName]  NVARCHAR (MAX)  NOT NULL,
	[BrandName]   NVARCHAR (MAX)  NULL,
	[ApplicationNumber] NVARCHAR (MAX) NULL,
	[Manufacturer] NVARCHAR(MAX)   NOT NULL,
	[ManufacturerLocation] NVARCHAR(MAX) NULL,
	[ApprovalNumber] NVARChar (MAX) NULL,
	[Schedule]   NVARCHAR(MAX)  NULL,
    [License]  NVARCHAR (MAX) NULL,
	[Ingredients] NVARCHAR (MAX) NULL,
    [PackSize]  NVARCHAR(MAX)  NULL,
    CONSTRAINT [PK_dbo.UzimaDrug] PRIMARY KEY CLUSTERED ([Id] ASC),
);
GO

CREATE TABLE [dbo].[UzimaLocation] (
    [Id]   INT   NOT NULL,
    [LocationName]  NVARCHAR (MAX)  NOT NULL,
    [Address]  NVARCHAR (MAX)  NOT NULL,
    [Phone]  NVARCHAR (MAX)  NOT NULL,
    CONSTRAINT [PK_dbo.UzimaLocation] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[UzimaLocationType] (
    [Id]   INT   NOT NULL,
    [LocationId] INT NOT NULL,
	[LocationType] NVARCHAR(MAX),
	[Supplier] INT NULL,
    CONSTRAINT [PK_dbo.UzimaLocationType] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.UzimaLocationType] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[UzimaLocation] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.UzimaLocationType_Supplier] FOREIGN KEY ([Supplier]) REFERENCES [dbo].[UzimaLocation] ([Id]) 
);
GO

CREATE TABLE [dbo].[UzimaStatus] (
    [Id]  INT  NOT NULL,
    [Status]  NVARCHAR (MAX)  NOT NULL,
    CONSTRAINT [PK_dbo.UzimaStatus] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [dbo].[UzimaInventory] (
    [Id]  INT   NOT NULL,
    [DateOrdered] DATETIME NOT NULL,
    [LastModifiedBy]  NVARCHAR(128) NOT NULL,
    [DrugId]  INT  NOT NULL,
    [StatusId]  INT  NOT NULL,
    [CurrentLocationId] INT   NOT NULL,
    [FutureLocationId] INT ,
    [ExpirationDate] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.UzimaOrder] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.UzimaInventory_dbo.AspNetUser_UserId] FOREIGN KEY ([LastModifiedBy]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.UzimaOrder_dbo.UzimaDrugBrand_BarcodeId] FOREIGN KEY ([DrugId]) REFERENCES [dbo].[UzimaDrug] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.UzimaOrder_dbo.UzimaStatus_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[UzimaStatus] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.UzimaOrder_dbo.UzimaLocation_CurrentLocationId] FOREIGN KEY ([CurrentLocationId]) REFERENCES [dbo].[UzimaLocation] ([Id]) ON DELETE NO ACTION, --may cause cycles or multiple paths
    CONSTRAINT [FK_dbo.UzimaOrder_dbo.UzimaLocation_FutureLocationId] FOREIGN KEY ([FutureLocationId]) REFERENCES [dbo].[UzimaLocation] ([Id]) ON DELETE NO ACTION --may cause cycles or multiple paths
);
