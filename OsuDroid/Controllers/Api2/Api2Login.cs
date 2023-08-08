using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Post;
using OsuDroid.View;
using OsuDroid.OutputHandler;
using OsuDroid.Validation;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace OsuDroid.Controllers.Api2 {
    public class Api2Login : ControllerExtensions {
        [HttpPost("/api2/token-create")]
        [PrivilegeRoute(route: "/api2/token-create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewCreateApi2TokenResult>))]
        public async Task<IActionResult> CreateApi2TokenAsync(
            [FromBody] PostApi.PostApi2GroundNoHeader<PostCreateApi2Token> prop) {
        
        
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                Class.LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateApi2Token>>, 
                ControllerPostWrapper<CreateApi2TokenDto>, 
                OptionHandlerOutput<ViewCreateApi2TokenResult>, 
                ApiTypes.ViewExistOrFoundInfo<ViewCreateApi2TokenResult>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new CreateApi2TokenValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateApi2Token>>,
                    ControllerPostWrapper<CreateApi2TokenDto>>((i) 
                    => new ControllerPostWrapper<CreateApi2TokenDto>(i.Controller, DtoMapper.CreateApi2TokenToDto(i.Post.Body!))),
                handler: new Handler.CreateApi2TokenHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewCreateApi2TokenResult>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateApi2Token>>(this.ControllerHandlerBuild(), prop)
            );
        
            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/token-refresh")]
        [PrivilegeRoute(route: "/api2/token-refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshApi2TokenAsync(
            [FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
        
            Transaction<ApiTypes.ViewWork> transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>, 
                ControllerPostWrapper<SimpleTokenDto>, 
                WorkHandlerOutput, 
                ApiTypes.ViewWork>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new SimpleTokenValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                    ControllerPostWrapper<SimpleTokenDto>>((i) 
                    => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
                handler: new Handler.RefreshApi2TokenHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(this.ControllerHandlerBuild(), prop)
            );
            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/token-remove")]
        [PrivilegeRoute(route: "/api2/token-remove")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveApi2Token([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
            Transaction<ApiTypes.ViewWork> transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>, 
                ControllerPostWrapper<SimpleTokenDto>, 
                WorkHandlerOutput, 
                ApiTypes.ViewWork>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new SimpleTokenValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                    ControllerPostWrapper<SimpleTokenDto>>((i) 
                    => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
                handler: new Handler.RemoveApi2TokenHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(this.ControllerHandlerBuild(), prop)
            );
            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/token-user-id")]
        [PrivilegeRoute(route: "/api2/token-user-id")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<long>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTokenUserId([FromBody] PostApi.PostApi2GroundNoHeader<PostSimpleToken> prop) {
            var transaction = await OsuDroidAttachment.Service.AttachmentServiceApi<
                OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper, 
                LogWrapper, 
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>, 
                ControllerPostWrapper<SimpleTokenDto>, 
                OptionHandlerOutput<long>,
                ApiTypes.ViewExistOrFoundInfo<long>>(
            
                dbCreates: new OsuDroidAttachment.DbBuilder.NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new SimpleTokenValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>,
                    ControllerPostWrapper<SimpleTokenDto>>((i) 
                    => new ControllerPostWrapper<SimpleTokenDto>(i.Controller, DtoMapper.SimpleTokenToDto(i.Post.Body!))),
                handler: new Handler.GetTokenUserIdHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<long>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>(this.ControllerHandlerBuild(), prop)
            );
            return TransactionToIResult(transaction);
        }
    }
}
