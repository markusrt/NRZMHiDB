using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum MeningoSamplingLocation
    {
        [Description("Analabstrich")]AnalSwab = 1,
        [Description("Bindehautabstrich")] ConjunctivalSwab = 2,
        [Description("Blut")] Blood = 3,
        [Description("Blut und Liquor")] BloodAndLiquor = 4,
        [Description("Bronchiallavage")] BronchoalveolarLavage = 5,
        [Description("Cervixabstrich")] CervixSwab = 6,
        [Description("Gelenkspunktat")] JointAspiration = 7,
        [Description("Liquor")] Liquor = 8,
        [Description("Nasenabstrich")] NasalSwab = 9,
        [Description("Ohrabstrich")] EarSwab = 10,
        [Description("Petechien")] Petechien = 11,
        [Description("Pleurapunktat")] PleuralAspiration = 12,
        [Description("Rachenabstrich")] PharynxSwab = 13,
        [Description("Sputum")] Sputum = 14,
        [Description("Tonsillenabstrich")] TonsilSwab = 15,
        [Description("Trachealsekret")] TrachealSecretion = 16,
        [Description("Urethralabstrich")] UrethralSwab = 17,
        [Description("Vaginalabstrich")] VaginalSwab = 18,
        [Description("Serum")] Serum = 19,
        [Description("Anderer")] Other = 32768
    }
}