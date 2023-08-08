using LamLibAllOver;
using OsuDroidAttachment.Interface;

namespace OsuDroidAttachment.Class; 

public record Transaction<T>(
    EModelResult Result, 
    Option<T> OptionResponse, 
    ResultErr<string> Err, string UserErrorMessage = "") {
    
    public static Transaction<T> Ok(T result) 
        => new(EModelResult.Ok , Option<T>.With(result), ResultErr<string>.Ok());
    public static Transaction<T> BadRequest() 
        => new(EModelResult.BadRequest, Option<T>.Empty, ResultErr<string>.Ok());
    public static Transaction<T> InternalServerError(ResultErr<string> resultErr) 
        => new(EModelResult.InternalServerError, Option<T>.Empty, resultErr);
    public static Transaction<T> HashNotMatch(string err) 
        => new(EModelResult.BadRequestWithMessage, Option<T>.Empty, ResultErr<string>.Ok(), err);
}