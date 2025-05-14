using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.Feature.Statistics;
using Rimu.Web.Gen2.Interface;
using Rimu.Web.Gen2.PreProcessor;
using Rimu.Web.Gen2.Share.Submit;

namespace Rimu.Web.Gen2.Feature.Submit;

public sealed class PostSubmitPlayStart: FastEndpoints.Endpoint<HashWithDataRequest<PostSubmitPlayStart.SubmitPlayStartRequest>,
    Results<Ok<Repository.Postgres.Adapter.Entities.Play>, InternalServerError>
>  {
    private readonly IQueryPlay _queryPlay;

    public PostSubmitPlayStart(IQueryPlay queryPlay) {
        _queryPlay = queryPlay;
    }


    public override void Configure() {
        Post("/api2/submit/play-start");
        AllowAnonymous();
        PreProcessor<UserTokenPreProcessor<HashWithDataRequest<PostSubmitPlayStart.SubmitPlayStartRequest>>>();
        PreProcessor<RequestHashValidationPreProcessor<HashWithDataRequest<SubmitPlayStartRequest>,SubmitPlayStartRequest>>();
    }

    public override async Task<Results<Ok<Repository.Postgres.Adapter.Entities.Play>, InternalServerError>> 
        ExecuteAsync(HashWithDataRequest<SubmitPlayStartRequest> req, CancellationToken ct) {
        
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;

        var play = new Repository.Postgres.Adapter.Entities.Play() {
            Id = -1,
            UserId = userId,
            FileHash = req.Data!.FileHash,
            Filename = req.Data!.Filename,
        };

        var idResult = (await _queryPlay.InsertIfNotExistAsync(play));
        if (idResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        play.Id = idResult.Ok();
        
        return TypedResults.Ok(play);
    }

    public sealed class SubmitPlayStartRequest: ISingleString {
        public string Filename { get; set; } = "";
        public string FileHash { get; set; } = "";


        public string ToSingleString() {
            return LamLibAllOver.Merge.ObjectsToString([
                Filename,
                FileHash
            ]);
        }
    }
}