using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Update;

public sealed class GetUpdateChangeLogs: Endpoint<GetUpdateChangeLogs.UpdateLanguageInfoRequest,
    Results<Ok<GetUpdateChangeLogs.UpdateLanguageInfoDto>, InternalServerError, NotFound>
> {
    private IEnvProvider _envProvider;

    public GetUpdateChangeLogs(IEnvProvider envProvider) {
        _envProvider = envProvider;
    }

    public override void Configure() {
        Get("/api2/update-version/change-logs");
        this.AllowAnonymous();
        this.ResponseCache(120);
    }

    public override async Task<Results<Ok<UpdateLanguageInfoDto>, InternalServerError, NotFound>>
        ExecuteAsync(UpdateLanguageInfoRequest req, CancellationToken ct) {

        var envDbFluid = await _envProvider.GetEnvDbFluid();

        var dirNameNumber = Directory
                            .GetDirectories(envDbFluid.ChangeLogs_Path)
                            .Select(long.Parse)
                            .MaxBy(static x => x);
        var changeLogsNamesFiles = Directory.GetFiles($"{envDbFluid.ChangeLogs_Path}/{dirNameNumber}");

        var filenameOption = Option<string>.NullSplit(changeLogsNamesFiles.Any(x => x == req.LanguageName)
            ? req.LanguageName
            : changeLogsNamesFiles.SingleOrDefault(static x => x == "en"));

        if (filenameOption.IsNotSet()) {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new UpdateLanguageInfoDto {
                Link = envDbFluid.ChangeLogs_UpdateUrl,
                VersionCode = envDbFluid.ChangeLogs_Version,
                Changelog = await System.IO.File.ReadAllTextAsync($"{envDbFluid.ChangeLogs_Path}/{dirNameNumber}/{req.LanguageName}", ct)
            }
        );
    }


    public sealed class UpdateLanguageInfoRequest {
        [FromRoute(Name = "lang")]
        public string LanguageName { get; set; } = "";
    }
    
    public sealed class UpdateLanguageInfoDto {
        public required string VersionCode { get; set; }
        public required string Link { get; set; }
        public required string Changelog { get; set; }
    }
}