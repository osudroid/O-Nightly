using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public struct UserIdBoxValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<UserIdBox>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerGetWrapper<UserIdBox> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Get.UserId >= 0));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<UserIdBox> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}