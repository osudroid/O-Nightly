using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using DbWrapper = OsuDroidAttachment.DbBuilder.NpgsqlCreates.DbWrapper;

namespace OsuDroid.Validation; 

public class SimpleTokenValidation
    : IValidationHandler<DbWrapper, LogWrapper,ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostSimpleToken>>> {
    
    public ValueTask<Result<bool, string>> Validate(DbWrapper db, LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostSimpleToken>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostSimpleToken>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}