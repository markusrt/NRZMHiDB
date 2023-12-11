-- Backup 20231211_removeDuplicatePatient

UPDATE MeningoSendings SET MeningoPatientId=10765 WHERE MeningoPatientId = 10766  
DELETE FROM MeningoPatients WHERE PatientId = 10766 