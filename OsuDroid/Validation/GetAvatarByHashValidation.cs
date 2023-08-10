using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class GetAvatarByHashValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<GetAvatarWithHash>> {
    public ValueTask<Result<bool, string>> Validate(
        NpgsqlCreates.DbWrapper db,
        LogWrapper logger,
        ControllerGetWrapper<GetAvatarWithHash> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(!string.IsNullOrEmpty(input.Get.Hash.Trim())));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<GetAvatarWithHash> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}