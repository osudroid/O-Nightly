using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class ResetPasswdAndSendEmailValidation: IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostResetPasswdAndSendEmail>> {
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<PostResetPasswdAndSendEmail> input) {
        return ValueTask.FromResult<Result<bool, string>>(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<PostResetPasswdAndSendEmail> input) {
        return ValueTask.FromResult<Result<bool, string>>(Result<bool, string>.Ok(true));
    }
}