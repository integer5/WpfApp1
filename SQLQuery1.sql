

IF EXISTS (SELECT ProviderEventID FROM Events WHERE ProviderEventID = 222)

	UPDATE Events SET EventName = 'Team B vs. Team C', EventDate = CONVERT(DATETIME, '2022-10-19T02:35:10') WHERE ProviderEventID = 222

ELSE

	INSERT INTO Events VALUES (222, 'Team B vs. Team D', CONVERT(DATETIME, '2022-10-19T02:30:10'))


SELECT * FROM Events

TRUNCATE TABLE TestTable