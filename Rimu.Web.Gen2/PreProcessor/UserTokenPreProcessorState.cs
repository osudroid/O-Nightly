using LamLibAllOver.ErrorHandling;
using Rimu.Repository.Authentication.Adapter.Dto;

namespace Rimu.Web.Gen2.PreProcessor;

public class UserTokenPreProcessorState {
    public Option<TokenWithTTLDto> TokenWithTTLDto { get; set; } = Option<TokenWithTTLDto>.Empty;
}