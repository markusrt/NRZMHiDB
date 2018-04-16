using System.ComponentModel;

namespace HaemophilusWeb.Models.Meningo
{
    public enum MeningoSamplingLocation
    {
        [Description("Bindehautabstrich")] ConjunctivalSwab = 1,
        [Description("Blut")] Blood = 2,
        [Description("Bronchiallavage")] BronchoalveolarLavage = 3,
        [Description("Cervixabstrich")] CervixSwab = 4,
        [Description("Gelenkspunktat")] JointAspiration = 5,
        [Description("Liquor")] Liquor = 6,
        [Description("Liquor und Blut")] LiquorAndBlood = 7,
        [Description("Nasenabstrich")] NasalSwab = 8,
        [Description("Ohrabstrich")] EarSwab = 9,
        [Description("Petechien")] Petechien = 10,
        [Description("Pleurapunktat")] PleuralAspiration = 11,
        [Description("Rachenabstrich")] PharynxSwab = 12,
        [Description("Sonstiges (invasiv)")] OtherInvasive = 13,
        [Description("Sputum")] Sputum = 14,
        [Description("Tonsillenabstrich")] TonsilSwab = 15,
        [Description("Trachealsekret")] TrachealSecretion = 16,
        [Description("Vaginalabstrich")] VaginalSwab = 17,
        [Description("Serum")] Serum = 18,
        [Description("Sonstiges (nicht invasiv)")] OtherNonInvasive = 19,
        [Description("nicht-steriles Mat. bei IMD")] NonSterileImd = 20,
        [Description("Anderer")] Other = 21
    }
}