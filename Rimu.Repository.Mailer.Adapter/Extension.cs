using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Mailer.Adapter.Interface;

namespace Rimu.Repository.Mailer.Adapter;

public static class Extension {
    public static IMailerProvider GetMailerProvider(this IServiceProvider self) => self.GetService<IMailerProvider>() ?? throw new InvalidOperationException();
}