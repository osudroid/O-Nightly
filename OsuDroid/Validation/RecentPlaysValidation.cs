using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class RecentPlaysValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostRecentPlays>>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostRecentPlays>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostRecentPlays>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}