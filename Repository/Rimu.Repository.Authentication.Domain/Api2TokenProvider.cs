using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Authentication.Domain.Token;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

public class Api2TokenProvider: IApi2TokenProvider {
    private readonly IQueryTokenUser _queryTokenUser;
    private readonly IEnvDb _envDb;
    private TokenGenerator? _tokenGenerator;

    private TokenGenerator TokenGenerator => _tokenGenerator ??= new TokenGenerator(_envDb.TokenUser_TTL);

    public Api2TokenProvider(IQueryTokenUser queryTokenUser, IEnvDb envDb) {
        _queryTokenUser = queryTokenUser;
        _envDb = envDb;
    }


    public TokenWithTTLDto CreateToken(long userId) => TokenGenerator.Generate(userId);

    public async Task<ResultOk<TokenWithTTLDto>> CreateTokenAndInsertAsync(long userId) {
        var createToken = CreateToken(userId);
        var resultNone = await _queryTokenUser.CreateOrUpdateAsync(createToken.CreateTime, createToken.UserId, createToken.Token);
        return resultNone == EResult.Err 
                ? ResultOk<TokenWithTTLDto>.Err() 
                : ResultOk<TokenWithTTLDto>.Ok(createToken)
            ;
    }

    public TokenWithTTLDto UpdateToken(TokenWithTTLDto userToken) => TokenGenerator.UpdateTTL(userToken);
    public async Task<ResultOk<Option<TokenWithTTLDto>>> FindAndIsValidAsync(string token) {
        return (await _queryTokenUser.GetByTokenAsync(token))
            .Map(x => x
                .Map(x => TokenGenerator.Create(x.TokenId, x.UserId, x.CreateDate))
            );
    }

    public async Task<ResultOk<TokenWithTTLDto>> UpdateTokenAndUpdateInDbAsync(TokenWithTTLDto userToken) {
        var userTokenWithNewTTL = UpdateToken(userToken);
        var resultNone = await _queryTokenUser.CreateOrUpdateAsync(userTokenWithNewTTL.CreateTime, userTokenWithNewTTL.UserId, userTokenWithNewTTL.Token);
        return resultNone == EResult.Err 
            ? ResultOk<TokenWithTTLDto>.Err() 
            : ResultOk<TokenWithTTLDto>.Ok(userTokenWithNewTTL)
        ;
    }

    public async Task<ResultNone> DeleteTokenInDbAsync(TokenWithTTLDto userToken) {
        return await _queryTokenUser.DeleteByTokenIdAsync(userToken.Token);
    }
}