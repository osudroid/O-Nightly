namespace OsuDroidMediator.Domain.Interface; 

public interface IDomainData {
    public IValuesAreGood DataValue { get; }
    public IDto ToDto();
}