using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Environment.Domain.Class;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Environment.Domain.Provider;

internal class EnvDbFluidProviderBehavior: IProviderBehavior<Task<IEnvDbFluid>> {
    private readonly IQuerySettingsHot _querySettingsHot;
    private readonly IEnvDbFluid? _envDbFluid = null;

    public Task<IEnvDbFluid> GetInstance {
        get {
            if (_envDbFluid is null) {
                return CreateAndSetEnvDbFluidAsync();
            }

            return Task.FromResult(_envDbFluid);
        }
    }

    public EnvDbFluidProviderBehavior(IQuerySettingsHot querySettingsHot) {
        _querySettingsHot = querySettingsHot;
    }

    private async Task<IEnvDbFluid> CreateAndSetEnvDbFluidAsync() {
        var resultOk =  (await _querySettingsHot.GetAllAsync())
                        .Map(x => x.ToDictionary(x => x.MainKey + "_" + x.SubKey, x => x.Value))
                        .Map(x => (IEnvDbFluid) new EnvDbFluid(x));
        if (resultOk == EResult.Err) {
            throw new Exception("Create EnvDb Failed");
        }

        return resultOk.Ok();
    }
}