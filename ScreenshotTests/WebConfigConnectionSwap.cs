using System.IO;
using System.Xml.Linq;

namespace ScreenshotTests
{
    /// <summary>
    /// Temporarily repoints the hosted app's DefaultConnection at the isolated screenshot catalog while
    /// the harness runs, then restores the original Web.config. Crash-safe: a leftover backup from a killed
    /// run is restored before the next swap, and the backup is git-ignored.
    /// </summary>
    internal sealed class WebConfigConnectionSwap
    {
        private static string BackupPath => Paths.WebConfig + ".screenshotbak";

        public void Apply()
        {
            // Recover from a previous interrupted run before touching anything.
            Restore();

            File.Copy(Paths.WebConfig, BackupPath, overwrite: false);

            var doc = XDocument.Load(Paths.WebConfig);
            var connectionStringsElement = doc.Root?.Element("connectionStrings");
            if (connectionStringsElement == null)
            {
                connectionStringsElement = new XElement("connectionStrings");
                doc.Root!.AddFirst(connectionStringsElement);
            }

            var add = FindDefaultConnection(connectionStringsElement);
            if (add == null)
            {
                add = new XElement("add",
                    new XAttribute("name", "DefaultConnection"),
                    new XAttribute("providerName", "System.Data.SqlClient"));
                connectionStringsElement.Add(add);
            }

            add.SetAttributeValue("connectionString", Paths.ConnectionString);
            add.SetAttributeValue("providerName", "System.Data.SqlClient");
            doc.Save(Paths.WebConfig);
        }

        public void Restore()
        {
            if (File.Exists(BackupPath))
            {
                File.Copy(BackupPath, Paths.WebConfig, overwrite: true);
                File.Delete(BackupPath);
            }
        }

        private static XElement FindDefaultConnection(XElement connectionStrings)
        {
            foreach (var element in connectionStrings.Elements("add"))
            {
                if ((string)element.Attribute("name") == "DefaultConnection")
                {
                    return element;
                }
            }

            return null;
        }
    }
}
