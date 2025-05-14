using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Avatar.Domain;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Postgres.Adapter;

namespace Rimu.Repository.Avatar.Binder;

public sealed class ServiceAvatarBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<IUserAvatarProvider>(static x => new UserAvatarProvider(x.GetQueryView_UserAvatarNoBytes(), x.GetQueryUserAvatar(), x.GetEnvDb()));
    }
}