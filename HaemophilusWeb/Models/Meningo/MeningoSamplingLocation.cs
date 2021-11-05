using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    /// <summary>
    ///  <p>
    ///    Additional invasive location (@hclaus24, 2021-11-05): Serum
    ///  </p>
    ///  <p>
    ///    Initial invasive locations (@thien-tri, 2019-01-14):
    ///    Blut, Blut und Liquor, Gelenkspunktat, Liquor, Petechien, Pleurapunktat, Anderer invasiv
    ///  </p>
    /// </summary>
    public enum MeningoSamplingLocation
    {
        [Description("Analabstrich")]
        AnalSwab = 0,
        [Description("Bindehautabstrich")]
        ConjunctivalSwab = 1,
        [InvasiveSamplingLocation]
        [Description("Blut")]
        Blood = 2,
        [Description("Blut und Liquor")]
        [InvasiveSamplingLocation]
        BloodAndLiquor = 3,
        [Description("Bronchiallavage")]
        BronchoalveolarLavage = 4,
        [Description("Cervixabstrich")]
        CervixSwab = 5,
        [Description("Gelenkspunktat")]
        [InvasiveSamplingLocation]
        JointAspiration = 6,
        [Description("Liquor")]
        [InvasiveSamplingLocation]
        Liquor = 7,
        [Description("Nasenabstrich")]
        NasalSwab = 8,
        [Description("Ohrabstrich")]
        EarSwab = 9,
        [Description("Petechien")]
        [InvasiveSamplingLocation]
        Petechien = 10,
        [Description("Pleurapunktat")]
        [InvasiveSamplingLocation]
        PleuralAspiration = 11,
        [Description("Rachenabstrich")] PharynxSwab = 12,
        [Description("Sputum")] Sputum = 13,
        [Description("Tonsillenabstrich")] TonsilSwab = 14,
        [Description("Trachealsekret")] TrachealSecretion = 15,
        [Description("Urethralabstrich")] UrethralSwab = 16,
        [Description("Vaginalabstrich")] VaginalSwab = 17,
        [InvasiveSamplingLocation]
        [Description("Serum")]
        Serum = 18,
        [Description("Anderer invasiv")]
        [InvasiveSamplingLocation]
        OtherInvasive = 32767,
        [Description("Anderer nicht invasiv")]
        OtherNonInvasive = 32768
    }
}