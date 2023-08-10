using OsuDroid.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class GuidValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<Guid>> {
    public ValueTask<Result<bool, string>> Validate(
        NpgsqlCreates.DbWrapper db,
        LogWrapper logger,
        ControllerGetWrapper<Guid> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Get != Guid.Empty));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<Guid> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}