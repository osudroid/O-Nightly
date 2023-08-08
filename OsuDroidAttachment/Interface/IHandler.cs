using LamLibAllOver;
using Microsoft.Extensions.Logging;

namespace OsuDroidAttachment.Interface; 

public interface IHandler<Db, Logger, Input, Output>
    where Db: IDb
    where Logger: OsuDroidAttachment.Interface.ILogger
    where Input: ITransformOutput
    where Output: IHandlerOutput {
    public ValueTask<Result<Output, string>> Handel(Db dbWrapper, Logger logger, Input request);
    
}