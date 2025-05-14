using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Statistics;

public sealed class GetStatisticsClassifications: EndpointWithoutRequest<
    Results<Ok<GetStatisticsClassifications.StatisticsClassificationsDto[]>, InternalServerError>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IQueryView_UserInfo_UserClassifications _queryView_UserInfo_Classifications;

    public GetStatisticsClassifications(IQueryView_UserInfo_UserClassifications queryViewUserInfoClassifications) {
        _queryView_UserInfo_Classifications = queryViewUserInfoClassifications;
    }

    public override void Configure() {
        Get("/api2/statistic/classifications");
        ResponseCache(240);
    }

    public override async Task<Results<Ok<StatisticsClassificationsDto[]>, InternalServerError>> ExecuteAsync(CancellationToken ct) {
        return (await _queryView_UserInfo_Classifications.GetAllWithSingleActiveClassificationsAndPublicShowAsync())
               .Map(static x => x.Select(StatisticsClassificationsDto.From).ToArray())
               .Map<Results<Ok<StatisticsClassificationsDto[]>, InternalServerError>>(static x => TypedResults.Ok(x))
               .OkOr(TypedResults.InternalServerError());
    }

    public sealed class StatisticsClassificationsDto {
        public required string Username { get; init; }
        public required long UserId { get; init; }
        public required bool CoreDeveloper { get; init; }
        public required bool Developer { get; init; }
        public required bool Contributor { get; init; }
        public required bool Supporter { get; init; }

        public static StatisticsClassificationsDto From(IViewUserInfoUserClassificationsReadonly value) {
            return new StatisticsClassificationsDto {
                Username = value.Username,
                UserId = value.UserId,
                CoreDeveloper = value.CoreDeveloper,
                Developer = value.Developer,
                Contributor = value.Contributor,
                Supporter = value.Supporter,
            };
        }
    }
}