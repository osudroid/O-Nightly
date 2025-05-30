using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Authentication.Domain.Token;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.Authentication.Domain;

/// <summary>
/// Provides functionality for managing API tokens, including creation, updating, validation, and deletion.
/// </summary>
public class Api2TokenProvider: IApi2TokenProvider {
    private readonly IQueryTokenUser _queryTokenUser;
    private readonly IEnvDb _envDb;
    private TokenGenerator? _tokenGenerator;

    private TokenGenerator TokenGenerator => _tokenGenerator ??= new TokenGenerator(_envDb.TokenUser_TTL);

    public Api2TokenProvider(IQueryTokenUser queryTokenUser, IEnvDb envDb) {
        _queryTokenUser = queryTokenUser;
        _envDb = envDb;
    }


    /// <summary>
    /// Creates a new token for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the token is created.</param>
    /// <returns>A <see cref="TokenWithTTLDto"/> containing the generated token and its TTL.</returns>
    public TokenWithTTLDto CreateToken(long userId) => TokenGenerator.Generate(userId);

    /// <summary>
    /// Creates a new token for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the token is created.</param>
    /// <returns>A <see cref="TokenWithTTLDto"/> containing the generated token and its TTL.</returns>
    public async Task<ResultOk<TokenWithTTLDto>> CreateTokenAndInsertAsync(long userId) {
        var createToken = CreateToken(userId);
        var resultNone = await _queryTokenUser.CreateOrUpdateAsync(createToken.CreateTime, createToken.UserId, createToken.Token);
        return resultNone == EResult.Err 
                ? ResultOk<TokenWithTTLDto>.Err() 
                : ResultOk<TokenWithTTLDto>.Ok(createToken)
            ;
    }

    /// <summary>
    /// Updates the TTL of an existing token.
    /// </summary>
    /// <param name="userToken">The token to update.</param>
    /// <returns>A <see cref="TokenWithTTLDto"/> with the updated TTL.</returns>
    public TokenWithTTLDto UpdateToken(TokenWithTTLDto userToken) => TokenGenerator.UpdateTTL(userToken);
    
    /// <summary>
    /// Finds a token by its value and checks if it is valid.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A <see cref="ResultOk{T}"/> containing the token if valid, or an empty result if invalid.</returns>
    public async Task<ResultOk<Option<TokenWithTTLDto>>> FindAndIsValidAsync(string token) {
        return (await _queryTokenUser.GetByTokenAsync(token))
            .Map(x => x
                .Map(x => TokenGenerator.Create(x.TokenId, x.UserId, x.CreateDate))
            );
    }

    /// <summary>
    /// Creates a new token for the specified user ID and inserts it into the database.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the token is created.</param>
    /// <returns>A <see cref="ResultOk{T}"/> containing the created token or an error result.</returns>
    public async Task<ResultOk<TokenWithTTLDto>> UpdateTokenAndUpdateInDbAsync(TokenWithTTLDto userToken) {
        var userTokenWithNewTTL = UpdateToken(userToken);
        var resultNone = await _queryTokenUser.CreateOrUpdateAsync(userTokenWithNewTTL.CreateTime, userTokenWithNewTTL.UserId, userTokenWithNewTTL.Token);
        return resultNone == EResult.Err 
            ? ResultOk<TokenWithTTLDto>.Err() 
            : ResultOk<TokenWithTTLDto>.Ok(userTokenWithNewTTL)
        ;
    }

    /// <summary>
    /// Deletes the specified token from the database.
    /// </summary>
    /// <param name="userToken">The token to be deleted.</param>
    /// <returns>
    /// A <see cref="ResultNone"/> indicating the success or failure of the delete operation.
    /// </returns>
    public async Task<ResultNone> DeleteTokenInDbAsync(TokenWithTTLDto userToken) {
        return await _queryTokenUser.DeleteByTokenIdAsync(userToken.Token);
    }
}