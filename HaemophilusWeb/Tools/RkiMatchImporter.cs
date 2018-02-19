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
                foreach (var rkiMatch in rkiMatches)
                {
                    if (!rkiMatch.klhi_nr.HasValue)
                    {
                        continue;
                    }
                    var dbMatch = context.Sendings.SingleOrDefault(i => i.SendingId.Equals(rkiMatch.klhi_nr.Value));
                    if (dbMatch != null && dbMatch.SamplingDate >= MinImportDate)
                    {
                        dbMatch.RkiMatchRecord = new RkiMatchRecord
                        {
                            RkiReferenceId = rkiMatch.InterneRef,
                            RkiStatus = EnumSerializer.DeserializeEnumStrict<RkiStatus>(rkiMatch.StatusName),
                            RkiReferenceNumber = rkiMatch.Aktenzeichen
                        };
                    }
                }
            }
        }

        private class RkiMatchImportRecord
        {
            public int InterneRef { get; set; }
            public string Aktenzeichen { get; set; }
            public string StatusName { get; set; }
            public int? klhi_nr { get; set; }

        }
    }
}