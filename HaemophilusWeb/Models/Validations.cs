namespace HaemophilusWeb.Models
{
    internal class Validations
    {
        public const string PostalCodeValidation = @"\d{5}";
        public const string PostalCodeValidationError = "Die Postleitzahl muss eine 5-stellige Nummer sein";
    }
}