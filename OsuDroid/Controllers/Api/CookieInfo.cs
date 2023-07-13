using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.View;
using OsuDroidLib.Query;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;
using DtoMapper = OsuDroidMediator.DtoMapper;
using EModelResult = OsuDroidMediator.Domain.Model.EModelResult;

namespace OsuDroid.Controllers.Api;

public sealed class CookieInfo : ControllerExtensions {
    [HttpGet("/api/user-info-by-cookie")]
    [PrivilegeRoute(route: "/api/user-info-by-cookie")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserInfoByCookie() {
        ITransaction<OptionResponse<CookieCookieUserInfoDto>> transaction = await OsuDroidMediator.Application.Service
            .CookieInfoService.GetUserInfoByCookieHandlerAsync(this.ControllerHandlerBuild());

        return HandelResultData(transaction, value => {
            if (value.Data.IsNotSet()) {
                return ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>.NotExist();
            }

            var dto = value.Data.Unwrap();
                
            return ApiTypes.ViewExistOrFoundInfo<ViewUserInfo>.Exist(
                OsuDroid.Lib.DtoMapper.ToViewUserInfo(dto));
        });
    }
}