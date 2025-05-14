using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

public class AuthenticationProvider: IAuthenticationProvider {
    private readonly IQueryUserInfo _queryUserInfo;

    public IPasswordProvider PasswordGen1Provider { get; init; } = new Password.PasswordGen1Provider();
    public IPasswordProvider PasswordGen2Provider { get; init; } = new Password.PasswordGen2Provider();
    
    public AuthenticationProvider(IQueryUserInfo queryUserInfo) {
        _queryUserInfo = queryUserInfo;
    }

    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUsername(string username) {
        return (await _queryUserInfo.GetByUsernameAsync(username))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }

    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUserId(long userId) {
        return (await _queryUserInfo.GetByUserIdAsync(userId))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }
    
    public async Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByEmail(string email) {
        return (await _queryUserInfo.GetByEmailAsync(email))
            .Map(x => x.Map(x => (IUserAuthContext) UserAuthContext.FromUserInfo(x, _queryUserInfo)));
    }
}