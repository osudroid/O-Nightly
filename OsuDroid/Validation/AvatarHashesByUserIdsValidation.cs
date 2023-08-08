using OsuDroid.Class;
using OsuDroid.HttpGet;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class AvatarHashesByUserIdsValidation
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostApi.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>>>{
    
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<Api2.PostApi2GroundNoHeader<PostAvatarHashesByUserIds>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(true));
    }
}