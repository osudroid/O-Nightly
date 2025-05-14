using Microsoft.AspNetCore.Http;
using FakeItEasy;
using FluentAssertions;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Domain.Token;
using Rimu.Repository.Postgres.Adapter.Entities;

namespace Rimu.Repository.Authentication.Domain.Test.Unit.Token;

public class TokenGeneratorTest {
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(1);
    private static readonly long UserId = 42;
    private readonly TokenGenerator _handler;
    
    public TokenGeneratorTest() {
        _handler = new TokenGenerator(Ttl);
    }
    
    [Test]
    public void Generate() {
        TokenWithTTLDto res = this._handler.Generate(UserId);
        res.UserId.Should().Be(UserId);
        res.IsEndless.Should().BeFalse();
        res.Ttl.Should().Be(Ttl);
    }

    [Test]
    public void UpdateTTL() {
        TokenWithTTLDto res = this._handler.Generate(UserId);
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var resUpdate = _handler.UpdateTTL(res);
        
        resUpdate.CreateTime.Ticks.Should().BeGreaterThan(res.CreateTime.Ticks);
    }

    [Test]
    public void Create() {
        var now = DateTime.UtcNow;
        var res = _handler.Create("aaa", UserId, now);
        res.UserId.Should().Be(UserId);
        res.IsEndless.Should().BeFalse();
        res.Ttl.Should().Be(Ttl);
        res.CreateTime.Ticks.Should().BeLessThanOrEqualTo(now.Ticks);
    }
}