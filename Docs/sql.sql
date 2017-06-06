SELECT COUNT(*) FROM EDStations;
SELECT TOP 100 * FROM EDStations;

SELECT COUNT(*) FROM EDSystems; -- 7,505,000
SELECT MIN(X), MIN(Y), MAX(X), MAX(Y) FROM EDSystems;
SELECT TOP 100 * FROM EDSystems WHERE population>0;
SELECT * FROM EDSystems WHERE x is null OR y is null OR z is null;

SELECT * FROM EDsystems WHERE name = '18 Comae Berenices'; -- 8, 17.90625	200.0313	-7

SELECT name AS [Database Name], recovery_model_desc AS [Recovery Model] FROM sys.databases
DBCC SHRINKFILE ('Trade.mdf', TRUNCATEONLY);
DBCC SHRINKFILE ('Trade.mdf', 0);
DBCC SHRINKFILE ('Trade_log.ldf', 0);
DBCC SHRINKFILE ('Trade_log.ldf', TRUNCATEONLY);

SELECT * FROM EDSystems WHERE [Name] LIKE 'Olgrea%' -- DECLARE @X FLOAT = -28.125;DECLARE @Y FLOAT = 68.28125;DECLARE @Z FLOAT = -6.375;


-- precise distance (00:20)
DECLARE @JumpRange FLOAT = 30;
DECLARE @X FLOAT = 0; DECLARE @Y FLOAT = 0; DECLARE @Z FLOAT = 0;
SELECT @X=x, @Y=y, @Z=z FROM EDSystems WHERE [Name] LIKE 'Olgrea%'

SELECT name, x, y, z, ROUND(SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2)), 2) AS Dist
FROM EDsystems
WHERE SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2)) < @JumpRange; --

-- boxing first, then precise
DECLARE @JumpRange FLOAT = 30;
DECLARE @X FLOAT = -28.125;DECLARE @Y FLOAT = 68.28125;DECLARE @Z FLOAT = -6.375;
-- SELECT @X=x, @Y=y, @Z=z FROM EDSystems WHERE [Name] LIKE 'Olgrea%'

SELECT name, x, y, z FROM EDsystems
WHERE (ABS(x - @X) < @JumpRange AND ABS(y - @Y) < @JumpRange AND ABS(z - @Z) < @JumpRange)
-- AND (SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2)) < @JumpRange)



-- spatial
DECLARE @JumpRange FLOAT = 30;
DECLARE @X FLOAT = -28.125;DECLARE @Y FLOAT = 68.28125;DECLARE @Z FLOAT = -6.375;

DECLARE @Area geometry = geometry::Parse('POLYGON((' + CAST(@x - @JumpRange AS nvarchar) + ' ' + CAST(@y - @JumpRange AS nvarchar) + ', ' + CAST(@x - @JumpRange AS nvarchar) + ' ' + CAST(@y + @JumpRange AS nvarchar) + ', ' + CAST(@x + @JumpRange AS nvarchar) + ' ' + CAST(@y - @JumpRange AS nvarchar) + ', ' + CAST(@x + @JumpRange AS nvarchar) + ' ' + CAST(@y + @JumpRange AS nvarchar) + ', ' + CAST(@x - @JumpRange AS nvarchar) + ' ' + CAST(@y - @JumpRange AS nvarchar) + '))');
SELECT  name, x, y, z
FROM EDsystems
WHERE GeomTest.STWithin(@Area.MakeValid())=1
AND z >= @Z - @JumpRange AND z <= @Z + @JumpRange
-- AND (SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2)) < @JumpRange)


-- spatial
DECLARE @JumpRange FLOAT = 30;
DECLARE @X FLOAT = -28.125;DECLARE @Y FLOAT = 68.28125;DECLARE @Z FLOAT = -6.375;
SELECT @X=x, @Y=y, @Z=z FROM EDSystems WHERE [Name] = 'Maia'

DECLARE @ReferencePoint geometry = geometry::Parse('POINT(' + CAST(@x AS nvarchar) + ' ' + CAST(@y AS nvarchar) + ' ' + CAST(@z AS nvarchar) + ')')
SELECT  name, x, y, z, SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2))
FROM EDsystems
WHERE GeomTest.STDistance(@ReferencePoint.MakeValid()) <= @JumpRange
AND (SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2)) < @JumpRange)
ORDER BY SQRT(POWER(x - @X, 2) + POWER(y - @Y, 2) + POWER(z - @Z, 2))





ALTER TABLE EDsystems ADD GeomTest geometry NULL;
SELECT TOP 100 name, x, y, z, GeomTest
UPDATE EDsystems SET GeomTest = geometry::Parse('POINT(' + CAST(x AS nvarchar) + ' ' + CAST(y AS nvarchar) + ')');
CREATE SPATIAL INDEX ixGeomTest ON EDsystems(GeomTest) WITH (BOUNDING_BOX = (xmin=-44000, ymin=-18000, xmax=41000, ymax=5500));
