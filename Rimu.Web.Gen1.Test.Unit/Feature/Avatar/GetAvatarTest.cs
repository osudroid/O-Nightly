using Microsoft.AspNetCore.Http;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Web.Gen1.Feature.Avatar;
using FakeItEasy;
using FluentAssertions;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Web.Gen1.Test.Unit.Feature.Avatar;

public class GetAvatarTest {
    private readonly GetAvatar.GetAvatarHandler _getAvatarHandler = default!;
    private readonly HttpContext _httpContext;
    private IUserAvatarProvider _userAvatarProvider; 
    public GetAvatarTest() {
        _httpContext = A.Fake<HttpContext>();
        _userAvatarProvider = A.Fake<IUserAvatarProvider>();
        _getAvatarHandler = new GetAvatar.GetAvatarHandler(_httpContext, _userAvatarProvider);
    }

    [Test]
    public async Task GetAvatar_UserAvatarResultError() {
        var request = new GetAvatar.GetAvatarGen1Request() { UserId = 2 };
        
        var userAvatarContext = A.Fake<IUserAvatarContext>();
        A.CallTo(() => userAvatarContext.FindAvatarOriginalByUserIdAsync())
            .Returns(Task.FromResult(ResultOk<Option<UserAvatar>>.Err()));
        A.CallTo(() => _userAvatarProvider.CreateNewContext(request.UserId))
            .Returns(userAvatarContext);
        
        
        Results<Ok<IResult>, NotFound, BadRequest, InternalServerError> response = await _getAvatarHandler.HandleAsync(request, CancellationToken.None);

        var result = response.Result;

        result.Should().BeOfType<InternalServerError>();
    }
    
    [Test]
    public async Task GetAvatar_ToPngAsyncError() {
        var request = new GetAvatar.GetAvatarGen1Request() { UserId = 2 };
        var userAvatar = new UserAvatar() {
            UserId = request.UserId,
            Hash = "Abc",
            TypeExt = null,
            PixelSize = 256,
            Animation = false,
            Bytes = [1,1,1,1,1,1,1],
            Original = true,
        }; 
        var userAvatarContext = A.Fake<IUserAvatarContext>();
        A.CallTo(() => userAvatarContext.FindAvatarOriginalByUserIdAsync())
            .Returns(Task.FromResult(ResultOk<Option<UserAvatar>>.Ok(Option<UserAvatar>.With(userAvatar))));
        A.CallTo(() => userAvatarContext.ToPngAsync(userAvatar))
            .Returns(ResultOk<UserAvatar>.Err());
        A.CallTo(() => _userAvatarProvider.CreateNewContext(request.UserId))
            .Returns(userAvatarContext);
        
        Results<Ok<IResult>, NotFound, BadRequest, InternalServerError> response = await _getAvatarHandler.HandleAsync(request, CancellationToken.None);

        var result = response.Result;

        result.Should().BeOfType<InternalServerError>();
    }
    
    [Test]
    public async Task GetAvatar_UserNotExist() {
        var request = new GetAvatar.GetAvatarGen1Request() { UserId = 2 };
        
        var userAvatarContext = A.Fake<IUserAvatarContext>();
        A.CallTo(() => userAvatarContext.FindAvatarOriginalByUserIdAsync())
            .Returns(Task.FromResult(ResultOk<Option<UserAvatar>>.Ok(Option<UserAvatar>.Empty)));
        A.CallTo(() => _userAvatarProvider.CreateNewContext(request.UserId))
            .Returns(userAvatarContext);
        
        
        Results<Ok<IResult>, NotFound, BadRequest, InternalServerError> response = await _getAvatarHandler.HandleAsync(request, CancellationToken.None);

        var result = response.Result;

        result.Should().BeOfType<NotFound>();
    }
    
    [Test]
    public async Task GetAvatar_Ok() {
        var request = new GetAvatar.GetAvatarGen1Request() { UserId = 2 };
        var userAvatar = new UserAvatar() {
            UserId = request.UserId,
            Hash = "Abc",
            TypeExt = "png",
            PixelSize = 256,
            Animation = false,
            Bytes = [1,1,1,1,1,1,1],
            Original = true,
        }; 
        var userAvatarContext = A.Fake<IUserAvatarContext>();
        A.CallTo(() => userAvatarContext.FindAvatarOriginalByUserIdAsync())
            .Returns(Task.FromResult(ResultOk<Option<UserAvatar>>.Ok(Option<UserAvatar>.With(userAvatar))));
        A.CallTo(() => userAvatarContext.ToPngAsync(userAvatar))
            .Returns(ResultOk<UserAvatar>.Ok(userAvatar));
        A.CallTo(() => _userAvatarProvider.CreateNewContext(request.UserId))
            .Returns(userAvatarContext);
        
        Results<Ok<IResult>, NotFound, BadRequest, InternalServerError> response = await _getAvatarHandler.HandleAsync(request, CancellationToken.None);

        IResult result = response.Result;
        
        result.Should().BeOfType<Ok<IResult>>();
        
        Ok<IResult> ok = (Ok<IResult>) result;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().BeOfType<FileContentHttpResult>();
    }
}