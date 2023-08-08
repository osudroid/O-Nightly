using OsuDroid.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class TopPlaysPageingValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<TopPlaysPageing>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger,
        ControllerGetWrapper<TopPlaysPageing> input) {
        var check = input.Get;
        return ValueTask.FromResult(Result<bool, string>.Ok(
            check is { UserId: >= 0, Page: >= 0 }
        ));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<TopPlaysPageing> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}