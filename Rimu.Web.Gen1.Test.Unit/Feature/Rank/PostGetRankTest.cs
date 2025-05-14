using System.Net;
using FakeItEasy;
using FastEndpoints;
using FluentAssertions;
using LamLibAllOver.ErrorHandling;
using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;
using Rimu.Repository.Security.Adapter.Interface;
using Rimu.Web.Gen1.Feature.Login;
using Rimu.Web.Gen1.Feature.Rank;
using Rimu.Web.Gen1.PreProcessor;

namespace Rimu.Web.Gen1.Test.Unit.Feature.Rank;

public class PostGetRankTest {
    private readonly PostGetRank.PostGetRankHandler _handler;
    private readonly IQueryView_Play_PlayStats _queryViewPlayPlayStats;
    private readonly IQueryView_Play_PlayStats_UserInfo _queryView_Play_PlayStats_UserInfo;
    private readonly ISecurityPhp _securityPhp;
    private readonly IEnvDb _envDb;
    private readonly HttpContext _httpContext;
    
    public PostGetRankTest() {
        _httpContext = A.Fake<HttpContext>();
        _queryViewPlayPlayStats = A.Fake<IQueryView_Play_PlayStats>();
        _queryView_Play_PlayStats_UserInfo = A.Fake<IQueryView_Play_PlayStats_UserInfo>();
        _securityPhp = A.Fake<ISecurityPhp>();
        _envDb = A.Fake<IEnvDb>();

        _handler = new PostGetRank.PostGetRankHandler(
            _httpContext,
            _queryViewPlayPlayStats, 
            _queryView_Play_PlayStats_UserInfo, 
            _securityPhp, 
            _envDb
        );
    }

    [Test]
    [TestCase("PlayId")]
    [TestCase("Sign")]
    [TestCase("UidStr")]
    [TestCase("Type")]
    public async Task PostGetRankHandler_Null(string byNull) {
        var inp = new PostGetRank.PostGetRankRequest() {
            PlayId = "2",
            Sign = "asd",
            Hash = "asdasdasd",
            UidStr = "2123",
            Type = "v",
        };

        if (byNull == "PlayId") {
            inp.Sign = null;
            inp.PlayId = null;
        }
        else if (byNull == "Sign") inp.Sign = null;
        else if (byNull == "Hash") inp.Hash = null;
        else if (byNull == "UidStr") inp.UidStr = null;
        else if (byNull == "Type") inp.Type = null;
        else throw new NotImplementedException();
        
        Results<Ok<string>, NotFound, BadRequest, InternalServerError> res = await _handler.HandleAsync(inp, default);

        res.Result.Should().BeOfType<BadRequest>();
    }
}