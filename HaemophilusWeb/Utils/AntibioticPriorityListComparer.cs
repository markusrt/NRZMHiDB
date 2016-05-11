using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Utils
{
    public class AntibioticPriorityListComparer : IComparer<Antibiotic>
    {
        private readonly List<Antibiotic> antibioticsOrder;

        public AntibioticPriorityListComparer(string antibioticsOrder)
            : this(EnumUtils.ParseCommaSeperatedListOfNames<Antibiotic>(antibioticsOrder))
        {
        }

        private AntibioticPriorityListComparer(List<Antibiotic> antibioticsOrder)
        {
            this.antibioticsOrder = antibioticsOrder;
        }

        public int Compare(Antibiotic a, Antibiotic b)
        {
            if (antibioticsOrder.Contains(a) && antibioticsOrder.Contains(b))
            {
                return antibioticsOrder.IndexOf(a).CompareTo(antibioticsOrder.IndexOf(b));
            }
            if (antibioticsOrder.Contains(a))
            {
                return -1;
            }
            if (antibioticsOrder.Contains(b))
            {
                return 1;
            }
            return string.Compare(EnumUtils.GetEnumDescription<Antibiotic>(a),
                EnumUtils.GetEnumDescription<Antibiotic>(b), StringComparison.InvariantCulture);
        }
    }
}