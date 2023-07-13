using LamLibAllOver;
using Mediator;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Interface;
using OsuDroidMediator.Domain.Model;

namespace OsuDroidMediator.Command.Handler; 

public class ValidationHandler<T>: IRequestHandler<ValidationHandlerRequest<T>, ValidationHandlerResponse<T>> where T : IResponse {
    
    public async ValueTask<ValidationHandlerResponse<T>> Handle(ValidationHandlerRequest<T> request, CancellationToken cancellationToken) {
        try {
            if (!request.Data.DataValue.ValuesAreGood()) {
                await request.Logger.AddLogDebugAsync("Value Are Not Good");
                return new ValidationHandlerResponse<T>(Transaction<T>.BadRequest());
            }

            var dto = request.Data.ToDto();
            
            DtoReponse<T> sendResult = await Mediator.Service.Send(
                new DtoRequest<T, IDto>(dto, request.UserCookie, request.Logger, request.Db), 
                cancellationToken
            );

            return new ValidationHandlerResponse<T>(sendResult.Response);
        }
        catch (Exception e) {
            await request.Logger.AddLogErrorAsync(e.ToString(), Option<string>.With(e.StackTrace??""));
            return new ValidationHandlerResponse<T>(Transaction<T>.InternalServerError(ResultErr<string>.Err(e.ToString())));
        }
    }
}