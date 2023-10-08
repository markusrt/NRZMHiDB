-- Backup 20230528_removeOneDuplicatePatients

UPDATE MeningoSendings SET MeningoPatientId=10590 WHERE MeningoPatientId = 10594 
DELETE FROM MeningoPatients WHERE PatientId = 10594 