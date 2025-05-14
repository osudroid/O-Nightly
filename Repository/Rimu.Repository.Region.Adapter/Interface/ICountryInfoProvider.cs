using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Region.Adapter.Interface;

public interface ICountryInfoProvider {
    public ICountry? FindByNameShort(string nameShort);

    public Option<ICountry> GetRegionAsCountry(string region);

    public Option<ICountry> FindByName(string name);
}