using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Validation; 

public class ValidationHandlerNothing<TDb, ELogger, UInput>: OsuDroidAttachment.Interface.IValidationHandler<TDb, ELogger, UInput> 
    where UInput : IInput 
    where ELogger : ILogger 
    where TDb : IDb {
    
    public ValueTask<Result<bool, string>> Validate(TDb db, ELogger logger, UInput input) 
        => new(Result<bool, string>.Ok(true));

    public ValueTask<Result<bool, string>> HashMatch(ELogger logger, UInput input) 
        => new(Result<bool, string>.Ok(true));
}