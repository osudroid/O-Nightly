using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Security.Adapter.Interface;

namespace Rimu.Repository.Security.Adapter;

public static class Extension {
    public static ISecurityProvider GetSecurityProvider(this IServiceProvider self) => self.GetService<ISecurityProvider>() ?? throw new InvalidOperationException();
    public static ISecurity GetSecurity(this IServiceProvider self) => self.GetService<ISecurity>() ?? throw new InvalidOperationException();
    public static ISecurityPhp GetSecurityPhp(this IServiceProvider self) => self.GetService<ISecurityPhp>() ?? throw new InvalidOperationException();
    public static IInputCheckerAndConvertPhp GetInputCheckerAndConvertPhp(this IServiceProvider self) => self.GetService<IInputCheckerAndConvertPhp>() ?? throw new InvalidOperationException();
}