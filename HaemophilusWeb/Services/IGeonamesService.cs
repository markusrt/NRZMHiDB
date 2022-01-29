using Geolocation;

namespace HaemophilusWeb.Utils;

public interface IGeonamesService
{
    string QueryByPostalCode(string postalCode, string placeName = "");
    string QueryByPostalCodePrefix(string postalCodePrefix);
    Coordinate? QueryCoordinateByPostalCode(string postalCode, string placeName = "");
}