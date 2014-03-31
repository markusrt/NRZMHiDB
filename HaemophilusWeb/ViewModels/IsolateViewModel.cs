using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class IsolateViewModel
    {
        internal const string PenicillinAmpicillin = "Ampicillin";
        internal const string PenicillinAmoxicillinClavulanate = "AmoxicillinClavulanate";
        internal const string PenicillinCefotaxime = "Cefotaxime";
        internal const string PenicillinMeropenem = "Meropenem";

        public Isolate TheIsolate { get; set; }

        public List<EucastClinicalBreakpoint> CurrentClinicalBreakpoints { get; set; }

        public EpsilometerTest Ampicillin { get; set; }

        [Display(Name = "Amoxicillin/Clavulansäure")]
        public EpsilometerTest AmoxicillinClavulanate { get; set; }

        [Display(Name = "Cefotaxim")]
        public EpsilometerTest Cefotaxime
        {
            get { return GetEpsilometerTest(PenicillinCefotaxime); }
        }

        public EpsilometerTest Meropenem
        {
            get { return GetEpsilometerTest(PenicillinMeropenem); }
        }

        private EpsilometerTest GetEpsilometerTest(string penicillin)
        {
            var existingTest = FindTestWithPenicillin(penicillin);

            if (existingTest == null)
            {
                AddNewTestForPenicillin(penicillin);
            }
            return FindTestWithPenicillin(penicillin);
        }

        private EpsilometerTest FindTestWithPenicillin(string penicillin)
        {
            return
                TheIsolate.EpsilometerTests.SingleOrDefault(e => e.EucastClinicalBreakpoint.Penicillin.Equals(penicillin));
        }

        private void AddNewTestForPenicillin(string penicillin)
        {
            var clinicalBreakPoint =
                CurrentClinicalBreakpoints.SingleOrDefault(e => e.Penicillin.Equals(penicillin));
            TheIsolate.EpsilometerTests.Add(new EpsilometerTest
            {
                EucastClinicalBreakpoint = clinicalBreakPoint
            });
        }
    }
}