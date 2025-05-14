using LamLibAllOver.ErrorHandling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Environment.Domain.Class;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Environment.Domain.Provider;

internal  class EnvDbProviderBehavior: IProviderBehavior<IEnvDb> {
    private readonly IQuerySetting _querySetting;
    private static volatile IEnvDb? _envJson;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    

    public IEnvDb GetInstance {
        get {
            if (_envJson is null) {
                _envJson = CreateAsync().GetAwaiter().GetResult();
            }
            return _envJson;
        }
    }

    public EnvDbProviderBehavior(IQuerySetting querySetting) {
        _querySetting = querySetting; 
    }
    

    private async Task<IEnvDb> CreateAsync() {
        var queryResult = await _querySetting.GetAllAsync();
        if (queryResult == EResult.Err) {
            throw new Exception("Create EnvDb Failed");
        }

        var rows = queryResult
                   .Ok()
                   .ToDictionary(x => x.MainKey + "_" + x.SubKey, x => x.Value)
        ;
        
        return new EnvDb(rows);
    }
}