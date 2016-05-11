using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class AntibioticPriorityListComparerTest
    {
        private AntibioticPriorityListComparer comparer;

        [SetUp]
        public void SetUp()
        {
            comparer = new AntibioticPriorityListComparer("Dalbavancin, Cefixime, Narasin");    
        }

        [Test]
        public void CompareTo_AntibioticsNotInPriorityList_ComparesDescription()
        {
            var result = comparer.Compare(Antibiotic.OritavancinWithP80, Antibiotic.OritavancinWithoutP80);

            result.Should().BeLessThan(0);
        }

        [Test]
        public void CompareTo_BothAntibioticsInPriorityList_ComparesListIndex()
        {
            var result = comparer.Compare(Antibiotic.Dalbavancin, Antibiotic.Cefixime);

            result.Should().BeLessThan(0);
        }

        [Test]
        public void CompareTo_FirstAntibioticInPriorityList_PrioritizesFirst()
        {
            var result = comparer.Compare(Antibiotic.Narasin, Antibiotic.Amikacin);

            result.Should().BeLessThan(0);
        }

        [Test]
        public void CompareTo_SecondAntibioticInPriorityList_PrioritizesSecond()
        {
            var result = comparer.Compare(Antibiotic.Amikacin, Antibiotic.Narasin);

            result.Should().BeGreaterThan(0);
        }
    }
}