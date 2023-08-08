using LamLibAllOver;

namespace OsuDroidAttachment.Interface; 

public interface IValidationHandler<TDb, ELogger, UInput> 
    where TDb : IDb
    where ELogger: ILogger
    where UInput: IInput {
    public ValueTask<Result<bool, string>> Validate(TDb db, ELogger logger, UInput input);

    public ValueTask<Result<bool, string>> HashMatch(ELogger logger, UInput input);
}