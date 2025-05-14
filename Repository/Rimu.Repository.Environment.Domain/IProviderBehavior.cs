namespace Rimu.Repository.Environment.Domain;

public interface IProviderBehavior<T> where T : class {
    public T GetInstance { get; }
}