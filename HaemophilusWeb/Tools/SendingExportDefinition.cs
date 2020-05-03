using System;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Tools
{
    public class SendingExportDefinition<TSending, TPatient> : ExportDefinition<TSending>
        where TPatient : PatientBase, new()
        where TSending : SendingBase<TPatient>, new()
    {
        protected string ExportClinicalInformation<T>(T clinicalInformation, Func<string> otherClinicalInformation, Func<T, bool> isOther)
        {
            var clinicalInformationAsString = EnumEditor.GetEnumDescription(clinicalInformation);
            if (isOther(clinicalInformation))
            {
                clinicalInformationAsString = clinicalInformationAsString.Replace(
                    EnumEditor.GetEnumDescription(ClinicalInformation.Other), otherClinicalInformation());
            }
            return clinicalInformationAsString;
        }

        protected void AddEpsilometerTestFields(ExportDefinition<TSending> export, Antibiotic antibiotic, string measurementHeaderParameter = null, string evaluationHeaderParameter = null)
        {
            var antibioticName = ExportToString(antibiotic);
            var measurementHeader = measurementHeaderParameter ?? string.Format("{0} MHK", antibioticName);
            var evaluationHeader = evaluationHeaderParameter ?? string.Format("{0} Bewertung", antibioticName);

            export.AddField(s => FindEpsilometerTestMeasurement(s, antibiotic), measurementHeader);
            export.AddField(s => ExportToString(FindEpsilometerTestEvaluation(s, antibiotic)), evaluationHeader);
        }

        protected static string ExportGender(Gender? gender)
        {
            return gender == null
                ? "?"
                : EnumEditor.GetEnumDescription(gender).Substring(0, 1);
        }


        private double? FindEpsilometerTestMeasurement(TSending sending, Antibiotic antibiotic)
        {
            var eTestResult = FindEpsilometerTestResult(sending, antibiotic);
            if (eTestResult == null)
            {
                return null;
            }
            return Math.Round(eTestResult.Measurement, 3);
        }

        private EpsilometerTestResult? FindEpsilometerTestEvaluation(TSending sending, Antibiotic antibiotic)
        {
            var eTestResult = FindEpsilometerTestResult(sending, antibiotic);
            if (eTestResult == null)
            {
                return null;
            }
            return eTestResult.Result;
        }

        private EpsilometerTest FindEpsilometerTestResult(TSending sending, Antibiotic antibiotic)
        {
            var eTestResult = sending.GetIsolate().EpsilometerTests.SingleOrDefault(
                e => e.EucastClinicalBreakpoint.Antibiotic == antibiotic);
            return eTestResult;
        }
    }
}