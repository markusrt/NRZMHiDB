-- Backup 20230731_Haemophilus_Genomsequenz_und_Auto_Invasiv
UPDATE
  [HaemophilusWebTest].[dbo].[Isolates]
  SET GenomeSequencing=1
  WHERE StemNumber IN ( /*StemNumbers from Excel*/ )

-- Stem numbers marked with GenomeSequencing = 1 but not in List
-- H4343
-- H4600
-- H865
-- H633
-- H783
-- H4550
-- H618
-- H887