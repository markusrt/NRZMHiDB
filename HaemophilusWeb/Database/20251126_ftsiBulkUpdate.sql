-- Temp Table
CREATE TABLE [dbo].[TempFtsi3](
	[Stemnumber] [int] NULL,
	[FtsiEvaluation3] [nchar](10) NULL
) ON [PRIMARY]
GO

-- Update 1
UPDATE
    Isolates
SET
    Isolates.Ftsi = 1    
FROM
    Isolates SI
WHERE SI.StemNumber IN (SELECT StemNumber FROM TempFtsi3)

-- Update 2
UPDATE
    Isolates
SET
    Isolates.FtsiEvaluation3 = REPLACE(RAN.FtsiEvaluation3, 'ftsI_','')    
FROM
    Isolates IS
INNER JOIN
    TempFtsi3 TEMP
ON 
    IS.StemNumber = TEMP.StemNumber;

-- Export
SELECT
  CONCAT('H', StemNumber),
  YEAR(ReceivingDate) AS Jahr,
  (CASE
	WHEN BetaLactamase=0 THEN 'n.d.'
	WHEN BetaLactamase=1 THEN 'negativ'
	WHEN BetaLactamase=2 THEN 'positiv'
	ELSE 'XXX'
  END) AS 'Beta Lactamase',
  (CASE
	WHEN SerotypePcr=0 THEN 'n.d.'
	WHEN SerotypePcr=1 THEN 'a'
	WHEN SerotypePcr=2 THEN 'b'
	WHEN SerotypePcr=3 THEN 'c'
	WHEN SerotypePcr=4 THEN 'd'
	WHEN SerotypePcr=5 THEN 'e'
	WHEN SerotypePcr=6 THEN 'f'
	WHEN SerotypePcr=7 THEN 'negativ'
	ELSE 'XXX'
  END) AS 'Serotyp-PCR',
  (CASE
	WHEN Evaluation=0 THEN 'NTHi'
	WHEN Evaluation=1 THEN 'Hia'
	WHEN Evaluation=2 THEN 'Hib'
	WHEN Evaluation=3 THEN 'Hic'
	WHEN Evaluation=4 THEN 'Hid'
	WHEN Evaluation=5 THEN 'Hie'
	WHEN Evaluation=6 THEN 'Hif'
	WHEN Evaluation=7 THEN 'H. haemolyticus'
	WHEN Evaluation=8 THEN 'H. parainfluenzae'
	WHEN Evaluation=9 THEN 'kein Wachstum'
	WHEN Evaluation=10 THEN 'keine Haemophilus-Spezies'
	WHEN Evaluation=11 THEN 'H. influenzae'
	WHEN Evaluation=12 THEN 'Haemophilus sp., nicht H. influenzae'
	WHEN Evaluation=13 THEN 'kein H. influenzae'
	ELSE 'XXX'
  END) AS Beurteilung, Measurement, TRIM(REPLACE(REPLACE(AntibioticDetails, 'Amoxicillin-clavulanate', 'Amoxicillin / Clavulansäure'), 'Cefotaxime', 'Cefotaxim')), Ftsi, FtsiEvaluation3
  FROM Isolates i 
	LEFT JOIN EpsilometerTests e ON i.SendingId=e.Isolate_SendingId LEFT JOIN EucastClinicalBreakpoints b ON e.EucastClinicalBreakpointId=b.EucastClinicalBreakpointId
	LEFT JOIN Sendings s ON s.SendingId = i.SendingId
  WHERE StemNumber IN (SELECT StemNumber FROM TempFtsi3) AND Antibiotic IN (2,1,0) --AND FtsiEvaluation3 IS NOT NULL
