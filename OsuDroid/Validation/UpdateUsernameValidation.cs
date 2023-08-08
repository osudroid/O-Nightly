using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class UpdateUsernameValidation 
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdateUsername>>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostUpdateUsername>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostUpdateUsername>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}