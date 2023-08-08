using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class UpdateAvatarValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostUpdateAvatar>>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostUpdateAvatar>> input) {
        var check = input.Post;

        return ValueTask.FromResult(Result<bool, string>.Ok(check.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger,
        ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostUpdateAvatar>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}