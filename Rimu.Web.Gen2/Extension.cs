using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Feature.Login;

namespace Rimu.Web.Gen2;

public static class Extension {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public static Option<TokenWithTTLDto> GetUserTokenTtl(this IEndpoint self) {
        var item = self.HttpContext.Items["UserTokenTtl"];
        if (item is null) {
            return default;
        }
        return (Option<TokenWithTTLDto>) item;
    }
}