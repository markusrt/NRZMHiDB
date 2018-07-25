using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Xml.XPath;
using HaemophilusWeb.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace HaemophilusWeb.Tools
{
    public class RkiTool
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string Uri = "https://tools.rki.de/PLZTool/?q={0}";
        private readonly Func<string, string> queryPostalCode;
        private readonly Regex DuplicateInnerSpaces = new Regex(@"\s\s+");

        public RkiTool() : this(QueryUsingWebClient)
        {
        }

        private static string QueryUsingWebClient(string postalCode)
        {
            using (var client = new WebClient())
            using (var result = client.OpenRead(new Uri(string.Format(Uri, postalCode))))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public RkiTool(Func<string, string> queryPostalCode )
        {
            this.queryPostalCode = queryPostalCode;
        }

        public HealthOffice QueryHealthOffice(string postalCode)
        {
            try
            {
                var result = queryPostalCode(postalCode);
                var document = new HtmlDocument();
                document.LoadHtml(result);

                var results = document.DocumentNode.SelectNodes("//div[@id='result']/select/option");
                if (results == null || results.Count < 1)
                {
                    return null;
                }
                var dataNodes = document.DocumentNode.
                    SelectNodes("//input[@type='text'] | //address");
                
                var address = Regex.Replace(dataNodes[0].InnerText.Trim(), "\r\n[ \t]*", "\n");
                address = WebUtility.HtmlDecode(address);
                return new HealthOffice
                {
                    Address = address,
                    Phone = DuplicateInnerSpaces.Replace(dataNodes[1].GetAttributeValue("Value", string.Empty), " ").Trim(),
                    Fax = DuplicateInnerSpaces.Replace(dataNodes[2].GetAttributeValue("Value", string.Empty)," ").Trim(),
                    Email = dataNodes[3].GetAttributeValue("Value", string.Empty),
                    PostalCode = postalCode
                };
            }
            catch (Exception e)
            {
                Log.Warn(e, "Failed to query RKI Tool");
                return null;
            }
        }
    }
}