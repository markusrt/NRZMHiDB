-- Backup 20240519_removeThreeDuplicatePatients
UPDATE MeningoSendings SET MeningoPatientId=10924 WHERE MeningoPatientId = 10939  
DELETE FROM MeningoPatients WHERE PatientId = 10939 

UPDATE MeningoSendings SET MeningoPatientId=10916 WHERE MeningoPatientId = 10923  
DELETE FROM MeningoPatients WHERE PatientId = 10923 

UPDATE MeningoSendings SET MeningoPatientId=10079 WHERE MeningoPatientId = 10080  
DELETE FROM MeningoPatients WHERE PatientId = 10080 
 