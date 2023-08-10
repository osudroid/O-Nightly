using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation;

public class TopPlaysByMarkPageingValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerGetWrapper<GetTopPlaysByMarkPageing>> {
    public ValueTask<Result<bool, string>> Validate(
        NpgsqlCreates.DbWrapper db,
        LogWrapper logger,
        ControllerGetWrapper<GetTopPlaysByMarkPageing> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Get is {
                    MarkString: not null,
                    Page: >= 0,
                    UserId: >= 0
                }
            )
        );
    }

    public ValueTask<Result<bool, string>> HashMatch(
        LogWrapper logger,
        ControllerGetWrapper<GetTopPlaysByMarkPageing> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}