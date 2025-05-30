namespace Rimu.Repository.Region.Adapter.Interface;

/// <summary>
/// Represents a country with various properties such as name, continent, capital, and more.
/// </summary>
public interface ICountry {
    public string NameShort { get; init; }
    public string Name { get; init; }
    public string Native { get; init; }
    public string[] Phone { get; init; }
    public string Continent { get; init; }
    public string Capital { get; init; }
    public string[] Currency { get; init; }
    public string[] Languages { get; init; }
}