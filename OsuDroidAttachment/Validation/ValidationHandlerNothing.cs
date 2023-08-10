using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Validation;

public class ValidationHandlerNothing<TDb, ELogger, UInput> : IValidationHandler<TDb, ELogger, UInput>
    where UInput : IInput
    where ELogger : ILogger
    where TDb : IDb {
    public ValueTask<Result<bool, string>> Validate(TDb db, ELogger logger, UInput input) {
        return new ValueTask<Result<bool, string>>(Result<bool, string>.Ok(true));
    }

    public ValueTask<Result<bool, string>> HashMatch(ELogger logger, UInput input) {
        return new ValueTask<Result<bool, string>>(Result<bool, string>.Ok(true));
    }
}