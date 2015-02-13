using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Tools
{
    /// <summary>
    /// Imports DeStatis data format of "GV100AD Quartalsausgabe"
    /// https://www.destatis.de/DE/ZahlenFakten/LaenderRegionen/Regionales/Gemeindeverzeichnis/Administrativ/Archiv/GV100ADQ/GV100AD4QAktuell.html
    /// </summary>
    public class CountyImporter
    {
        private const string RecordTypeCounty = "40";
        private readonly string filename;

        private readonly List<CountyRecordImport> importRecords = new List<CountyRecordImport>
        {
            new CountyRecordImport
            {
                StartIndex = 22,
                Length = 50,
                AssignValue = (county, value) => county.Name = ExtractCountyName(value)
            },
            new CountyRecordImport
            {
                StartIndex = 2,
                Length = 8,
                AssignValue =
                    (county, value) =>
                        county.ValidSince = DateTime.ParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture)
            },
            new CountyRecordImport
            {
                StartIndex = 10,
                Length = 5,
                AssignValue = (county, value) => county.CountyNumber = value
            }
        };

        private static string ExtractCountyName(string value)
        {
            var index = value.IndexOf(',');
            return index > 0 ? value.Substring(0, index) : value;
        }

        public CountyImporter(string filename)
        {
            this.filename = filename;
        }

        public IEnumerable<County> LoadCounties()
        {
            return File.ReadAllLines(filename, Encoding.GetEncoding(1252)).
                Where(line => line.StartsWith(RecordTypeCounty)).
                Select(DataRecordToCounty);
        }

        private County DataRecordToCounty(string line)
        {
            var county = new County();
            foreach (var record in importRecords)
            {
                var value = line.Substring(record.StartIndex, record.Length).Trim();
                record.AssignValue(county, value);
            }
            return county;
        }

        private class CountyRecordImport
        {
            public int StartIndex { get; set; }

            public int Length { get; set; }

            public Action<County, string> AssignValue { get; set; }
        }
    }
}