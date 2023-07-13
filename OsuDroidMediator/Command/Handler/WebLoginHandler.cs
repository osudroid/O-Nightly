using Mediator;
using OsuDroidMediator.Command.Request;
using OsuDroidMediator.Command.Response;
using OsuDroidMediator.Domain.Model;
using OsuDroidMediator.Domain.Model.Dto;

namespace OsuDroidMediator.Command.Handler; 

public class WebLoginHandler: 
        IRequestHandler<DtoRequest<OptionResponse<WebLoginResponseDto>,WebLoginDto>,
        DtoReponse<OptionResponse<WebLoginResponseDto>>> {
        public ValueTask<DtoReponse<OptionResponse<WebLoginResponseDto>>> Handle(
                DtoRequest<OptionResponse<WebLoginResponseDto>, WebLoginDto> request, CancellationToken cancellationToken) {
                

        }
}