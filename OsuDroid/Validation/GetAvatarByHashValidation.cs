using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class GetAvatarByHashValidation 
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerGetWrapper<HttpGet.GetAvatarWithHash>>{
    
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerGetWrapper<HttpGet.GetAvatarWithHash> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(!string.IsNullOrEmpty(input.Get.Hash.Trim())));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerGetWrapper<HttpGet.GetAvatarWithHash> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}