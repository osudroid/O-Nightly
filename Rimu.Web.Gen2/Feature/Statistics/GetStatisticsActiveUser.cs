using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Share.Rank;

namespace Rimu.Web.Gen2.Feature.Statistics;

public sealed class GetStatisticsActiveUser: EndpointWithoutRequest<
    Results<Ok<GetStatisticsActiveUser.StatisticsActiveUserResponse>, InternalServerError>
> {
    private readonly IQueryUserInfo _queryUserInfo;

    public GetStatisticsActiveUser(IQueryUserInfo queryUserInfo) {
        _queryUserInfo = queryUserInfo;
    }

    public override void Configure() {
        Get("/api2/statistic/active-user");
        ResponseCache(120);
    }

    public override async Task<Results<Ok<StatisticsActiveUserResponse>, InternalServerError>> 
        ExecuteAsync(CancellationToken ct) {

        return (await _queryUserInfo.GetStatisticActiveUserAsync())
            .Map<Results<Ok<StatisticsActiveUserResponse>, InternalServerError>>(x => TypedResults.Ok(new StatisticsActiveUserResponse {
                ActiveUserLast1Day = x.ActiveUserLast1Day,
                ActiveUserLast1H = x.ActiveUserLast1H,
                RegisterUser = x.RegisterUser
            }))
            .OkOr(TypedResults.InternalServerError())
        ;

    }

    public sealed class StatisticsActiveUserResponse {
        public required long ActiveUserLast1H { get; init; }
        public required long ActiveUserLast1Day { get; init; }
        public required long RegisterUser { get; init; }
    }
}