﻿using System;
using System.Linq;
using DocumentFormat.OpenXml.Bibliography;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
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

        protected void AddEpsilometerTestFields(ExportDefinition<TSending> export, Antibiotic antibiotic, bool skipEvaluation = false, string measurementHeaderParameter = null, string evaluationHeaderParameter = null)
        {
            var antibioticName = ExportToString(antibiotic);
            var measurementHeader = measurementHeaderParameter ?? string.Format("{0} MHK", antibioticName);
            var evaluationHeader = evaluationHeaderParameter ?? string.Format("{0} Bewertung", antibioticName);

            export.AddField(s => FindEpsilometerTestMeasurement(s, antibiotic), measurementHeader);
            if(!skipEvaluation) export.AddField(s => ExportToString(FindEpsilometerTestEvaluation(s, antibiotic)), evaluationHeader);
        }

        //protected void AddPorAAndFetAFields(ExportDefinition<MeningoSending> export, string porAVr1Header = null, string evaluationHeaderParameter = null)
        //{
        //    export.AddField(s => ExportToString(s.Isolate.PorAVr1));
        //    AddField(s => s.GetIsolate().FetAVr == ExportToString(s.Isolate.PorAVr2));
        //    AddField(s => ExportToString(s.Isolate.FetAVr));
        //    var antibioticName = ExportToString(antibiotic);
        //    var measurementHeader = measurementHeaderParameter ?? string.Format("{0} MHK", antibioticName);
        //    var evaluationHeader = evaluationHeaderParameter ?? string.Format("{0} Bewertung", antibioticName);

        //    export.AddField(s => FindEpsilometerTestMeasurement(s, antibiotic), measurementHeader);
        //    if(!skipEvaluation) export.AddField(s => ExportToString(FindEpsilometerTestEvaluation(s, antibiotic)), evaluationHeader);
        //}


        protected static string ExportGender(Gender? gender)
        {
            return gender == null
                ? "?"
                : EnumEditor.GetEnumDescription(gender).Substring(0, 1);
        }


        protected double? FindEpsilometerTestMeasurement(TSending sending, Antibiotic antibiotic)
        {
            var eTestResult = FindEpsilometerTestResult(sending, antibiotic);
            if (eTestResult == null)
            {
                return null;
            }
            return Math.Round(eTestResult.Measurement, 3);
        }

        protected EpsilometerTestResult? FindEpsilometerTestEvaluation(TSending sending, Antibiotic antibiotic)
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
            var eTestResult = sending.GetIsolate().EpsilometerTests
                .Where(e => e.EucastClinicalBreakpoint.Antibiotic == antibiotic)
                .OrderByDescending(e => e.Measurement)
                .FirstOrDefault();
            return eTestResult;
        }
    }
}