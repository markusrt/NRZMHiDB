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
        [Description("Liquor und Blut")] LiquorAndBlood = 8,
        [Description("Nasenabstrich")] NasalSwab = 9,
        [Description("Ohrabstrich")] EarSwab = 10,
        [Description("Petechien")] Petechien = 11,
        [Description("Pleurapunktat")] PleuralAspiration = 12,
        [Description("Rachenabstrich")] PharynxSwab = 13,
        [Description("Sonstiges (invasiv)")] OtherInvasive = 14,
        [Description("Sputum")] Sputum = 15,
        [Description("Tonsillenabstrich")] TonsilSwab = 16,
        [Description("Trachealsekret")] TrachealSecretion = 17,
        [Description("Urethralabstrich")] UrethralSwab = 18,
        [Description("Vaginalabstrich")] VaginalSwab = 19,
        [Description("Serum")] Serum = 20,
        [Description("Sonstiges (nicht invasiv)")] OtherNonInvasive = 21,
        [Description("nicht-steriles Mat. bei IMD")] NonSterileImd = 22,
        [Description("Anderer")] Other = 32768
    }
}