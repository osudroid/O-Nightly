using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.Handler;
using OsuDroid.HttpGet;
using OsuDroid.Lib;
using OsuDroid.OutputHandler;
using OsuDroid.Post;
using OsuDroid.Validation;
using OsuDroid.View;
using OsuDroidAttachment;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;

// ReSharper disable All

namespace OsuDroid.Controllers.Api2 {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class Api2Profile : ControllerExtensions {
        [HttpGet("/api2/profile/stats/{id:long}")]
        [PrivilegeRoute(route: "/api2/profile/stats/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewProfileStats>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WebProfileStats([FromRoute(Name = "id")] long userId) {
            var prop = new UserIdBox() { UserId = userId };


            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<UserIdBox>,
                ControllerGetWrapper<UserIdBoxDto>,
                OptionHandlerOutput<ViewProfileStats>, ApiTypes.ViewExistOrFoundInfo<ViewProfileStats>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UserIdBoxValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<UserIdBox>,
                    ControllerGetWrapper<UserIdBoxDto>>((i)
                    => new ControllerGetWrapper<UserIdBoxDto>(i.Controller, DtoMapper.UserIdBoxToDto(i.Get))
                ),
                handler: new WebProfileStatsHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewProfileStats>(),
                input: new ControllerGetWrapper<UserIdBox>(this.ControllerHandlerBuild(), prop)
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/stats/timeline/{id:long}")]
        [PrivilegeRoute(route: "/api2/profile/stats/timeline/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserRankTimeLine>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> WebProfileStatsTimeLine([FromRoute(Name = "id")] long userId) {
            var prop = new UserIdBox() { UserId = userId };

            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<UserIdBox>,
                ControllerGetWrapper<UserIdBoxDto>,
                OptionHandlerOutput<ViewUserRankTimeLine>, ApiTypes.ViewExistOrFoundInfo<ViewUserRankTimeLine>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UserIdBoxValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<UserIdBox>,
                    ControllerGetWrapper<UserIdBoxDto>>((i)
                    => new ControllerGetWrapper<UserIdBoxDto>(i.Controller, DtoMapper.UserIdBoxToDto(i.Get))
                ),
                handler: new WebProfileStatsTimeLineHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewUserRankTimeLine>(),
                input: new ControllerGetWrapper<UserIdBox>(this.ControllerHandlerBuild(), prop)
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/topplays/{id:long}")]
        [PrivilegeRoute(route: "/api2/profile/topplays/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlays>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WebProfileTopPlays([FromRoute(Name = "id")] long userId) {
            var prop = new UserIdBox() { UserId = userId };

            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<UserIdBox>,
                ControllerGetWrapper<UserIdBoxDto>,
                OptionHandlerOutput<ViewPlays>, ApiTypes.ViewExistOrFoundInfo<ViewPlays>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UserIdBoxValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<UserIdBox>,
                    ControllerGetWrapper<UserIdBoxDto>>((i)
                    => new ControllerGetWrapper<UserIdBoxDto>(i.Controller, DtoMapper.UserIdBoxToDto(i.Get))
                ),
                handler: new WebProfileTopPlaysHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlays>(),
                input: new ControllerGetWrapper<UserIdBox>(this.ControllerHandlerBuild(), prop)
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/topplays/{id:long}/page/{page:int}")]
        [PrivilegeRoute(route: "/api2/profile/topplays/{id:long}/page/{page:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlays>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WebProfileTopPlaysPage([FromRoute(Name = "id")] long userId, int page) {
            var prop = new TopPlaysPageing() { Page = page, UserId = userId };

            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<TopPlaysPageing>,
                ControllerGetWrapper<TopPlaysPageingDto>,
                OptionHandlerOutput<ViewPlays>, ApiTypes.ViewExistOrFoundInfo<ViewPlays>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new TopPlaysPageingValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<TopPlaysPageing>,
                    ControllerGetWrapper<TopPlaysPageingDto>>((i)
                    => new ControllerGetWrapper<TopPlaysPageingDto>(i.Controller,
                        DtoMapper.TopPlaysPageingToDto(i.Get)
                    )
                ),
                handler: new WebProfileTopPlaysPageHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlays>(),
                input: new ControllerGetWrapper<TopPlaysPageing>(this.ControllerHandlerBuild(), prop)
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/recentplays/{id:long}")]
        [PrivilegeRoute(route: "/api2/profile/recentplays/{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlays>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WebProfileTopRecent([FromRoute(Name = "id")] long userId) {
            var prop = new UserIdBox() { UserId = userId };

            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<UserIdBox>,
                ControllerGetWrapper<UserIdBoxDto>,
                OptionHandlerOutput<ViewPlays>, ApiTypes.ViewExistOrFoundInfo<ViewPlays>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UserIdBoxValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<UserIdBox>,
                    ControllerGetWrapper<UserIdBoxDto>>((i)
                    => new ControllerGetWrapper<UserIdBoxDto>(i.Controller, DtoMapper.UserIdBoxToDto(i.Get))
                ),
                handler: new WebProfileTopRecentHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlays>(),
                input: new ControllerGetWrapper<UserIdBox>(this.ControllerHandlerBuild(), prop)
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/update/email")]
        [PrivilegeRoute(route: "/api2/profile/update/email")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmail([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateEmail> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateEmail>>,
                ControllerPostWrapper<UpdateEmailDto>,
                WorkHandlerOutput,
                ApiTypes.ViewWork>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UpdateEmailValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateEmail>>,
                    ControllerPostWrapper<UpdateEmailDto>>((i)
                    => new ControllerPostWrapper<UpdateEmailDto>(i.Controller,
                        DtoMapper.UpdateEmailToDto(i.Post.Body)
                    )
                ),
                handler: new UpdateEmailHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateEmail>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/update/passwd")]
        [PrivilegeRoute(route: "/api2/profile/update/passwd")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>
            UpdatePasswd([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdatePasswd> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePasswd>>,
                ControllerPostWrapper<UpdatePasswdDto>,
                WorkHandlerOutput,
                ApiTypes.ViewWork>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UpdatePasswordValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePasswd>>,
                    ControllerPostWrapper<UpdatePasswdDto>>((i)
                    => new ControllerPostWrapper<UpdatePasswdDto>(i.Controller,
                        DtoMapper.UpdatePasswdToDto(i.Post.Body!)
                    )
                ),
                handler: new UpdatePasswordHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePasswd>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/update/username")]
        [PrivilegeRoute(route: "/api2/profile/update/username")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUpdateUsernameRes>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>
            UpdateUsername([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateUsername> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateUsername>>,
                ControllerPostWrapper<UpdateUsernameDto>,
                OptionHandlerOutput<ViewUpdateUsernameRes>, ApiTypes.ViewExistOrFoundInfo<ViewUpdateUsernameRes>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UpdateUsernameValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateUsername>>,
                    ControllerPostWrapper<UpdateUsernameDto>>((i)
                    => new ControllerPostWrapper<UpdateUsernameDto>(i.Controller,
                        DtoMapper.UpdateUsernameToDto(i.Post.Body!)
                    )
                ),
                handler: new UpdateUsernameHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewUpdateUsernameRes>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateUsername>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/update/avatar")]
        [PrivilegeRoute(route: "/api2/profile/update/avatar")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUpdateAvatar>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>
            UpdateAvatar([FromBody] PostApi.PostApi2GroundNoHeader<PostUpdateAvatar> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateAvatar>>,
                ControllerPostWrapper<UpdateAvatarDto>,
                OptionHandlerOutput<ViewUpdateAvatar>, ApiTypes.ViewExistOrFoundInfo<ViewUpdateAvatar>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UpdateAvatarValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateAvatar>>,
                    ControllerPostWrapper<UpdateAvatarDto>>((i)
                    => new ControllerPostWrapper<UpdateAvatarDto>(i.Controller,
                        DtoMapper.UpdateAvatarToDto(i.Post.Body!)
                    )
                ),
                handler: new UpdateAvatarHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewUpdateAvatar>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateAvatar>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/update/patreonemail")]
        [PrivilegeRoute(route: "/api2/profile/update/patreonemail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePatreonEmail(
            [FromBody] PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>>,
                ControllerPostWrapper<UpdatePatreonEmailDto>,
                WorkHandlerOutput,
                ApiTypes.ViewWork>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UpdatePatreonEmailValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>>,
                    ControllerPostWrapper<UpdatePatreonEmailDto>>((i)
                    => new ControllerPostWrapper<UpdatePatreonEmailDto>(i.Controller,
                        DtoMapper.UpdatePatreonEmailToDto(i.Post.Body!)
                    )
                ),
                handler: new UpdatePatreonEmaildHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/accept/patreonemail/token/{token:guid}")]
        [PrivilegeRoute(route: "/api2/profile/accept/patreonemail/token/{token:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptPatreonEmail([FromRoute(Name = "token")] Guid token) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<Guid>,
                ControllerGetWrapper<Guid>,
                WorkHandlerOutput,
                ApiTypes.ViewWork>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new GuidValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<Guid>,
                    ControllerGetWrapper<Guid>>((i)
                    => new ControllerGetWrapper<Guid>(i.Controller, i.Get)
                ),
                handler: new AcceptPatreonEmailHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerGetWrapper<Guid>(this.ControllerHandlerBuild(), token)
            );

            return TransactionToIResult(transaction);
        }

        [HttpPost("/api2/profile/drop-account/sendMail")]
        [PrivilegeRoute(route: "/api2/profile/drop-account/sendMail}")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewCreateDropAccountTokenRes>)
        )]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateDropAccountToken(
            [FromBody] PostApi.PostApi2GroundNoHeader<PostCreateDropAccountToken> prop) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateDropAccountToken>>,
                ControllerPostWrapper<CreateDropAccountTokenDto>,
                OptionHandlerOutput<ViewCreateDropAccountTokenRes>,
                ApiTypes.ViewExistOrFoundInfo<ViewCreateDropAccountTokenRes>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new CreateDropAccountTokenValidation(),
                transformHandler: new TransformAction<
                    ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateDropAccountToken>>,
                    ControllerPostWrapper<CreateDropAccountTokenDto>>((i)
                    => new ControllerPostWrapper<CreateDropAccountTokenDto>(i.Controller,
                        DtoMapper.CreateDropAccountTokenToDto(i.Post.Body!)
                    )
                ),
                handler: new CreateDropAccountTokenHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewCreateDropAccountTokenRes>(),
                input: new ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostCreateDropAccountToken>>(
                    this.ControllerHandlerBuild(), prop
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet("/api2/profile/drop-account/token/{token:guid}")]
        [PrivilegeRoute(route: "/api2/profile/drop-account/token/{token:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewWork))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DropAccountWithTokenAsync([FromRoute(Name = "token")] Guid token) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<Guid>,
                ControllerPostWrapper<Guid>,
                WorkHandlerOutput,
                ApiTypes.ViewWork>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new GuidValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<Guid>,
                    ControllerPostWrapper<Guid>>((i)
                    => new ControllerPostWrapper<Guid>(i.Controller, i.Get)
                ),
                handler: new DropAccountWithTokenAsyncHandler(),
                outputHandler: new WorkHandler(),
                input: new ControllerGetWrapper<Guid>(this.ControllerHandlerBuild(), token)
            );

            return TransactionToIResult(transaction);
        }


        [HttpGet("/api2/profile/top-play-by-marks-length/user-id/{userId:long}")]
        [PrivilegeRoute(route: "/api2/profile/top-play-by-marks-length/user-id/{userId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK,
            Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlaysMarksLength>)
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> WebProfileTopPlaysByMarksLength([FromRoute] long userId) {
            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<UserIdBox>,
                ControllerGetWrapper<UserIdBox>,
                OptionHandlerOutput<ViewPlaysMarksLength>, ApiTypes.ViewExistOrFoundInfo<ViewPlaysMarksLength>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new UserIdBoxValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<UserIdBox>,
                    ControllerGetWrapper<UserIdBox>>((i)
                    => new ControllerGetWrapper<UserIdBox>(i.Controller, i.Get)
                ),
                handler: new WebProfileTopPlaysByMarksLengthHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlaysMarksLength>(),
                input: new ControllerGetWrapper<UserIdBox>(this.ControllerHandlerBuild(),
                    new UserIdBox() { UserId = userId }
                )
            );

            return TransactionToIResult(transaction);
        }

        [HttpGet(
            "/api2/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}"
        )]
        [PrivilegeRoute(
            route:
            "/api2/profile/top-play-by-marks-length/user-id/{userId:long}/mark/{markString:alpha}/page/{page:int}"
        )]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewPlays>))]
        public async Task<IActionResult> WebProfileTopPlaysByMark(
            [FromRoute] long userId,
            [FromRoute] string markString,
            [FromRoute] int page) {
            var request = new HttpGet.GetTopPlaysByMarkPageing() {
                Page = page,
                MarkString = markString,
                UserId = userId
            };

            var transaction = await Service.AttachmentServiceApi<
                NpgsqlCreates.DbWrapper,
                Class.LogWrapper,
                ControllerGetWrapper<HttpGet.GetTopPlaysByMarkPageing>,
                ControllerGetWrapper<TopPlaysByMarkPageingDto>,
                OptionHandlerOutput<ViewPlays>, ApiTypes.ViewExistOrFoundInfo<ViewPlays>>(
                dbCreates: new NpgsqlCreates(),
                loggerCreates: new Class.LogCreates(),
                validationHandler: new TopPlaysByMarkPageingValidation(),
                transformHandler: new TransformAction<
                    ControllerGetWrapper<HttpGet.GetTopPlaysByMarkPageing>,
                    ControllerGetWrapper<TopPlaysByMarkPageingDto>>((i)
                    => new ControllerGetWrapper<TopPlaysByMarkPageingDto>(i.Controller,
                        DtoMapper.TopPlaysByMarkPageingToDto(i.Get)
                    )
                ),
                handler: new WebProfileTopPlaysByMarkHandler(),
                outputHandler: new ViewExistOrFoundInfoHandler<ViewPlays>(),
                input: new ControllerGetWrapper<HttpGet.GetTopPlaysByMarkPageing>(this.ControllerHandlerBuild(),
                    request
                )
            );

            return TransactionToIResult(transaction);
        }
    }
}