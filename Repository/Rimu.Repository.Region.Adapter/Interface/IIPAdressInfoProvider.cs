using System.Net;
using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Region.Adapter.Interface;

public interface IIPAdressInfoProvider {
    public Task<ResultOk<Option<ICountry>>> GetCountyFromIPAdressAsync(IPAddress ipAddress);
}