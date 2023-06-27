using Npgsql;
using System.Net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Lib;

public static class UserInfoHandler {
    public static async Task<ResultErr<string>> UpdateIpAndRegionByIpAsync(NpgsqlConnection db, UserInfo userInfo,
        IPAddress address) {
        CountryResponse? countryResponse = IpInfo.Country(address);
        if (countryResponse is null || countryResponse.RegisteredCountry.Name is null)
            return ResultErr<string>.Err("County Not Found By IpAddress");
        var countryName = countryResponse.RegisteredCountry.Name;

        userInfo.LatestIp = address.ToString();

        var optionCountry = CountryInfo.FindByName(countryName);
        if (optionCountry.IsSet() == false)
            return ResultErr<string>.Err("CountryInfo Not Found");

        userInfo.Region = optionCountry.Unwrap().NameShort;

        return await QueryUserInfo.UpdateIpAndRegionAsync(db, userInfo.UserId, userInfo.Region, userInfo.LatestIp);
    }
}