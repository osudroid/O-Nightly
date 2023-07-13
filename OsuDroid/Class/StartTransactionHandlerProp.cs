using Mediator;
using OsuDroid.Class.Dto;
using OsuDroid.Extensions;
using OsuDroid.View;

namespace OsuDroid.Class; 

public record StartTransactionHandlerProp<TView>(
    PostApi.IValuesAreGood Value,
    ControllerExtensions Controller
):
    IRequest<StartTransactionHandlerResult<TView>> where TView: IView;