using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class WebLoginWithUsernameValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<PostWebLoginWithUsername>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerPostWrapper<PostWebLoginWithUsername> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger,
        ControllerPostWrapper<PostWebLoginWithUsername> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}