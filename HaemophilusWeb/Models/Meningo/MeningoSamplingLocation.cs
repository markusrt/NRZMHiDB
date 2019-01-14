using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    /// <summary>
    /// Invasive locations (ttlam, 2019-01-14):
    //  Blut, Blut und Liquor, Gelenkspunktat, Liquor, Petechien, Pleurapunktat, Anderer invasiv
    /// </summary>
    public enum MeningoSamplingLocation
    {
        [Description("Analabstrich")]
        AnalSwab = 1,
        [Description("Bindehautabstrich")]
        ConjunctivalSwab = 2,
        [InvasiveSamplingLocation]
        [Description("Blut")]
        Blood = 3,
        [Description("Blut und Liquor")]
        [InvasiveSamplingLocation]
        BloodAndLiquor = 4,
        [Description("Bronchiallavage")]
        BronchoalveolarLavage = 5,
        [Description("Cervixabstrich")]
        CervixSwab = 6,
        [Description("Gelenkspunktat")]
        [InvasiveSamplingLocation]
        JointAspiration = 7,
        [Description("Liquor")]
        [InvasiveSamplingLocation]
        Liquor = 8,
        [Description("Nasenabstrich")]
        NasalSwab = 9,
        [Description("Ohrabstrich")]
        EarSwab = 10,
        [Description("Petechien")]
        [InvasiveSamplingLocation]
        Petechien = 11,
        [Description("Pleurapunktat")]
        [InvasiveSamplingLocation]
        PleuralAspiration = 12,
        [Description("Rachenabstrich")] PharynxSwab = 13,
        [Description("Sputum")] Sputum = 14,
        [Description("Tonsillenabstrich")] TonsilSwab = 15,
        [Description("Trachealsekret")] TrachealSecretion = 16,
        [Description("Urethralabstrich")] UrethralSwab = 17,
        [Description("Vaginalabstrich")] VaginalSwab = 18,
        [Description("Serum")] Serum = 19,
        [Description("Anderer invasiv")]
        [InvasiveSamplingLocation]
        OtherInvasive = 32767,
        [Description("Anderer nicht invasiv")]
        OtherNonInvasive = 32768
    }
}