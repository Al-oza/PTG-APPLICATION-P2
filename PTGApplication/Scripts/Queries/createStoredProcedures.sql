CREATE PROCEDURE sp_Drug
(@Id int)
as 
BEGIN
Delete from UzimaDrug where Id = @Id;
UPDATE UzimaDrug set Id = Id - 1 where Id > @Id 
END
Go
CREATE PROCEDURE sp_Inventory
(@Id int)
as 
BEGIN
Delete from UzimaInventory where Id = @Id;
UPDATE UzimaInventory set Id = Id - 1 where Id > @Id 
END

Go
CREATE PROCEDURE sp_Location
(@Id int)
as 
BEGIN
Delete from UzimaLocation where Id = @Id;
UPDATE UzimaLocation set Id = Id - 1 where Id > @Id 
END

Go
CREATE PROCEDURE sp_LocationType
(@Id int)
as 
BEGIN
Delete from UzimaLocationType where Id = @Id;
UPDATE UzimaLocationType set Id = Id - 1 where Id > @Id 
END


