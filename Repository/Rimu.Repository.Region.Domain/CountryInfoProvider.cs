using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Repository.Region.Domain;

public class CountryInfoProvider: ICountryInfoProvider {
    public ICountry? FindByNameShort(string nameShort) => CountryInfoHolder.FindByNameShort(nameShort);

    public Option<ICountry> GetRegionAsCountry(string region) => CountryInfoHolder.GetRegionAsCountry(region);

    public Option<ICountry> FindByName(string name) => CountryInfoHolder.FindByName(name);
}