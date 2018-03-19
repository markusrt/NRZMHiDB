using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using ServiceStack.Text;

namespace HaemophilusWeb.Tools
{
    /// <summary>
    /// Imports records of RKI matching table
    /// >InterneRef (<see cref="RkiMatchRecord.RkiReferenceId"/>), Aktenzeichen (<see cref="RkiMatchRecord.RkiReferenceNumber"/>), StatusName (<see cref="RkiMatchRecord.RkiStatus"/>), klhi_nr (<see cref="Sending.SendingId"/>)
    /// </summary>
    public class RkiMatchImporter
    {
        private static readonly DateTime MinImportDate = new DateTime(2009,1,1);
        private readonly string filename;
        private readonly IApplicationDbContext context;

        public RkiMatchImporter(string filename, IApplicationDbContext context)
        {
            this.filename = filename;
            this.context = context;
        }

        public void ImportMatches()
        {
            context.WrapInTransaction(ImportMatchesToContext);
        }

        private void ImportMatchesToContext()
        {
            using (var fileStream = new FileStream(filename, FileMode.Open))
            {
                CsvSerializer.UseEncoding = Encoding.UTF8;
                CsvConfig.ItemSeperatorString = ";";
                var rkiMatches = CsvSerializer.DeserializeFromStream<List<RkiMatchImportRecord>>(fileStream);
                foreach (var rkiMatch in rkiMatches.Where(r => r.klhi_nr.HasValue))
                {
                    var dbMatch = context.Sendings.SingleOrDefault(i => i.Isolate.StemNumber==rkiMatch.klhi_nr);
                    if (dbMatch == null && rkiMatch.Jahr.HasValue && rkiMatch.LaufendeNummer.HasValue)
                    {
                        dbMatch = context.Sendings.SingleOrDefault(s =>
                            s.Isolate.YearlySequentialIsolateNumber == rkiMatch.LaufendeNummer &&
                            s.Isolate.Year == rkiMatch.Jahr);
                    }
                    if (dbMatch != null && dbMatch.ReceivingDate >= MinImportDate && rkiMatch.InterneRef.HasValue)
                    {
                        if (dbMatch.RkiMatchRecord == null)
                        {
                            dbMatch.RkiMatchRecord = new RkiMatchRecord {RkiStatus = (RkiStatus) (-1)};
                        }
                        var status = EnumSerializer.DeserializeEnumStrict<RkiStatus>(rkiMatch.StatusName);
                        if (dbMatch.RkiMatchRecord.RkiStatus <= status)
                        {
                            dbMatch.RkiMatchRecord.RkiReferenceId = rkiMatch.InterneRef.Value;
                            dbMatch.RkiMatchRecord.RkiStatus = status;
                            dbMatch.RkiMatchRecord.RkiReferenceNumber = rkiMatch.Aktenzeichen;
                        }
                    }
                }
            }
        }

        private class RkiMatchImportRecord
        {
            public int? InterneRef { get; set; }
            public string Aktenzeichen { get; set; }
            public string StatusName { get; set; }
            public int? klhi_nr { get; set; }
            public int? LaufendeNummer { get; set; }
            public int? Jahr { get; set; }
            }
    }
}