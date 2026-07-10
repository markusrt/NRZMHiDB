using System.Collections.Generic;

namespace ScreenshotTests
{
    /// <summary>A single navigable view to screenshot.</summary>
    public sealed class Route
    {
        public Route(string name, string url, bool anonymous = false, bool mobile = false)
        {
            Name = name;
            Url = url;
            Anonymous = anonymous;
            Mobile = mobile;
        }

        /// <summary>Filesystem-safe identifier used as the screenshot file name.</summary>
        public string Name { get; }

        /// <summary>App-relative URL (starts with '/').</summary>
        public string Url { get; }

        /// <summary>True for pages captured without a logged-in session (e.g. the login page).</summary>
        public bool Anonymous { get; }

        /// <summary>True to also capture a narrow (mobile) viewport for this route.</summary>
        public bool Mobile { get; }

        public override string ToString() => Name;
    }

    /// <summary>
    /// The gold-master coverage set: as many distinct views as render deterministically from the seeded
    /// data. id-based routes rely on the seeded records at id = 1.
    /// </summary>
    internal static class Routes
    {
        public static IReadOnlyList<Route> All { get; } = new List<Route>
        {
            // Home + account
            new Route("Home_Index", "/", mobile: true),
            new Route("Home_About", "/Home/About"),
            new Route("Home_Change", "/Home/Change"),
            new Route("Account_Login", "/Account/Login", anonymous: true, mobile: true),
            new Route("Account_Manage", "/Account/Manage"),

            // Sender
            new Route("Sender_Index", "/Sender/Index", mobile: true),
            new Route("Sender_Create", "/Sender/Create", mobile: true),
            new Route("Sender_Details", "/Sender/Details/1"),
            new Route("Sender_Edit", "/Sender/Edit/1"),
            new Route("Sender_Export", "/Sender/Export"),

            // Patient (Haemophilus)
            new Route("Patient_Index", "/Patient/Index"),
            new Route("Patient_Create", "/Patient/Create", mobile: true),
            new Route("Patient_Details", "/Patient/Details/1"),
            new Route("Patient_Edit", "/Patient/Edit/1"),

            // Sending (Haemophilus)
            new Route("Sending_Index", "/Sending/Index"),
            new Route("Sending_Create", "/Sending/Create"),
            new Route("Sending_Details", "/Sending/Details/1"),
            new Route("Sending_Edit", "/Sending/Edit/1"),

            // Isolate + report (Haemophilus)
            new Route("Isolate_Edit", "/Isolate/Edit/1"),
            new Route("Report_Isolate", "/Report/Isolate/1"),

            // PatientSending (Haemophilus) + exports
            new Route("PatientSending_Index", "/PatientSending/Index"),
            new Route("PatientSending_Create", "/PatientSending/Create"),
            new Route("PatientSending_Edit", "/PatientSending/Edit/1"),
            new Route("PatientSending_RkiExport", "/PatientSending/RkiExport"),
            new Route("PatientSending_LaboratoryExport", "/PatientSending/LaboratoryExport"),
            new Route("PatientSending_PubMlstExport", "/PatientSending/PubMlstExport"),

            // Meningococci mirrors
            new Route("MeningoPatient_Index", "/MeningoPatient/Index"),
            new Route("MeningoPatient_Create", "/MeningoPatient/Create"),
            new Route("MeningoPatient_Edit", "/MeningoPatient/Edit/1"),
            new Route("MeningoSending_Index", "/MeningoSending/Index"),
            new Route("MeningoSending_Create", "/MeningoSending/Create"),
            new Route("MeningoSending_Edit", "/MeningoSending/Edit/1"),
            new Route("MeningoPatientSending_Index", "/MeningoPatientSending/Index"),
            new Route("MeningoPatientSending_Create", "/MeningoPatientSending/Create"),
            new Route("MeningoPatientSending_Edit", "/MeningoPatientSending/Edit/1"),
            new Route("MeningoIsolate_Edit", "/MeningoIsolate/Edit/1"),
            new Route("MeningoSender_Export", "/MeningoSender/Export"),
            new Route("MeningoReport_Isolate", "/MeningoReport/Isolate/1"),

            // Admin / lookup
            new Route("County_Index", "/County/Index"),
            new Route("HealthOffices_Index", "/HealthOffices/Index"),
            new Route("EucastClinicalBreakpoints_Index", "/EucastClinicalBreakpoints/Index"),
            new Route("Role_Index", "/Role/Index"),
            new Route("Configuration_Edit", "/Configuration/Edit"),
            new Route("RkiMatch_Index", "/RkiMatch/Index"),
        };
    }
}
