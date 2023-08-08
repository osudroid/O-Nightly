using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class SimpleTokenValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostSimpleToken>>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostSimpleToken>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostSimpleToken>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}