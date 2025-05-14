using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Authentication.Adapter.Interface;

namespace Rimu.Repository.Authentication.Adapter;

public static class Extension {
    public static IAuthenticationProvider GetAuthenticationProvider(this IServiceProvider self) => self.GetService<IAuthenticationProvider>() ?? throw new InvalidOperationException();
    public static IApi2TokenProvider GetApi2TokenProvider(this IServiceProvider self) => self.GetService<IApi2TokenProvider>() ?? throw new InvalidOperationException();
    public static IUserAuthContext GetUserAuthContext(this IServiceProvider self) => self.GetService<IUserAuthContext>() ?? throw new InvalidOperationException();
    public static IUserAuthDataContext GetUserAuthDataContext(this IServiceProvider self) => self.GetService<IUserAuthDataContext>() ?? throw new InvalidOperationException();
}