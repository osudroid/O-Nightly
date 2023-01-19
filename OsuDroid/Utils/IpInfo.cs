using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;

namespace OsuDroid.Utils;

public static class IpInfo {
    public static CountryResponse? Country(IPAddress ipAddress) {
        try {
            var client = new WebServiceClient(int.Parse(Env.UserIdGeoIp), Env.LicenseKeyGeoIp, null, "geolite.info");
            var country = client.Country(ipAddress);
            return country;
        }
#if DEBUG
        catch (Exception e) {
            WriteLine(e);
            throw;
        }
#else
        catch (Exception) {
            return null;
        }
#endif
    }
}