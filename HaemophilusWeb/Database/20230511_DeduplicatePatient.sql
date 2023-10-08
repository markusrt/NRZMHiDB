-- Backup 20230511_removeThreeDuplicatePatients

UPDATE MeningoSendings SET MeningoPatientId=10286 WHERE MeningoPatientId = 10287 
DELETE FROM MeningoPatients WHERE PatientId = 10287 

UPDATE MeningoSendings SET MeningoPatientId=10338 WHERE MeningoPatientId = 10331  
DELETE FROM MeningoPatients WHERE PatientId = 10331 

UPDATE MeningoSendings SET MeningoPatientId=10339 WHERE MeningoPatientId = 10340   
DELETE FROM MeningoPatients WHERE PatientId = 10340 