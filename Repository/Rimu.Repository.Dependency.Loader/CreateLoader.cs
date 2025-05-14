using Rimu.Repository.Dependency.Adapter.LoadProvider;
using Rimu.Repository.Dependency.Domain;

namespace Rimu.Repository.Dependency.Loader;

public class CreateLoader {
    public static ILoadProvider Create() => new LoadProvider();
}