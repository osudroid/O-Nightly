using Mediator;
using Npgsql;
using OsuDroid.Extensions;
using OsuDroid.View;

namespace OsuDroid.Class; 

public record class ValidationHandlerProp<TView>(
    PostApi.IValuesAreGood Value,
    ControllerExtensions Controller,
    NpgsqlConnection Db
    ): IRequest<ValidationHandlerResult<TView>> where TView: IView;