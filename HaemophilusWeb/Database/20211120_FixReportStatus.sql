UPDATE MeningoIsolates SET ReportStatus = 2 Where ReportDate IS NOT NULL and ReportStatus = 0
-- -> 33 rows updated on 2021-11-20 13:50 UTC
UPDATE Isolates SET ReportStatus = 2 Where ReportDate IS NOT NULL and ReportStatus = 0 AND ReportDate < '2021-01-03 00:00:00.000' 
-- +2 additional ones Isolate Ids 6846 and 6832
-- -> 157 rows updated on 2021-11-20 13:50 UTC