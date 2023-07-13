using OsuDroidMediator.Domain.Interface;

namespace OsuDroidMediator.Domain.Model; 

public sealed class DomainData<T, E>(T dataValue, Func<T, E> toDto): 
    IDomainData
    where T: IValuesAreGood
    where E: IDto {

    public IValuesAreGood DataValue { get; } = dataValue;

    public IDto ToDto() => toDto(dataValue);
}