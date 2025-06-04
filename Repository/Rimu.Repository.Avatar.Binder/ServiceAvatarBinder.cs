using ImageMagick.Formats;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Avatar.Domain;
using Rimu.Repository.Avatar.Domain.ImageConverter;
using Rimu.Repository.Avatar.Domain.ImageConverter.Interface;
using Rimu.Repository.Dependency.Adapter.Export;
using Rimu.Repository.Environment.Adapter;
using Rimu.Repository.Postgres.Adapter;

namespace Rimu.Repository.Avatar.Binder;

public sealed class ServiceAvatarBinder: IServiceBinder {
    public void Bind(IServiceCollection serviceCollection) {
        serviceCollection.AddScoped<IImageTo<PngWriteDefines>>(static x => new ImageToPng(x.GetEnvDb()));
        serviceCollection.AddScoped<IImageTo<WebPWriteDefines>>(static x => new ImageToWebp(x.GetEnvDb()));
        serviceCollection.AddScoped<IUserAvatarProvider>(static x => new UserAvatarProvider(
            x.GetQueryView_UserAvatarNoBytes(), 
            x.GetQueryUserAvatar(), 
            x.GetEnvDb(),
            x.GetService<IImageTo<WebPWriteDefines>>()?? throw new NullReferenceException(nameof(IImageTo<WebPWriteDefines>)),
            x.GetService<IImageTo<PngWriteDefines>>()?? throw new NullReferenceException(nameof(IImageTo<PngWriteDefines>))
            ));
    }
}