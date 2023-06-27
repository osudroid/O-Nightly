using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;

namespace OsuDroidLib.Lib;

public static class IpInfo {
    public static CountryResponse? Country(IPAddress ipAddress) {
        try {
            var client = new WebServiceClient(Setting.GeoIp_UserId!.Value, Setting.GeoIp_LicenseKey!.Value, null,
                "geolite.info");
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