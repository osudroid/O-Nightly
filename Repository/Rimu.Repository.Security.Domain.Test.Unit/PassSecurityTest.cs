using FluentAssertions;
namespace Rimu.Repository.Security.Domain.Test.Unit;

public class PassSecurityTest {
    private readonly PassSecurity _handler;

    public PassSecurityTest() {
        _handler = new PassSecurity();
    }

    [Test]
    public void Api2HashValidate_Work() {
        _handler.Api2HashValidate("", "", "").Should().Be(true);
    }

    [Test]
    public void DecodeString_Work() {
        _handler.DecodeString("", "").Should().Be(true);
    }

    [Test]
    public void EncryptString_Work() {
        _handler.EncryptString("").Should().Be("");
    }

    [Test]
    public void SignRequest_Work() {
        var s = _handler.SignRequest("", "");
        s.Should().Be("");
    }

    [Test]
    public void CheckRequest_Work() {
        _handler.CheckRequest("", "").Should().Be(true);
    }

    [Test]
    public void CheckApiKey_Work() {
        _handler.CheckApiKey("").Should().Be(true);
    }

    [Test]
    public void CheckIfApiKeyIsRoot_Work() {
        _handler.CheckIfApiKeyIsRoot("").Should().Be(true);
    }

    [Test]
    public void CheckSudo_Work() {
        _handler.CheckSudo("", "asd").Should().Be(true);
    }
}