using LamLibAllOver;

namespace OsuDroidAttachment.Interface; 

public interface ITransformHandler<TInput, EOutput> 
    where TInput: IInput 
    where EOutput: ITransformOutput {
    public ValueTask<Result<EOutput, string>> Transform(TInput input);
}