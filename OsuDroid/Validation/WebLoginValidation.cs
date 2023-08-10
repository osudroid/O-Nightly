using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class
    WebLoginValidation : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerPostWrapper<PostWebLogin>> {
    public ValueTask<Result<bool, string>> Validate(
        NpgsqlCreates.DbWrapper db,
        LogWrapper logger,
        ControllerPostWrapper<PostWebLogin> input) {
        return new ValueTask<Result<bool, string>>(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<PostWebLogin> input) {
        return new ValueTask<Result<bool, string>>(Result<bool, string>.Ok(true));
    }
}