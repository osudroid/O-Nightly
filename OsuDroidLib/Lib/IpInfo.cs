using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;

namespace OsuDroidLib.Lib;

public static class IpInfo {
    public static CountryResponse? Country(IPAddress ipAddress) {
        try {
            if (Equals(ipAddress, IPAddress.None)) {
                return null;
            }
            var client = new WebServiceClient(Setting.GeoIp_UserId!.Value, Setting.GeoIp_LicenseKey!.Value, null,
                "geolite.info");
            CountryResponse country = client.Country(ipAddress);
            return country;
        }
        catch (Exception) {
            return null;
        }
    }
}