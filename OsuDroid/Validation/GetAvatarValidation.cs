using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class GetAvatarValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<GetAvatar>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerGetWrapper<GetAvatar> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Get is { Id: >= 0, Size: >= 1 }));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<GetAvatar> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}