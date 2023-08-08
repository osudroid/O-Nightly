using OsuDroid.View;
using OsuDroidAttachment.Class;

namespace OsuDroid.OutputHandler; 

public struct ViewImage<T>: OsuDroidAttachment.Interface.IOutputHandler<ImageWrapper, T> {
    public required Func<(byte[] Bytes, string Ext), T> Converter { get; init; }
    
    public ValueTask<Transaction<T>> Handel(Result<ImageWrapper, string> input) {
        if (input == EResult.Err) {
            return ValueTask.FromResult(Transaction<T>.InternalServerError(input)); 
        }
        
        var option = input.Ok();
        
        return ValueTask.FromResult(option.Option.IsSet()
            ? Transaction<T>.Ok(Converter(option.Option.Unwrap()))
            : Transaction<T>.BadRequest()
            );
    }
}