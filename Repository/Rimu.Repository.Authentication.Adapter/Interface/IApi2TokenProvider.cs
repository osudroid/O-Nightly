using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Dto;

namespace Rimu.Repository.Authentication.Adapter.Interface;

/// <summary>
/// Provides functionality for managing API tokens, including creation, updating, validation, and deletion.
/// </summary>
public interface IApi2TokenProvider {
    /// <summary>
    /// Creates a new token for the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the token is created.</param>
    /// <returns>A <see cref="TokenWithTTLDto"/> containing the generated token and its TTL.</returns>
    TokenWithTTLDto CreateToken(long userId);

    /// <summary>
    /// Creates a new token for the specified user ID and inserts it into the database.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the token is created.</param>
    /// <returns>A <see cref="ResultOk{T}"/> containing the created token or an error result.</returns>
    Task<ResultOk<TokenWithTTLDto>> CreateTokenAndInsertAsync(long userId);

    /// <summary>
    /// Updates the TTL of an existing token.
    /// </summary>
    /// <param name="userToken">The token to update.</param>
    /// <returns>A <see cref="TokenWithTTLDto"/> with the updated TTL.</returns>
    TokenWithTTLDto UpdateToken(TokenWithTTLDto userToken);

    /// <summary>
    /// Finds a token by its value and checks if it is valid.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A <see cref="ResultOk{T}"/> containing the token if valid, or an empty result if invalid.</returns>
    Task<ResultOk<Option<TokenWithTTLDto>>> FindAndIsValidAsync(string token);

    /// <summary>
    /// Updates the TTL of a token and updates the token in the database.
    /// </summary>
    /// <param name="userToken">The token to update.</param>
    /// <returns>A <see cref="ResultOk{T}"/> containing the updated token or an error result.</returns>
    Task<ResultOk<TokenWithTTLDto>> UpdateTokenAndUpdateInDbAsync(TokenWithTTLDto userToken);

    /// <summary>
    /// Deletes the specified token from the database.
    /// </summary>
    /// <param name="userToken">The token to be deleted.</param>
    /// <returns>
    /// A <see cref="ResultNone"/> indicating the success or failure of the delete operation.
    /// </returns>
    Task<ResultNone> DeleteTokenInDbAsync(TokenWithTTLDto userToken);
}