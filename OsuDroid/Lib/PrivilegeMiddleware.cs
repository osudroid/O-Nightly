using Microsoft.AspNetCore.Http.Features;
using Npgsql;
using OsuDroidLib.Extension;

namespace OsuDroid.Lib;

internal class PrivilegeRouteAttribute : Attribute
{
    public PrivilegeRouteAttribute(string route) => Route = route;

    public string Route { get; init; }
}

public interface ICookieHandler {
    public string Name { get; }
    
    public Task<Result<(bool IsOk, long UserId, Guid Token), string>> HandleCookieAsync (
        NpgsqlConnection db, IRequestCookieCollection requestCookie, IResponseCookies responseCookies);
}

public class PrivilegeMiddleware {
    public const string ItemName = "USER_ID";
    
    public record RouteInfo(Guid NeedPrilegeId, string NeedPrivileName, bool NeedCookie, Option<ICookieHandler> CookieHandler);
    
    private static IReadOnlyDictionary<string, ICookieHandler> CookieHandlers = new Dictionary<string, ICookieHandler>(1);
    private static IReadOnlyDictionary<string, RouteInfo> NeedPrivilegeDic = new Dictionary<string, RouteInfo>(1);
    
    private readonly RequestDelegate _next;
    
    public static async Task Load(IReadOnlyList<ICookieHandler> cookieHandlerList) {
        var cookieHandlers = new Dictionary<string, ICookieHandler>(cookieHandlerList.Count * 2);
        foreach (var cookieHandler in cookieHandlerList) {
            cookieHandlers[cookieHandler.Name] = cookieHandler;
        }
    
        CookieHandlers = cookieHandlers;
        
        await using var db = await DbBuilder.BuildNpgsqlConnection();
    
        var result = await db.SafeQueryAsync<Entities.RouterSetting>("SELECT * FROM RouterSetting");
        if (result == EResult.Err)
            throw new Exception(result.Err());
        var routerSettings = result.Ok().ToList();
    
        var newMap = new Dictionary<string, RouteInfo>(routerSettings.Count * 2);
        foreach (var routerSetting in routerSettings) {
            var fetchResult = await db.SafeQueryFirstOrDefaultAsync<Entities.NeedPrivilege>(
                "SELECT * FROM NeedPrivilege WHERE NeedPrivilegeId = @NeedPrivilegeId", 
                new { NeedPrivilegeId = routerSetting.NeedPrivilege }
                );
            
            if (newMap.ContainsKey(routerSetting.Path ?? ""))
                throw new Exception("Key Exist In NeedPrivilegeMap");
            if (fetchResult == EResult.Err || fetchResult.Ok().IsNotSet())
                throw new Exception("NeedPrivilege Not Found");
    
            var needPrivilege = fetchResult.Ok().Unwrap();
            Option<ICookieHandler> cookieHandler = Option<ICookieHandler>.Empty;
            
            if (cookieHandlers.ContainsKey(routerSetting.NeedCookieHandler ?? ""))
                cookieHandler = new Option<ICookieHandler>(cookieHandlers[routerSetting.NeedCookieHandler??""]);
            
            newMap[routerSetting.Path!] = new(needPrivilege.NeedPrivilegeId, needPrivilege.Name??"", routerSetting.NeedCookie, cookieHandler);
        }
    
        NeedPrivilegeDic = newMap;
    }
    
    public PrivilegeMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context) {
        Endpoint? endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

        if (endpoint is null) {
            await _next.Invoke(context);
            return;
        }
            
        PrivilegeRouteAttribute? attribute = endpoint.Metadata.GetMetadata<PrivilegeRouteAttribute>();
        
        if (attribute is null) throw new NullReferenceException(nameof(attribute));

        var checkResult = CheckIfNeedCookie(attribute);
        if (checkResult.routeInfo.IsSet() == false) {
            context.Response.StatusCode = 403;
            return;
        }
        
        if (checkResult.needCookie == false) {
            await _next.Invoke(context);
            return;
        }

        RouteInfo routeInfo = checkResult.routeInfo.Unwrap();
        
        if (routeInfo.CookieHandler.IsSet())
            throw new NullReferenceException("CookieHandler Not Set");

        await InvokeAsyncWithCookie(context, routeInfo);
    }

    private async Task InvokeAsyncWithCookie(HttpContext context, RouteInfo routeInfo) {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        
        var result = await routeInfo.CookieHandler.Unwrap().HandleCookieAsync(
            db,
            context.Request.Cookies,
            context.Response.Cookies);

        if (result == EResult.Err)
            throw new Exception(result.Err());

        var (isOk, userId, token) = result.Ok();
        
        if (isOk == false) {
            context.Response.StatusCode = 403;
            return;
        }

        
        var resultPrivilegeOk = OsuDroid.Lib.PrivilegeManager.UserCanUseById(db, routeInfo.NeedPrilegeId, result.Ok().UserId);

        if (resultPrivilegeOk == EResult.Err)
            throw new Exception(resultPrivilegeOk.Err());
        
        foreach ((string name, Guid id, bool has) in resultPrivilegeOk.Ok()) {
            if (has) 
                continue;

            context.Response.StatusCode = 403;
            return;
        }

        
        context.Items.Add(ItemName, new UserIdAndToken(userId, token));
        await _next(context);
    }
    
    private static (bool needCookie, Option<RouteInfo> routeInfo) CheckIfNeedCookie(PrivilegeRouteAttribute attribute) {
        return NeedPrivilegeDic.TryGetValue(attribute.Route, out RouteInfo? routeInfo) == false 
            ? (false, Option<RouteInfo>.Empty) 
            : (true, Option<RouteInfo>.With(routeInfo));
    }
}

public static class PrivilegeMiddlewareExtensions
{
    public static IApplicationBuilder UsePrivilege(this IApplicationBuilder builder, IReadOnlyList<ICookieHandler> cookieHandler) {
        PrivilegeMiddleware.Load(cookieHandler).Wait();
        return builder.UseMiddleware<PrivilegeMiddleware>();
    }
}


public record UserIdAndToken(long UserId, Guid Token);