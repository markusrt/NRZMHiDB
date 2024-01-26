-- Backup 20240126_removeDuplicatePatient
UPDATE MeningoSendings SET MeningoPatientId=10691 WHERE MeningoPatientId = 10684  
DELETE FROM MeningoPatients WHERE PatientId = 10684 