using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Environment.Domain.Class;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Environment.Domain.Provider;

public class EnvProvider: IEnvProvider {
    private readonly EnvDbFluidProviderBehavior _envDbFluidProviderBehavior;
    private readonly EnvDbProviderBehavior _envDbProviderBehavior;
    private readonly EnvJsonProviderBehavior _envJsonProviderBehavior;

    public IEnvJson EnvJson => _envJsonProviderBehavior.GetInstance;
    public IEnvDb EnvDb => _envDbProviderBehavior.GetInstance;

    public EnvProvider(IQuerySetting querySetting, IQuerySettingsHot querySettingsHot) {
        _envDbFluidProviderBehavior = new EnvDbFluidProviderBehavior(querySettingsHot);
        _envDbProviderBehavior = new EnvDbProviderBehavior(querySetting);
        _envJsonProviderBehavior = new EnvJsonProviderBehavior();
    }

    public Task<IEnvDbFluid> GetEnvDbFluid() => _envDbFluidProviderBehavior.GetInstance;
}