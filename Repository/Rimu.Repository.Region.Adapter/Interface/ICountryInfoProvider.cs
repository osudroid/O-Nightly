using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Region.Adapter.Interface;

/// <summary>
/// Provides methods to retrieve country information based on various criteria.
/// </summary>
public interface ICountryInfoProvider {
    /// <summary>
    /// Finds a country by its short name or abbreviation.
    /// </summary>
    /// <param name="nameShort">The short name or abbreviation of the country.</param>
    /// <returns>The country matching the short name, or null if not found.</returns>
    public ICountry? FindByNameShort(string nameShort);

    /// <summary>
    /// Retrieves a region as a country object.
    /// </summary>
    /// <param name="region">The name of the region to retrieve.</param>
    /// <returns>An option containing the country if found, or an empty option if not.</returns>
    public Option<ICountry> GetRegionAsCountry(string region);

    /// <summary>
    /// Finds a country by its full name.
    /// </summary>
    /// <param name="name">The full name of the country.</param>
    /// <returns>An option containing the country if found, or an empty option if not.</returns>
    public Option<ICountry> FindByName(string name);
}