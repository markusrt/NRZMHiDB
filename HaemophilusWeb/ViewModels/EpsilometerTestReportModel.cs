using System;
using System.Globalization;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.ViewModels
{
    public class EpsilometerTestReportModel
    {
        public string Antibiotic { get; set; }
        public string Result { get; set; }
        public string MicBreakpointSusceptible { get; set; }
        public string MicBreakpointResistent { get; set; }
        public string Measurement { get; set; }
        public string ValidFromYear { get; set; }

        public static EpsilometerTestReportModel CreateFromViewModel(
            EpsilometerTestViewModel epsilometerTestViewModel)
        {
            var reportModel = new EpsilometerTestReportModel
            {
                Antibiotic = EnumEditor.GetEnumDescription(epsilometerTestViewModel.Antibiotic),
                Measurement = FloatToString(epsilometerTestViewModel.Measurement),
                Result = EnumEditor.GetEnumDescription(epsilometerTestViewModel.Result),
                MicBreakpointResistent = FloatToString(epsilometerTestViewModel.MicBreakpointResistent),
                MicBreakpointSusceptible = FloatToString(epsilometerTestViewModel.MicBreakpointSusceptible),
                ValidFromYear = epsilometerTestViewModel.ValidFromYear.ToString(),
            };
            if (epsilometerTestViewModel.Result == EpsilometerTestResult.NotDetermined)
            {
                reportModel.MicBreakpointResistent = "---";
                reportModel.MicBreakpointSusceptible = "---";
                reportModel.ValidFromYear = "---";
            }
            return reportModel;
        }

        private static string FloatToString(float? number)
        {
            if (!number.HasValue)
            {
                return string.Empty;
            }
            return Math.Round(number.Value, 3).ToString(CultureInfo.CurrentCulture);
        }
    }
}