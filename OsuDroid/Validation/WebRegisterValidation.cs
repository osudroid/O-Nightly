using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class WebRegisterValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostWebRegister>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<PostWebRegister> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<PostWebRegister> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}