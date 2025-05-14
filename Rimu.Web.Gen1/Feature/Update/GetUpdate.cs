using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Environment.Adapter.Interface;

namespace Rimu.Web.Gen1.Feature.Update;

public sealed class GetUpdate: FastEndpoints.Endpoint<
    GetUpdate.GetUpdateRequest, 
    Results<Ok<GetUpdate.ChangeLogs>, NotFound, BadRequest<string>, InternalServerError>
> {
    // ReSharper disable once UnusedMember.Local
    private new static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvProvider _envProvider;

    public GetUpdate(IEnvProvider envProvider) {
        _envProvider = envProvider;
    }

    public override void Configure() {
        this.Get("/api/update.php");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<GetUpdate.ChangeLogs>, NotFound, BadRequest<string>, InternalServerError>> ExecuteAsync(
        GetUpdateRequest req, 
        CancellationToken ct) {
        
        var envDbFluid = await _envProvider.GetEnvDbFluid();
        var files = Directory
                    .GetFiles(envDbFluid.ChangeLogs_Path)
                    .Select<string, (string fullPath, string lang)>(static x => (x, System.IO.Path.GetFileName(x).Split('.')[0]))
                    .ToArray();



        var file = files.Any(x => x.lang == req.Lang)
                ? files.FirstOrDefault(x => x.lang == req.Lang).fullPath
                : files.FirstOrDefault(x => x.lang == "en").fullPath
            ;
        
        return TypedResults.Ok(new GetUpdate.ChangeLogs {
                link = envDbFluid.ChangeLogs_UpdateUrl,
                version_code = envDbFluid.ChangeLogs_Version,
                changelog = await System.IO.File.ReadAllTextAsync(file, ct)
            }
        );
    }

    public sealed class GetUpdateRequest {
        [FromForm(Name = "lang")] public string? Lang { get; set; }
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ChangeLogs {
        public required string version_code { get; set; }
        public required string link { get; set; }
        public required string changelog { get; set; }
    }
}