using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum MeningoSamplingLocation
    {
        [Description("Analabstrich")]AnalSwab = 1,
        [Description("Bindehautabstrich")] ConjunctivalSwab = 2,
        [Description("Blut")] Blood = 3,
        [Description("Bronchiallavage")] BronchoalveolarLavage = 4,
        [Description("Cervixabstrich")] CervixSwab = 5,
        [Description("Gelenkspunktat")] JointAspiration = 6,
        [Description("Liquor")] Liquor = 7,
        [Description("Nasenabstrich")] NasalSwab = 8,
        [Description("Ohrabstrich")] EarSwab = 9,
        [Description("Petechien")] Petechien = 10,
        [Description("Pleurapunktat")] PleuralAspiration = 11,
        [Description("Rachenabstrich")] PharynxSwab = 12,
        [Description("Sputum")] Sputum = 13,
        [Description("Tonsillenabstrich")] TonsilSwab = 14,
        [Description("Trachealsekret")] TrachealSecretion = 15,
        [Description("Urethralabstrich")] UrethralSwab = 16,
        [Description("Vaginalabstrich")] VaginalSwab = 17,
        [Description("Serum")] Serum = 18,
        [Description("Anderer")] Other = 32768
    }
}