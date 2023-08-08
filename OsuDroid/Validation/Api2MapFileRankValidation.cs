using OsuDroid.Class;
using OsuDroid.Post;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;

namespace OsuDroid.Validation; 

public class Api2MapFileRankValidation 
    : IValidationHandler<NpgsqlCreates.DbWrapper, LogWrapper,ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>>>{
    
    public ValueTask<Result<bool, string>> Validate(NpgsqlCreates.DbWrapper db, LogWrapper logger, ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.ValuesAreGood()));
    }

    public ValueTask<Result<bool, string>> HashMatch(LogWrapper logger, ControllerPostWrapper<PostApi.PostApi2GroundWithHash<PostApi2MapFileRank>> input) {
        return ValueTask.FromResult(Result<bool, string>.Ok(input.Post.HashValidate()));
    }
}