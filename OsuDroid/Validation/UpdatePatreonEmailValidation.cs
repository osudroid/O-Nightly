using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class UpdatePatreonEmailValidation 
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>>> {
    public ValueTask<Result<bool, string>> Validate(
        NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(
        LogWrapper logger, ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostUpdatePatreonEmail>> input) {
        
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}