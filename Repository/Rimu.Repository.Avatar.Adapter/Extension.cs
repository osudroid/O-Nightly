using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Avatar.Adapter.Interface;

namespace Rimu.Repository.Avatar.Adapter;

public static class Extension {
    public static IUserAvatarProvider GetUserAvatarProvider(this IServiceProvider self) => self.GetService<IUserAvatarProvider>() ?? throw new InvalidOperationException();
}