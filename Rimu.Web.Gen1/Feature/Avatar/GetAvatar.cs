using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Kernel;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Pp.Adapter;
using Rimu.Repository.Security.Adapter.Interface;

namespace Rimu.Web.Gen1.Feature.Avatar;

public class GetAvatar: FastEndpoints.Endpoint<
    GetAvatar.GetAvatarGen1Request, 
    Results<Ok<IResult>, NotFound, BadRequest, InternalServerError>
> {
    private readonly GetAvatarHandler _getAvatarHandler;

    public GetAvatar(GetAvatarHandler getAvatarHandler) {
        _getAvatarHandler = getAvatarHandler;
    }

    public override void Configure() {
        Get("/user/avatar");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IResult>, NotFound, BadRequest, InternalServerError>> 
        ExecuteAsync(GetAvatarGen1Request req, CancellationToken ct) {

        return await _getAvatarHandler.HandleAsync(req, ct);
    }

    public sealed class GetAvatarGen1Request {
        [FromRoute(Name = "userId")]
        public long UserId { get; set; }
    }

    public sealed class GetAvatarHandler: WebRequestHandler<
        GetAvatar.GetAvatarGen1Request, 
        Results<Ok<IResult>, NotFound, BadRequest, InternalServerError>
    > {
        private readonly IUserAvatarProvider _userAvatarProvider;
        
        public GetAvatarHandler(HttpContext httpContext, IUserAvatarProvider userAvatarProvider): base(httpContext) {
            _userAvatarProvider = userAvatarProvider;
        }


        public override async Task<Results<Ok<IResult>, NotFound, BadRequest, InternalServerError>> HandleAsync(
            GetAvatarGen1Request req, CancellationToken ct) {
            
            var userAvatarContext =  _userAvatarProvider.CreateNewContext(req.UserId);
            var userAvatarResult = await userAvatarContext.FindAvatarOriginalByUserIdAsync();
        
            if (userAvatarResult == EResult.Err) {
                return TypedResults.InternalServerError();
            }

            if (userAvatarResult.Ok().IsNotSet()) {
                return TypedResults.NotFound();
            }
            
            var imageAvatarResult = await userAvatarContext.ToPngAsync(userAvatarResult.Ok().Unwrap());
            if (imageAvatarResult == EResult.Err) {
                return TypedResults.InternalServerError();
            }

            var imageAvatar = imageAvatarResult.Ok();
        
            return TypedResults.Ok(Results.File(imageAvatar.Bytes!, imageAvatar.TypeExt));
        }
    }
}