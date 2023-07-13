using LamLogger;
using Mediator;
using Npgsql;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Model;
using OsuDroid.View;

namespace OsuDroid.Lib.Handler; 



public class ValidationHandler<TView>: IRequestHandler<ValidationHandlerProp<TView>, ValidationHandlerResult<TView>> where TView: IView {
    
    public async ValueTask<ValidationHandlerResult<TView>> Handle(ValidationHandlerProp<TView> request, CancellationToken cancellationToken) {
        try {
            if (request.Value.ValuesAreGood() == false) {
                var model = ModelResult<TView>.BadRequest();
                return new ValidationHandlerResult<TView>(Result<ModelResult<TView>, string>.Ok(model));
            }
            
            // TODO GO ON
        }
        catch (Exception exception) {
            return new ValidationHandlerResult<TView>(Result<ModelResult<TView>, string>.Err(exception.ToString()));
        }
    }
}