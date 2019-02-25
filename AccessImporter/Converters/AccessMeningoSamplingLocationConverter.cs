using System;
using System.Collections.Generic;
using AutoMapper;
using HaemophilusWeb.Models.Meningo;

internal class AccessMeningoSamplingLocationConverter : ITypeConverter<object, MeningoSamplingLocation>
{
    private static readonly Dictionary<object, MeningoSamplingLocation> LegacyIdToMeningoSamplingLocationMap = new Dictionary<object, MeningoSamplingLocation>
    {
        {0,MeningoSamplingLocation.OtherInvasive },
        {1,MeningoSamplingLocation.ConjunctivalSwab },
        {2,MeningoSamplingLocation.Blood },
        {3,MeningoSamplingLocation.BronchoalveolarLavage },
        {4,MeningoSamplingLocation.CervixSwab },
        {5,MeningoSamplingLocation.JointAspiration },
        {6,MeningoSamplingLocation.Liquor },
        {7,MeningoSamplingLocation.BloodAndLiquor },
        {8,MeningoSamplingLocation.NasalSwab },
        {9,MeningoSamplingLocation.EarSwab },
        {10,MeningoSamplingLocation.Petechien },
        {11,MeningoSamplingLocation.PleuralAspiration },
        {12,MeningoSamplingLocation.PharynxSwab },
        {13,MeningoSamplingLocation.OtherInvasive },
        {14,MeningoSamplingLocation.Sputum },
        {15,MeningoSamplingLocation.TonsilSwab },
        {16,MeningoSamplingLocation.TrachealSecretion },
        {17,MeningoSamplingLocation.VaginalSwab },
        {18,MeningoSamplingLocation.Serum },
        {19,MeningoSamplingLocation.OtherNonInvasive },
        {20,MeningoSamplingLocation.OtherInvasive },
        {21,MeningoSamplingLocation.UrethralSwab },
        {22,MeningoSamplingLocation.AnalSwab }
    };

    public MeningoSamplingLocation Convert(object accessSamplingLocationId, MeningoSamplingLocation destination, ResolutionContext context)
    {
        try
        {
            if (accessSamplingLocationId is DBNull)
            {
                return MeningoSamplingLocation.OtherInvasive;
            }
            var samplingLocation = LegacyIdToMeningoSamplingLocationMap[accessSamplingLocationId];
            return samplingLocation;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to parse sampling location for: {accessSamplingLocationId}");
            throw e;
        }
    }
}