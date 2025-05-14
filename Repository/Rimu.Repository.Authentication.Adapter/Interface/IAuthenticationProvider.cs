using LamLibAllOver.ErrorHandling;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IAuthenticationProvider {
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUsername(string username);
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByUserId(long userId);
    public Task<ResultOk<Option<IUserAuthContext>>> GetUserAuthContextByEmail(string email);
    
    public IPasswordProvider PasswordGen1Provider { get; } 
    public IPasswordProvider PasswordGen2Provider { get; } 
}