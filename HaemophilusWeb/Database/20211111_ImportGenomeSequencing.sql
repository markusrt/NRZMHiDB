CREATE TABLE [dbo].[GenomeImport] (
    [StemNumber] INT        NOT NULL,
    [ST]         NCHAR (32) NOT NULL,
    PRIMARY KEY CLUSTERED ([StemNumber] ASC)
);

-- Copy excel data

UPDATE Isolates SET Isolates.Mlst = ISNUMERIC(G.ST), MlstSequenceType =  RTRIM(G.ST)
FROM Isolates I INNER JOIN GenomeImport G
ON I.StemNumber = G.StemNumber
WHERE ISNUMERIC(G.ST)=1

UPDATE Isolates SET Isolates.GenomeSequencing = 1
WHERE Isolates.StemNumber IN (SELECT StemNumber FROM GenomeImport)