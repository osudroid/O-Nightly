using System.Net;
using LamLibAllOver.ErrorHandling;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Repository.Region.Domain;

public sealed class IPAdressInfoProvider: IIPAdressInfoProvider {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private static Option<WebServiceClient> _webServiceClient = default; 
    private readonly IEnvDb _envDb;
    private readonly ICountryInfoProvider _countryInfoProvider;

    public IPAdressInfoProvider(IEnvDb envDb, ICountryInfoProvider countryInfoProvider) {
        _envDb = envDb;
        _countryInfoProvider = countryInfoProvider;
    }

    public async Task<ResultOk<Option<ICountry>>> GetCountyFromIPAdressAsync(IPAddress ipAddress) {
        try {
            var geoIpUserId = _envDb.GeoIp_UserId;
            var geoIpLicenseKey = _envDb.GeoIp_LicenseKey;

            if (Equals(ipAddress, IPAddress.None) || Equals(ipAddress, IPAddress.Any)) {
                Logger.Warn($"IP Address '{ipAddress}' is not a allowed IP address.");
                return ResultOk<Option<ICountry>>.Ok(default);
            }

            if (_webServiceClient.IsNotSet()) {
                _webServiceClient = Option<WebServiceClient>.NullSplit(new WebServiceClient(geoIpUserId, geoIpLicenseKey, null,
                    "geolite.info"
                ));    
            }

            var client = _webServiceClient.Unwrap();
            
            var countryResponse = await client.CountryAsync(ipAddress);

            return ResultOk<Option<ICountry>>
                .Ok(Option<ICountry>
                    .NullSplit(_countryInfoProvider.FindByNameShort(countryResponse.Country.IsoCode??"")));
        }
        catch (Exception e) {
            Logger.Error(e, "Error while getting country");
            return ResultOk<Option<ICountry>>.Err();
        }
    }
}