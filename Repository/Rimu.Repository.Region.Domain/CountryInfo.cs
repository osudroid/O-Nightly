using Rimu.Repository.Region.Adapter.Interface;

namespace Rimu.Repository.Region.Domain;

public readonly struct Country: ICountry {
    public readonly string NameShort { get; init; }
    public readonly string Name { get; init; }
    public readonly string Native { get; init; }
    public readonly string[] Phone { get; init; }
    public readonly string Continent { get; init; }
    public readonly string Capital { get; init; }
    public readonly string[] Currency { get; init; }
    public readonly string[] Languages { get; init; }
}