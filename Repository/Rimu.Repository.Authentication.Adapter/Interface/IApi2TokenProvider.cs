using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Dto;

namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IApi2TokenProvider {
    public TokenWithTTLDto CreateToken(long userId);
    public Task<ResultOk<TokenWithTTLDto>> CreateTokenAndInsertAsync(long userId);
    public TokenWithTTLDto UpdateToken(TokenWithTTLDto userToken);
    
    public Task<ResultOk<Option<TokenWithTTLDto>>> FindAndIsValidAsync(string token);
    public Task<ResultOk<TokenWithTTLDto>> UpdateTokenAndUpdateInDbAsync(TokenWithTTLDto userToken);
    public Task<ResultNone> DeleteTokenInDbAsync(TokenWithTTLDto userToken);
}