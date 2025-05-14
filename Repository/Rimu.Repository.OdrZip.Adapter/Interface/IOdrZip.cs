using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.OdrZip.Adapter.Interface;

public interface IOdrZip {
    public Task<ResultOk<Option<(byte[] bytes, string name)>>> FactoryAsync(long odrNumber);
}