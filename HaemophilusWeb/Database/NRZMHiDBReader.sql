CREATE LOGIN NRZMHiDBReader WITH PASSWORD = '******';

USE HaemophilusWeb
CREATE USER NRZMHiDBReader FOR LOGIN NRZMHiDBReader
EXEC sp_addrolemember 'db_datareader', 'NRZMHiDBReader'  