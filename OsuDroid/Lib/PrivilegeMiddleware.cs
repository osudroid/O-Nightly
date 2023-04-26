using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;

namespace OsuDroid.Lib;

internal class PrivilegeRouteAttribute : Attribute
{
    public PrivilegeRouteAttribute(string route) => Route = route;

    public string Route { get; init; }
}

public interface ICookieHandler {
    public string Name { get; }
    
    public Result<(bool IsOk, long UserId), string> HandleCookie (IRequestCookieCollection requestCookie, IResponseCookies responseCookies);
}

public class PrivilegeMiddleware {
    public record RouteInfo(Guid NeedPrilegeId, string NeedPrilegeName, bool NeedCookie, Option<ICookieHandler> CookieHandler);
    
    private static IReadOnlyDictionary<string, ICookieHandler> CookieHandlers = new Dictionary<string, ICookieHandler>(1);
    private static IReadOnlyDictionary<string, RouteInfo> NeedPrivilegeDic = new Dictionary<string, RouteInfo>(1);
    
    private readonly RequestDelegate _next;
    
    public static void Load(IReadOnlyList<ICookieHandler> cookieHandlerList) {
        var cookieHandlers = new Dictionary<string, ICookieHandler>(cookieHandlerList.Count * 2);
        foreach (var cookieHandler in cookieHandlerList) {
            cookieHandlers[cookieHandler.Name] = cookieHandler;
        }

        CookieHandlers = cookieHandlers;
        
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var result = db.Fetch<Entities.RouterSetting>("SELECT * FROM router_setting");
        if (result == EResult.Err)
            throw new Exception(result.Err());
        var routerSettings = result.Ok();

        var newMap = new Dictionary<string, RouteInfo>(routerSettings.Count * 2);
        foreach (var routerSetting in routerSettings) {
            var fetchResult = db.SingleOrDefault<Entities.NeedPrivilege>("SELECT * FROM need_privilege WHERE need_privilege_id = @0", routerSetting.NeedPrivilege);
            if (newMap.ContainsKey(routerSetting.Path ?? ""))
                throw new Exception("Key Exist In NeedPrivilegeMap");
            if (fetchResult == EResult.Err || fetchResult.Ok() == null)
                throw new Exception("NeedPrivilege Not Found");

            var needPrivilege = fetchResult.Ok();
            Option<ICookieHandler> cookieHandler = default;
            if (cookieHandlers.ContainsKey(routerSetting.NeedCookieManager ?? ""))
                cookieHandler = new Option<ICookieHandler>(cookieHandlers[routerSetting.NeedCookieManager??""]);
            
            newMap[routerSetting.Path!] = new(needPrivilege.NeedPrivilegeId, needPrivilege.Name??"", routerSetting.NeedCookie, cookieHandler);
        }

        NeedPrivilegeDic = newMap;
    }
    
    public PrivilegeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context) {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<PrivilegeRouteAttribute>();
        
        if (NeedPrivilegeDic.TryGetValue(attribute.Route, out var routeInfo) == false) {
            context.Response.StatusCode = 404;
            return;
        }

        if (routeInfo!.NeedCookie == false) {
            await _next.Invoke(context);
            return;
        }


        if (routeInfo.CookieHandler.IsSet())
            throw new NullReferenceException("CookieHandler Not Set");

        await _next.Invoke(context);
        var result = routeInfo.CookieHandler.Unwrap().HandleCookie(
            context.Request.Cookies,
            context.Response.Cookies);

        if (result == EResult.Err)
            throw new Exception(result.Err());
        
        if (result.Ok().IsOk == false) {
            context.Response.StatusCode = 403;
            return;
        }

        await using var db = DbBuilder.BuildNpgsqlConnection();
        var resultPrivilegeOk = OsuDroid.Lib.PrivilegeManager.UserCanUseById(db, routeInfo.NeedPrilegeId, result.Ok().UserId);

        if (resultPrivilegeOk == EResult.Err)
            throw new Exception(resultPrivilegeOk.Err());
        
        foreach ((string name, Guid id, bool has) in resultPrivilegeOk.Ok()) {
            if (has) 
                continue;

            context.Response.StatusCode = 403;
            return;
        }
        
        await _next(context);
    } 
}

public static class PrivilegeMiddlewareExtensions
{
    public static IApplicationBuilder UsePrivilege(this IApplicationBuilder builder, IReadOnlyList<ICookieHandler> cookieHandler) {
        PrivilegeMiddleware.Load(cookieHandler);
        return builder.UseMiddleware<PrivilegeMiddleware>();
    }
}
